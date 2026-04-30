using FluentAssertions;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class ExtratoServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static Conta NovaConta(string nome = "Banco", decimal saldoInicial = 0m, string userId = UserId, ContaTipo tipo = ContaTipo.ContaCorrente) => new()
    {
        UserId = userId,
        Nome = nome,
        Tipo = tipo,
        Cor = "#000",
        SaldoInicial = saldoInicial
    };

    private static Lancamento NovoLancamento(Guid contaId, decimal valor, LancamentoTipo tipo, DateTime data, bool realizado = false, Guid? categoriaId = null, string descricao = "Teste") => new()
    {
        UserId = UserId,
        ContaId = contaId,
        CategoriaFinanceiraId = categoriaId,
        Descricao = descricao,
        Valor = valor,
        DataVencimento = data,
        DataPagamento = realizado ? data : null,
        Tipo = tipo,
        Status = realizado ? LancamentoStatus.Pago : LancamentoStatus.Pendente,
        Realizado = realizado
    };

    [Fact]
    public async Task ObterPorContaAsync_ContaInexistente_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterPorContaAsync(Guid.NewGuid(), DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task ObterPorContaAsync_DeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(userId: OutroUserId);
        test.Db.Contas.Add(conta);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterPorContaAsync(conta.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task ObterPorContaAsync_CartaoCredito_RetornaErro()
    {
        await using var test = new TestDbContext();
        var cartao = NovaConta(tipo: ContaTipo.CartaoCredito);
        cartao.DiaFechamento = 15; cartao.DiaVencimento = 25;
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterPorContaAsync(cartao.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("fatura");
    }

    [Fact]
    public async Task ObterPorContaAsync_DataFimAnterior_RetornaErro()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta();
        test.Db.Contas.Add(conta);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterPorContaAsync(conta.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ObterPorContaAsync_SemMovimentos_RetornaSaldoInicialComoFinal()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 1000m);
        test.Db.Contas.Add(conta);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc), UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SaldoInicial.Should().Be(1000m);
        result.Data.SaldoFinal.Should().Be(1000m);
        result.Data.Movimentos.Should().BeEmpty();
        result.Data.SaldosDiarios.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterPorContaAsync_BaixaDeReceberSomaNoSaldo()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 100m);
        test.Db.Contas.Add(conta);
        var lanc = NovoLancamento(conta.Id, 200m, LancamentoTipo.Receber, new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc));
        test.Db.Lancamentos.Add(lanc);
        await test.Db.SaveChangesAsync();
        test.Db.LancamentosBaixas.Add(new LancamentoBaixa
        {
            LancamentoId = lanc.Id,
            ContaId = conta.Id,
            Data = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Valor = 200m
        });
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SaldoInicial.Should().Be(100m);
        result.Data.SaldoFinal.Should().Be(300m);
        result.Data.TotalEntradas.Should().Be(200m);
        result.Data.TotalSaidas.Should().Be(0m);
        result.Data.Movimentos.Should().HaveCount(1);
        result.Data.Movimentos[0].Tipo.Should().Be(ExtratoMovimentoTipo.Entrada);
        result.Data.Movimentos[0].SaldoApos.Should().Be(300m);
    }

    [Fact]
    public async Task ObterPorContaAsync_BaixaDePagarSubtraiDoSaldo()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 500m);
        test.Db.Contas.Add(conta);
        var lanc = NovoLancamento(conta.Id, 100m, LancamentoTipo.Pagar, new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc));
        test.Db.Lancamentos.Add(lanc);
        await test.Db.SaveChangesAsync();
        test.Db.LancamentosBaixas.Add(new LancamentoBaixa { LancamentoId = lanc.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Valor = 100m });
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        result.Data!.SaldoFinal.Should().Be(400m);
        result.Data.TotalSaidas.Should().Be(100m);
        result.Data.Movimentos[0].Tipo.Should().Be(ExtratoMovimentoTipo.Saida);
    }

    [Fact]
    public async Task ObterPorContaAsync_SaldoAposEhCorrenteEmSequencia()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 0m);
        test.Db.Contas.Add(conta);
        var l1 = NovoLancamento(conta.Id, 1000m, LancamentoTipo.Receber, new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc));
        var l2 = NovoLancamento(conta.Id, 200m, LancamentoTipo.Pagar, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc));
        var l3 = NovoLancamento(conta.Id, 50m, LancamentoTipo.Pagar, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc));
        test.Db.Lancamentos.AddRange(l1, l2, l3);
        await test.Db.SaveChangesAsync();
        test.Db.LancamentosBaixas.AddRange(
            new LancamentoBaixa { LancamentoId = l1.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc), Valor = 1000m },
            new LancamentoBaixa { LancamentoId = l2.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Valor = 200m },
            new LancamentoBaixa { LancamentoId = l3.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), Valor = 50m });
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        var movs = result.Data!.Movimentos;
        movs.Should().HaveCount(3);
        movs[0].SaldoApos.Should().Be(1000m);
        movs[1].SaldoApos.Should().Be(800m);
        movs[2].SaldoApos.Should().Be(750m);
        result.Data.SaldoFinal.Should().Be(750m);
    }

    [Fact]
    public async Task ObterPorContaAsync_MovimentosAntesDoPeriodoNaoAparecemMasAcumulamSaldoInicial()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 100m);
        test.Db.Contas.Add(conta);
        var l1 = NovoLancamento(conta.Id, 50m, LancamentoTipo.Receber, new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc));
        var l2 = NovoLancamento(conta.Id, 20m, LancamentoTipo.Receber, new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc));
        test.Db.Lancamentos.AddRange(l1, l2);
        await test.Db.SaveChangesAsync();
        test.Db.LancamentosBaixas.AddRange(
            new LancamentoBaixa { LancamentoId = l1.Id, ContaId = conta.Id, Data = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc), Valor = 50m },
            new LancamentoBaixa { LancamentoId = l2.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc), Valor = 20m });
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        result.Data!.SaldoInicial.Should().Be(150m);
        result.Data.Movimentos.Should().HaveCount(1);
        result.Data.Movimentos[0].Valor.Should().Be(20m);
        result.Data.SaldoFinal.Should().Be(170m);
    }

    [Fact]
    public async Task ObterPorContaAsync_LancamentoRealizadoSemBaixaContaComoMovimento()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 500m);
        test.Db.Contas.Add(conta);
        var l = NovoLancamento(conta.Id, 100m, LancamentoTipo.Pagar, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), realizado: true);
        test.Db.Lancamentos.Add(l);
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        result.Data!.Movimentos.Should().HaveCount(1);
        result.Data.Movimentos[0].Origem.Should().Be(ExtratoMovimentoOrigem.Lancamento);
        result.Data.SaldoFinal.Should().Be(400m);
    }

    [Fact]
    public async Task ObterPorContaAsync_LancamentoComBaixaAtivaNaoDuplicaComoMovimento()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta();
        test.Db.Contas.Add(conta);
        var l = NovoLancamento(conta.Id, 100m, LancamentoTipo.Pagar, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), realizado: true);
        test.Db.Lancamentos.Add(l);
        await test.Db.SaveChangesAsync();
        test.Db.LancamentosBaixas.Add(new LancamentoBaixa { LancamentoId = l.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Valor = 100m });
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        result.Data!.Movimentos.Should().HaveCount(1);
        result.Data.Movimentos[0].Origem.Should().Be(ExtratoMovimentoOrigem.Baixa);
    }

    [Fact]
    public async Task ObterPorContaAsync_SaldosDiariosAgrupamPorDia()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 0m);
        test.Db.Contas.Add(conta);
        var l1 = NovoLancamento(conta.Id, 100m, LancamentoTipo.Receber, new DateTime(2026, 5, 5));
        var l2 = NovoLancamento(conta.Id, 50m, LancamentoTipo.Pagar, new DateTime(2026, 5, 5));
        var l3 = NovoLancamento(conta.Id, 200m, LancamentoTipo.Receber, new DateTime(2026, 5, 6));
        test.Db.Lancamentos.AddRange(l1, l2, l3);
        await test.Db.SaveChangesAsync();
        test.Db.LancamentosBaixas.AddRange(
            new LancamentoBaixa { LancamentoId = l1.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc), Valor = 100m },
            new LancamentoBaixa { LancamentoId = l2.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 5, 15, 0, 0, DateTimeKind.Utc), Valor = 50m },
            new LancamentoBaixa { LancamentoId = l3.Id, ContaId = conta.Id, Data = new DateTime(2026, 5, 6, 10, 0, 0, DateTimeKind.Utc), Valor = 200m });
        await test.Db.SaveChangesAsync();

        var service = new ExtratoService(test.Db);
        var result = await service.ObterPorContaAsync(conta.Id, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), UserId);

        result.Data!.SaldosDiarios.Should().HaveCount(2);
        result.Data.SaldosDiarios[0].Data.Should().Be(new DateTime(2026, 5, 5));
        result.Data.SaldosDiarios[0].Entradas.Should().Be(100m);
        result.Data.SaldosDiarios[0].Saidas.Should().Be(50m);
        result.Data.SaldosDiarios[0].SaldoFinalDia.Should().Be(50m);
        result.Data.SaldosDiarios[1].Data.Should().Be(new DateTime(2026, 5, 6));
        result.Data.SaldosDiarios[1].SaldoFinalDia.Should().Be(250m);
    }

    [Fact]
    public async Task ObterConsolidadoAsync_SomaSaldosDeMultiplasContas()
    {
        await using var test = new TestDbContext();
        var c1 = NovaConta("Banco A", saldoInicial: 100m);
        var c2 = NovaConta("Banco B", saldoInicial: 200m);
        test.Db.Contas.AddRange(c1, c2);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterConsolidadoAsync(new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), null, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SaldoInicial.Should().Be(300m);
        result.Data.SaldoFinal.Should().Be(300m);
    }

    [Fact]
    public async Task ObterConsolidadoAsync_FiltraPorContaIds()
    {
        await using var test = new TestDbContext();
        var c1 = NovaConta("A", saldoInicial: 100m);
        var c2 = NovaConta("B", saldoInicial: 999m);
        test.Db.Contas.AddRange(c1, c2);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterConsolidadoAsync(new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), new[] { c1.Id }, UserId);

        result.Data!.SaldoInicial.Should().Be(100m);
    }

    [Fact]
    public async Task ObterConsolidadoAsync_NaoIncluiCartoesDeCredito()
    {
        await using var test = new TestDbContext();
        var conta = NovaConta(saldoInicial: 500m);
        var cartao = NovaConta("Cartão", tipo: ContaTipo.CartaoCredito);
        cartao.DiaFechamento = 15; cartao.DiaVencimento = 25; cartao.SaldoInicial = 0m;
        test.Db.Contas.AddRange(conta, cartao);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterConsolidadoAsync(new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), null, UserId);

        result.Data!.ContaIds.Should().HaveCount(1);
        result.Data.ContaIds.Should().Contain(conta.Id);
    }

    [Fact]
    public async Task ObterConsolidadoAsync_DeOutroUser_NaoInclui()
    {
        await using var test = new TestDbContext();
        var minha = NovaConta("Minha", saldoInicial: 100m);
        var outra = NovaConta("Outra", saldoInicial: 999m, userId: OutroUserId);
        test.Db.Contas.AddRange(minha, outra);
        await test.Db.SaveChangesAsync();
        var service = new ExtratoService(test.Db);

        var result = await service.ObterConsolidadoAsync(new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), null, UserId);

        result.Data!.SaldoInicial.Should().Be(100m);
    }
}
