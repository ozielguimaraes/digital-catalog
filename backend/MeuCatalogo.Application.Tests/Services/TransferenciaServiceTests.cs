using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class TransferenciaServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static Conta NovaContaCorrente(string nome, string userId = UserId) => new()
    {
        UserId = userId,
        Nome = nome,
        Tipo = ContaTipo.ContaCorrente,
        Cor = "#000"
    };

    private static Conta NovoCartao(string nome, string userId = UserId) => new()
    {
        UserId = userId,
        Nome = nome,
        Tipo = ContaTipo.CartaoCredito,
        Cor = "#000",
        DiaFechamento = 15,
        DiaVencimento = 25,
        Limite = 5000m
    };

    private static (TransferenciaService svc, ApplicationDbContext) NovoService(TestDbContext test)
    {
        var faturaSvc = new FaturaService(test.Db);
        return (new TransferenciaService(test.Db, faturaSvc), test.Db);
    }

    [Fact]
    public async Task CriarEntreContasAsync_GeraDoisLancamentosVinculados()
    {
        await using var test = new TestDbContext();
        var origem = NovaContaCorrente("Banco A");
        var destino = NovaContaCorrente("Banco B");
        test.Db.Contas.AddRange(origem, destino);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        var result = await service.CriarEntreContasAsync(new TransferenciaRequest
        {
            Data = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            ContaOrigemId = origem.Id,
            ContaDestinoId = destino.Id,
            Valor = 500m
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Type.Should().Be(ResponseType.Created);

        test.Db.Lancamentos.Should().HaveCount(2);
        var lancs = test.Db.Lancamentos.ToList();
        lancs.Should().Contain(l => l.Tipo == LancamentoTipo.Pagar && l.ContaId == origem.Id);
        lancs.Should().Contain(l => l.Tipo == LancamentoTipo.Receber && l.ContaId == destino.Id);
        lancs.Should().AllSatisfy(l =>
        {
            l.Operacao.Should().Be(LancamentoOperacao.Transferencia);
            l.TipoTransferencia.Should().Be(LancamentoTipoTransferencia.EntreContas);
            l.Realizado.Should().BeTrue();
            l.LancamentoTransferenciaId.Should().NotBeNull();
        });
        var pagar = lancs.First(l => l.Tipo == LancamentoTipo.Pagar);
        var receber = lancs.First(l => l.Tipo == LancamentoTipo.Receber);
        pagar.LancamentoTransferenciaId.Should().Be(receber.Id);
        receber.LancamentoTransferenciaId.Should().Be(pagar.Id);
    }

    [Fact]
    public async Task CriarEntreContasAsync_OrigemEDestinoIguais_RetornaErro()
    {
        await using var test = new TestDbContext();
        var conta = NovaContaCorrente("X");
        test.Db.Contas.Add(conta);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        var result = await service.CriarEntreContasAsync(new TransferenciaRequest
        {
            Data = DateTime.UtcNow,
            ContaOrigemId = conta.Id,
            ContaDestinoId = conta.Id,
            Valor = 50m
        }, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CriarEntreContasAsync_OrigemCartao_RetornaErro()
    {
        await using var test = new TestDbContext();
        var origem = NovoCartao("Cartão");
        var destino = NovaContaCorrente("Banco");
        test.Db.Contas.AddRange(origem, destino);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        var result = await service.CriarEntreContasAsync(new TransferenciaRequest
        {
            Data = DateTime.UtcNow,
            ContaOrigemId = origem.Id,
            ContaDestinoId = destino.Id,
            Valor = 50m
        }, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CriarEntreContasAsync_ContaDeOutroUser_RetornaErro()
    {
        await using var test = new TestDbContext();
        var origem = NovaContaCorrente("A", OutroUserId);
        var destino = NovaContaCorrente("B");
        test.Db.Contas.AddRange(origem, destino);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        var result = await service.CriarEntreContasAsync(new TransferenciaRequest
        {
            Data = DateTime.UtcNow,
            ContaOrigemId = origem.Id,
            ContaDestinoId = destino.Id,
            Valor = 50m
        }, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CriarPagamentoFaturaAsync_AtualizaValorPagoDaFatura()
    {
        await using var test = new TestDbContext();
        var conta = NovaContaCorrente("Banco");
        var cartao = NovoCartao("Cartão");
        test.Db.Contas.AddRange(conta, cartao);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        var result = await service.CriarPagamentoFaturaAsync(new TransferenciaRequest
        {
            Data = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc),
            ContaOrigemId = conta.Id,
            ContaDestinoId = cartao.Id,
            Valor = 200m,
            FaturaMes = 5,
            FaturaAno = 2026
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        var fatura = test.Db.Faturas.Single();
        fatura.ValorPago.Should().Be(200m);
        var lancs = test.Db.Lancamentos.ToList();
        lancs.Should().HaveCount(2);
        lancs.Should().AllSatisfy(l => l.FaturaId.Should().Be(fatura.Id));
    }

    [Fact]
    public async Task CriarPagamentoFaturaAsync_DestinoNaoEhCartao_RetornaErro()
    {
        await using var test = new TestDbContext();
        var origem = NovaContaCorrente("A");
        var destino = NovaContaCorrente("B");
        test.Db.Contas.AddRange(origem, destino);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        var result = await service.CriarPagamentoFaturaAsync(new TransferenciaRequest
        {
            Data = DateTime.UtcNow,
            ContaOrigemId = origem.Id,
            ContaDestinoId = destino.Id,
            Valor = 100m,
            FaturaMes = 5,
            FaturaAno = 2026
        }, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExcluirAsync_RemoveOsDoisLancamentosEDecrementaFatura()
    {
        await using var test = new TestDbContext();
        var conta = NovaContaCorrente("Banco");
        var cartao = NovoCartao("Cartão");
        test.Db.Contas.AddRange(conta, cartao);
        await test.Db.SaveChangesAsync();

        var (service, _) = NovoService(test);
        await service.CriarPagamentoFaturaAsync(new TransferenciaRequest
        {
            Data = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc),
            ContaOrigemId = conta.Id,
            ContaDestinoId = cartao.Id,
            Valor = 200m,
            FaturaMes = 5,
            FaturaAno = 2026
        }, UserId);

        var saida = test.Db.Lancamentos.First(l => l.Tipo == LancamentoTipo.Pagar);

        var del = await service.ExcluirAsync(saida.Id, UserId);

        del.IsSuccess.Should().BeTrue();
        test.Db.Lancamentos.Should().BeEmpty();
        test.Db.Faturas.Single().ValorPago.Should().Be(0m);
    }
}
