using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class LancamentoBaixaServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static (Lancamento lanc, Conta conta) Cadastros(TestDbContext test, decimal valor = 1000m, string userId = UserId)
    {
        var conta = new Conta { UserId = userId, Nome = "Banco", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        test.Db.Contas.Add(conta);
        var lanc = new Lancamento
        {
            UserId = userId,
            ContaId = conta.Id,
            Descricao = "Aluguel",
            Valor = valor,
            DataVencimento = DateTime.UtcNow.AddDays(5),
            Tipo = LancamentoTipo.Pagar,
            Status = LancamentoStatus.Pendente
        };
        test.Db.Lancamentos.Add(lanc);
        test.Db.SaveChanges();
        return (lanc, conta);
    }

    [Fact]
    public async Task AdicionarAsync_BaixaParcial_StatusVaiParaParcial()
    {
        await using var test = new TestDbContext();
        var (lanc, conta) = Cadastros(test);
        var service = new LancamentoBaixaService(test.Db);

        var result = await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest
        {
            Data = DateTime.UtcNow,
            Valor = 400m,
            ContaId = conta.Id
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        var reload = await test.Db.Lancamentos.FindAsync(lanc.Id);
        reload!.Status.Should().Be(LancamentoStatus.Parcial);
        reload.Realizado.Should().BeFalse();
    }

    [Fact]
    public async Task AdicionarAsync_BaixaTotal_StatusVaiParaPago()
    {
        await using var test = new TestDbContext();
        var (lanc, conta) = Cadastros(test, valor: 500m);
        var service = new LancamentoBaixaService(test.Db);

        await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest
        {
            Data = DateTime.UtcNow,
            Valor = 500m,
            ContaId = conta.Id
        }, UserId);

        var reload = await test.Db.Lancamentos.FindAsync(lanc.Id);
        reload!.Status.Should().Be(LancamentoStatus.Pago);
        reload.Realizado.Should().BeTrue();
        reload.DataPagamento.Should().NotBeNull();
    }

    [Fact]
    public async Task AdicionarAsync_DuasParciaisQueZeram_StatusFica_Pago()
    {
        await using var test = new TestDbContext();
        var (lanc, conta) = Cadastros(test, valor: 100m);
        var service = new LancamentoBaixaService(test.Db);

        await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest { Data = DateTime.UtcNow, Valor = 60m, ContaId = conta.Id }, UserId);
        await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest { Data = DateTime.UtcNow, Valor = 40m, ContaId = conta.Id }, UserId);

        var reload = await test.Db.Lancamentos.FindAsync(lanc.Id);
        reload!.Status.Should().Be(LancamentoStatus.Pago);
    }

    [Fact]
    public async Task AdicionarAsync_ValorAcimaDoSaldo_RetornaErro()
    {
        await using var test = new TestDbContext();
        var (lanc, conta) = Cadastros(test, valor: 100m);
        var service = new LancamentoBaixaService(test.Db);

        var result = await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest
        {
            Data = DateTime.UtcNow,
            Valor = 200m,
            ContaId = conta.Id
        }, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("excede");
    }

    [Fact]
    public async Task AdicionarAsync_LancamentoDeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var (lanc, _) = Cadastros(test, userId: OutroUserId);
        var conta = new Conta { UserId = UserId, Nome = "Minha", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        test.Db.Contas.Add(conta);
        await test.Db.SaveChangesAsync();
        var service = new LancamentoBaixaService(test.Db);

        var result = await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest
        {
            Data = DateTime.UtcNow,
            Valor = 50m,
            ContaId = conta.Id
        }, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task RemoverAsync_VoltaStatusParaPendente()
    {
        await using var test = new TestDbContext();
        var (lanc, conta) = Cadastros(test, valor: 200m);
        var service = new LancamentoBaixaService(test.Db);
        var b = await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest
        {
            Data = DateTime.UtcNow,
            Valor = 200m,
            ContaId = conta.Id
        }, UserId);

        var del = await service.RemoverAsync(lanc.Id, b.Data!.Id, UserId);

        del.IsSuccess.Should().BeTrue();
        var reload = await test.Db.Lancamentos.FindAsync(lanc.Id);
        reload!.Status.Should().BeOneOf(LancamentoStatus.Pendente, LancamentoStatus.Atrasado);
        reload.Realizado.Should().BeFalse();
    }

    [Fact]
    public async Task ListarAsync_RetornaBaixasDoLancamento()
    {
        await using var test = new TestDbContext();
        var (lanc, conta) = Cadastros(test, valor: 300m);
        var service = new LancamentoBaixaService(test.Db);
        await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest { Data = DateTime.UtcNow, Valor = 100m, ContaId = conta.Id }, UserId);
        await service.AdicionarAsync(lanc.Id, new LancamentoBaixaRequest { Data = DateTime.UtcNow.AddDays(1), Valor = 50m, ContaId = conta.Id }, UserId);

        var result = await service.ListarAsync(lanc.Id, UserId);

        result.Data.Should().HaveCount(2);
    }
}
