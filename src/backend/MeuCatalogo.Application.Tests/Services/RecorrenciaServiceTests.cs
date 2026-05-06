using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class RecorrenciaServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static (Conta conta, CategoriaFinanceira cat) Cadastros(TestDbContext test, string userId = UserId)
    {
        var conta = new Conta { UserId = userId, Nome = "Banco", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var cat = new CategoriaFinanceira { UserId = userId, Nome = "Casa", Tipo = CategoriaFinanceiraTipo.Despesa, Cor = "#000", IconeNome = "home" };
        test.Db.Contas.Add(conta);
        test.Db.CategoriasFinanceiras.Add(cat);
        test.Db.SaveChanges();
        return (conta, cat);
    }

    [Fact]
    public async Task CreateAsync_RecorrenciaMensalValorFixo()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);

        var result = await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Internet",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = 100m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc)
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Type.Should().Be(ResponseType.Created);
        result.Data!.ValorPadrao.Should().Be(100m);
        result.Data.ProximaData.Should().Be(new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task CreateAsync_ValorPadraoNull_PermiteRecorrenciaVariavel()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);

        var result = await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Luz",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = null,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 15,
            DataInicio = DateTime.UtcNow
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.ValorPadrao.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValorPadraoNegativo_RetornaErro()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);

        var result = await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "x",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = -1m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = DateTime.UtcNow
        }, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ContaDeOutroUser_RetornaErro()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test, OutroUserId);
        var service = new RecorrenciaService(test.Db);

        var result = await service.CreateAsync(new RecorrenciaRequest
        {
            Descricao = "x",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = DateTime.UtcNow
        }, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GerarPendentesAsync_MaterializaLancamentosAteData()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);
        await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Internet",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = 100m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
        }, UserId);

        var gerado = await service.GerarPendentesAsync(UserId, new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc));

        gerado.Should().Be(5);
        test.Db.Lancamentos.Should().HaveCount(5);
        test.Db.Lancamentos.Should().AllSatisfy(l =>
        {
            l.Realizado.Should().BeFalse();
            l.Status.Should().Be(LancamentoStatus.Pendente);
            l.Valor.Should().Be(100m);
            l.RecorrenciaId.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task GerarPendentesAsync_EhIdempotente()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);
        await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Internet",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = 100m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
        }, UserId);

        await service.GerarPendentesAsync(UserId, new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc));
        await service.GerarPendentesAsync(UserId, new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc));

        test.Db.Lancamentos.Should().HaveCount(3);
    }

    [Fact]
    public async Task GerarPendentesAsync_ValorPadraoNull_GeraComZero()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);
        await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Luz",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = null,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 15,
            DataInicio = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        }, UserId);

        await service.GerarPendentesAsync(UserId, new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc));

        test.Db.Lancamentos.Should().HaveCount(2);
        test.Db.Lancamentos.Should().AllSatisfy(l =>
        {
            l.Valor.Should().Be(0m);
            l.Realizado.Should().BeFalse();
        });
    }

    [Fact]
    public async Task GerarPendentesAsync_RespeitaDataFim()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);
        await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Internet",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = 100m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            DataFim = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc)
        }, UserId);

        await service.GerarPendentesAsync(UserId, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc));

        test.Db.Lancamentos.Should().HaveCount(3);
    }

    [Fact]
    public async Task GerarPendentesAsync_PulaInativas()
    {
        await using var test = new TestDbContext();
        var (conta, cat) = Cadastros(test);
        var service = new RecorrenciaService(test.Db);
        var rec = await service.CreateAsync(new RecorrenciaRequest
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Internet",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = 100m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
        }, UserId);
        await service.SetAtivoAsync(rec.Data!.Id, UserId, false);

        await service.GerarPendentesAsync(UserId, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc));

        test.Db.Lancamentos.Should().BeEmpty();
    }
}
