using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class ContaServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static ContaRequest NovoCartaoRequest(
        string nome = "Nubank",
        byte diaFechamento = 15,
        byte diaVencimento = 25,
        decimal? limite = 1000m) => new()
    {
        Nome = nome,
        Tipo = ContaTipo.CartaoCredito,
        Cor = "#820AD1",
        DiaFechamento = diaFechamento,
        DiaVencimento = diaVencimento,
        Limite = limite
    };

    private static ContaRequest NovaContaCorrenteRequest(string nome = "Itaú") => new()
    {
        Nome = nome,
        Tipo = ContaTipo.ContaCorrente,
        Cor = "#FF6F00"
    };

    [Fact]
    public async Task CreateAsync_ContaCorrente_PersisteSemCamposCartao()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);

        var result = await service.CreateAsync(NovaContaCorrenteRequest(), UserId);

        result.IsSuccess.Should().BeTrue();
        result.Type.Should().Be(ResponseType.Created);

        var conta = test.Db.Contas.Single();
        conta.Tipo.Should().Be(ContaTipo.ContaCorrente);
        conta.DiaFechamento.Should().BeNull();
        conta.DiaVencimento.Should().BeNull();
        conta.Limite.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CartaoCredito_PersisteCampos()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);

        var result = await service.CreateAsync(NovoCartaoRequest(), UserId);

        result.IsSuccess.Should().BeTrue();
        var conta = test.Db.Contas.Single();
        conta.DiaFechamento.Should().Be(15);
        conta.DiaVencimento.Should().Be(25);
        conta.Limite.Should().Be(1000m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_NomeVazio_RetornaErro(string nome)
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);

        var result = await service.CreateAsync(NovaContaCorrenteRequest(nome), UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Nome é obrigatório");
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(32, 25)]
    [InlineData(15, 0)]
    [InlineData(15, 32)]
    public async Task CreateAsync_CartaoComDiasInvalidos_RetornaErro(byte diaFech, byte diaVenc)
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);

        var result = await service.CreateAsync(NovoCartaoRequest(diaFechamento: diaFech, diaVencimento: diaVenc), UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_RetornaApenasContasDoUser()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        await service.CreateAsync(NovaContaCorrenteRequest("a"), UserId);
        await service.CreateAsync(NovaContaCorrenteRequest("b"), OutroUserId);

        var result = await service.GetAllAsync(UserId);

        result.Data.Should().HaveCount(1);
        result.Data![0].Nome.Should().Be("a");
    }

    [Fact]
    public async Task GetAllAsync_FiltraInativasPorPadrao()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovaContaCorrenteRequest(), UserId);
        await service.SetAtivoAsync(c.Data!.Id, UserId, false);

        var ativas = await service.GetAllAsync(UserId);
        var todas = await service.GetAllAsync(UserId, incluirInativas: true);

        ativas.Data.Should().BeEmpty();
        todas.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_AlterarTipo_ProibidoMesmoContaSemUso()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovaContaCorrenteRequest(), UserId);

        var result = await service.UpdateAsync(c.Data!.Id, NovoCartaoRequest("alterada"), UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("tipo");
    }

    [Fact]
    public async Task UpdateAsync_DeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovaContaCorrenteRequest(), OutroUserId);

        var result = await service.UpdateAsync(c.Data!.Id, NovaContaCorrenteRequest("x"), UserId);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_AlterarDiaVencimento_RecalculaFaturasFuturasNaoPagas()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovoCartaoRequest(diaFechamento: 15, diaVencimento: 25), UserId);

        var contaId = c.Data!.Id;
        var hoje = DateTime.UtcNow.Date;
        var futuraNaoPaga = new Fatura
        {
            ContaId = contaId,
            Mes = hoje.AddMonths(2).Month,
            Ano = hoje.AddMonths(2).Year,
            DataInicio = hoje.AddMonths(1),
            DataFim = hoje.AddMonths(2).AddDays(-1),
            DataVencimento = hoje.AddMonths(2),
            ValorPago = null
        };
        var futuraPaga = new Fatura
        {
            ContaId = contaId,
            Mes = hoje.AddMonths(3).Month,
            Ano = hoje.AddMonths(3).Year,
            DataInicio = hoje.AddMonths(2),
            DataFim = hoje.AddMonths(3).AddDays(-1),
            DataVencimento = hoje.AddMonths(3),
            ValorPago = 100m
        };
        var passadaNaoPaga = new Fatura
        {
            ContaId = contaId,
            Mes = hoje.AddMonths(-1).Month,
            Ano = hoje.AddMonths(-1).Year,
            DataInicio = hoje.AddMonths(-2),
            DataFim = hoje.AddMonths(-1).AddDays(-1),
            DataVencimento = hoje.AddMonths(-1),
            ValorPago = null
        };
        test.Db.Faturas.AddRange(futuraNaoPaga, futuraPaga, passadaNaoPaga);
        await test.Db.SaveChangesAsync();

        var venciOriginalPaga = futuraPaga.DataVencimento;
        var venciOriginalPassada = passadaNaoPaga.DataVencimento;

        var alterado = NovoCartaoRequest(diaFechamento: 10, diaVencimento: 5);
        var result = await service.UpdateAsync(contaId, alterado, UserId);
        result.IsSuccess.Should().BeTrue();

        var futuraReload = await test.Db.Faturas.FindAsync(futuraNaoPaga.Id);
        var pagaReload = await test.Db.Faturas.FindAsync(futuraPaga.Id);
        var passadaReload = await test.Db.Faturas.FindAsync(passadaNaoPaga.Id);

        futuraReload!.DataVencimento.Day.Should().Be(5);
        pagaReload!.DataVencimento.Should().Be(venciOriginalPaga);
        passadaReload!.DataVencimento.Should().Be(venciOriginalPassada);
    }

    [Fact]
    public async Task CreateAsync_ContaCorrente_PersisteSaldoInicial()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var req = NovaContaCorrenteRequest();
        req.SaldoInicial = 1500.50m;

        var result = await service.CreateAsync(req, UserId);

        result.IsSuccess.Should().BeTrue();
        var conta = test.Db.Contas.Single();
        conta.SaldoInicial.Should().Be(1500.50m);
    }

    [Fact]
    public async Task CreateAsync_ContaCorrente_AceitaSaldoInicialNegativo()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var req = NovaContaCorrenteRequest();
        req.SaldoInicial = -200m;

        var result = await service.CreateAsync(req, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.Contas.Single().SaldoInicial.Should().Be(-200m);
    }

    [Fact]
    public async Task CreateAsync_CartaoCredito_IgnoraSaldoInicial()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var req = NovoCartaoRequest();
        req.SaldoInicial = 999m;

        var result = await service.CreateAsync(req, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.Contas.Single().SaldoInicial.Should().Be(0m);
    }

    [Fact]
    public async Task UpdateAsync_AtualizaSaldoInicialDeContaNaoCartao()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovaContaCorrenteRequest(), UserId);

        var alterado = NovaContaCorrenteRequest("Itaú alterado");
        alterado.SaldoInicial = 250m;

        await service.UpdateAsync(c.Data!.Id, alterado, UserId);

        var reload = await test.Db.Contas.FindAsync(c.Data.Id);
        reload!.SaldoInicial.Should().Be(250m);
    }

    [Fact]
    public async Task DeleteAsync_SemLancamentos_RemoveFisicamente()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovaContaCorrenteRequest(), UserId);

        var result = await service.DeleteAsync(c.Data!.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.Contas.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ComLancamentos_ApenasInativa()
    {
        await using var test = new TestDbContext();
        var service = new ContaService(test.Db);
        var c = await service.CreateAsync(NovaContaCorrenteRequest(), UserId);
        test.Db.Lancamentos.Add(new Lancamento
        {
            UserId = UserId,
            ContaId = c.Data!.Id,
            Descricao = "x",
            Valor = 10m,
            DataVencimento = DateTime.UtcNow,
            Tipo = LancamentoTipo.Pagar
        });
        await test.Db.SaveChangesAsync();

        var result = await service.DeleteAsync(c.Data.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        var conta = test.Db.Contas.Single();
        conta.Ativo.Should().BeFalse();
    }
}
