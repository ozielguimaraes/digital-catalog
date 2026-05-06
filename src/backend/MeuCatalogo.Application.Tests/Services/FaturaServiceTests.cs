using FluentAssertions;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class FaturaServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static Conta NovoCartao(string userId = UserId, byte fech = 15, byte venc = 25) => new()
    {
        UserId = userId,
        Nome = "Cartão",
        Tipo = ContaTipo.CartaoCredito,
        Cor = "#000000",
        DiaFechamento = fech,
        DiaVencimento = venc,
        Limite = 5000m
    };

    [Fact]
    public async Task ObterOuCriarAsync_QuandoNaoExiste_CriaFaturaCalculada()
    {
        await using var test = new TestDbContext();
        var cartao = NovoCartao();
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();

        var service = new FaturaService(test.Db);
        var fatura = await service.ObterOuCriarAsync(cartao, mes: 5, ano: 2026);

        fatura.Should().NotBeNull();
        fatura.Mes.Should().Be(5);
        fatura.Ano.Should().Be(2026);
        fatura.DataVencimento.Day.Should().Be(25);
        fatura.DataFim.Day.Should().Be(15);
    }

    [Fact]
    public async Task ObterOuCriarAsync_QuandoJaExiste_RetornaExistente()
    {
        await using var test = new TestDbContext();
        var cartao = NovoCartao();
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();

        var service = new FaturaService(test.Db);
        var primeira = await service.ObterOuCriarAsync(cartao, 5, 2026);
        var segunda = await service.ObterOuCriarAsync(cartao, 5, 2026);

        primeira.Id.Should().Be(segunda.Id);
        test.Db.Faturas.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterOuCriarAsync_ContaNaoCartao_LancaExcecao()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "x", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var service = new FaturaService(test.Db);

        var act = async () => await service.ObterOuCriarAsync(conta, 5, 2026);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetAsync_DeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var cartao = NovoCartao(userId: OutroUserId);
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();

        var service = new FaturaService(test.Db);
        var result = await service.GetAsync(cartao.Id, 5, 2026, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task GetAsync_ContaNaoCartao_RetornaErro()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "x", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        test.Db.Contas.Add(conta);
        await test.Db.SaveChangesAsync();

        var service = new FaturaService(test.Db);
        var result = await service.GetAsync(conta.Id, 5, 2026, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cartões");
    }

    [Fact]
    public async Task GetAsync_AgrupaLancamentosComValorTotalEEmAberto()
    {
        await using var test = new TestDbContext();
        var cartao = NovoCartao();
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();

        var service = new FaturaService(test.Db);
        var fatura = await service.ObterOuCriarAsync(cartao, 5, 2026);

        test.Db.Lancamentos.AddRange(
            new Lancamento { UserId = UserId, ContaId = cartao.Id, Descricao = "A", Valor = 100m, DataVencimento = fatura.DataVencimento, Tipo = LancamentoTipo.Pagar, FaturaId = fatura.Id },
            new Lancamento { UserId = UserId, ContaId = cartao.Id, Descricao = "B", Valor = 50m, DataVencimento = fatura.DataVencimento, Tipo = LancamentoTipo.Pagar, FaturaId = fatura.Id });
        fatura.ValorPago = 30m;
        await test.Db.SaveChangesAsync();

        var result = await service.GetAsync(cartao.Id, 5, 2026, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.ValorTotal.Should().Be(150m);
        result.Data.ValorPago.Should().Be(30m);
        result.Data.ValorEmAberto.Should().Be(120m);
        result.Data.Quitada.Should().BeFalse();
        result.Data.Lancamentos.Should().HaveCount(2);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_AcumulaValorPago()
    {
        await using var test = new TestDbContext();
        var cartao = NovoCartao();
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();

        var service = new FaturaService(test.Db);
        var fatura = await service.ObterOuCriarAsync(cartao, 5, 2026);

        await service.RegistrarPagamentoAsync(fatura.Id, 50m, UserId);
        await service.RegistrarPagamentoAsync(fatura.Id, 30m, UserId);

        var reload = await test.Db.Faturas.FindAsync(fatura.Id);
        reload!.ValorPago.Should().Be(80m);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_DeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var cartao = NovoCartao(userId: OutroUserId);
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();
        var service = new FaturaService(test.Db);
        var fatura = await service.ObterOuCriarAsync(cartao, 5, 2026);

        var result = await service.RegistrarPagamentoAsync(fatura.Id, 50m, UserId);

        result.IsSuccess.Should().BeFalse();
    }
}
