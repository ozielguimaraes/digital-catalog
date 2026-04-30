using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class RelatorioFinanceiroServiceTests
{
    private const string UserId = "user-1";

    [Fact]
    public async Task RegimeCompetencia_AgrupaPorDataVencimento()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "Banco", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var cat = new CategoriaFinanceira { UserId = UserId, Nome = "Casa", Tipo = CategoriaFinanceiraTipo.Despesa, IconeNome = "home", Cor = "#000" };
        test.Db.Contas.Add(conta);
        test.Db.CategoriasFinanceiras.Add(cat);
        await test.Db.SaveChangesAsync();

        test.Db.Lancamentos.Add(new Lancamento
        {
            UserId = UserId,
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            Descricao = "Aluguel",
            Valor = 100m,
            DataVencimento = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Tipo = LancamentoTipo.Pagar
        });
        await test.Db.SaveChangesAsync();

        var service = new RelatorioFinanceiroService(test.Db);
        var result = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        {
            Ano = 2026,
            Mes = 5,
            Quantidade = 1,
            Regime = RegimeContabil.Competencia
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TotalDespesas.Should().Be(100m);
        result.Data.Despesas.Should().HaveCount(1);
        result.Data.Despesas[0].Nome.Should().Be("Casa");
    }

    [Fact]
    public async Task RegimeCaixa_Cartao_AgrupaPorVencimentoDaFatura()
    {
        await using var test = new TestDbContext();
        var cartao = new Conta
        {
            UserId = UserId,
            Nome = "Cartão",
            Tipo = ContaTipo.CartaoCredito,
            DiaFechamento = 15,
            DiaVencimento = 25,
            Cor = "#000"
        };
        var cat = new CategoriaFinanceira { UserId = UserId, Nome = "Compras", Tipo = CategoriaFinanceiraTipo.Despesa, IconeNome = "cart", Cor = "#000" };
        test.Db.Contas.Add(cartao);
        test.Db.CategoriasFinanceiras.Add(cat);
        var fatura = new Fatura
        {
            ContaId = cartao.Id,
            Mes = 6,
            Ano = 2026,
            DataInicio = new DateTime(2026, 5, 16, 0, 0, 0, DateTimeKind.Utc),
            DataFim = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            DataVencimento = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc)
        };
        test.Db.Faturas.Add(fatura);
        await test.Db.SaveChangesAsync();

        test.Db.Lancamentos.Add(new Lancamento
        {
            UserId = UserId,
            ContaId = cartao.Id,
            CategoriaFinanceiraId = cat.Id,
            FaturaId = fatura.Id,
            Descricao = "Compra X",
            Valor = 200m,
            DataVencimento = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Tipo = LancamentoTipo.Pagar
        });
        await test.Db.SaveChangesAsync();

        var service = new RelatorioFinanceiroService(test.Db);
        var maio = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        { Ano = 2026, Mes = 5, Quantidade = 1, Regime = RegimeContabil.Caixa }, UserId);
        var junho = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        { Ano = 2026, Mes = 6, Quantidade = 1, Regime = RegimeContabil.Caixa }, UserId);

        maio.Data!.TotalDespesas.Should().Be(0m);
        junho.Data!.TotalDespesas.Should().Be(200m);
    }

    [Fact]
    public async Task RegimeCaixa_ContaCorrente_AgrupaPorBaixa()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "Banco", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var cat = new CategoriaFinanceira { UserId = UserId, Nome = "Diversos", Tipo = CategoriaFinanceiraTipo.Despesa, IconeNome = "tag", Cor = "#000" };
        test.Db.Contas.Add(conta);
        test.Db.CategoriasFinanceiras.Add(cat);
        await test.Db.SaveChangesAsync();

        var lanc = new Lancamento
        {
            UserId = UserId,
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            Descricao = "Despesa",
            Valor = 300m,
            DataVencimento = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            Tipo = LancamentoTipo.Pagar
        };
        test.Db.Lancamentos.Add(lanc);
        await test.Db.SaveChangesAsync();

        test.Db.LancamentosBaixas.Add(new LancamentoBaixa
        {
            LancamentoId = lanc.Id,
            ContaId = conta.Id,
            Data = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc),
            Valor = 300m
        });
        await test.Db.SaveChangesAsync();

        var service = new RelatorioFinanceiroService(test.Db);
        var maio = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        { Ano = 2026, Mes = 5, Quantidade = 1, Regime = RegimeContabil.Caixa }, UserId);
        var junho = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        { Ano = 2026, Mes = 6, Quantidade = 1, Regime = RegimeContabil.Caixa }, UserId);

        maio.Data!.TotalDespesas.Should().Be(0m);
        junho.Data!.TotalDespesas.Should().Be(300m);
    }

    [Fact]
    public async Task FiltroPorCategoriaIds_RestringeResultados()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "B", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var catA = new CategoriaFinanceira { UserId = UserId, Nome = "A", Tipo = CategoriaFinanceiraTipo.Despesa, IconeNome = "x", Cor = "#000" };
        var catB = new CategoriaFinanceira { UserId = UserId, Nome = "B", Tipo = CategoriaFinanceiraTipo.Despesa, IconeNome = "x", Cor = "#000" };
        test.Db.Contas.Add(conta);
        test.Db.CategoriasFinanceiras.AddRange(catA, catB);
        await test.Db.SaveChangesAsync();

        test.Db.Lancamentos.AddRange(
            new Lancamento { UserId = UserId, ContaId = conta.Id, CategoriaFinanceiraId = catA.Id, Descricao = "x", Valor = 100m, DataVencimento = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Tipo = LancamentoTipo.Pagar },
            new Lancamento { UserId = UserId, ContaId = conta.Id, CategoriaFinanceiraId = catB.Id, Descricao = "y", Valor = 50m, DataVencimento = new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc), Tipo = LancamentoTipo.Pagar });
        await test.Db.SaveChangesAsync();

        var service = new RelatorioFinanceiroService(test.Db);
        var result = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        {
            Ano = 2026,
            Mes = 5,
            Quantidade = 1,
            Regime = RegimeContabil.Competencia,
            CategoriaFinanceiraIds = new List<Guid> { catA.Id }
        }, UserId);

        result.Data!.TotalDespesas.Should().Be(100m);
    }

    [Fact]
    public async Task FiltroApenasRealizados_RemoveLancamentosNaoRealizados()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "B", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var cat = new CategoriaFinanceira { UserId = UserId, Nome = "X", Tipo = CategoriaFinanceiraTipo.Despesa, IconeNome = "x", Cor = "#000" };
        test.Db.Contas.Add(conta);
        test.Db.CategoriasFinanceiras.Add(cat);
        test.Db.Lancamentos.AddRange(
            new Lancamento { UserId = UserId, ContaId = conta.Id, CategoriaFinanceiraId = cat.Id, Descricao = "a", Valor = 100m, DataVencimento = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Tipo = LancamentoTipo.Pagar, Realizado = true },
            new Lancamento { UserId = UserId, ContaId = conta.Id, CategoriaFinanceiraId = cat.Id, Descricao = "b", Valor = 50m, DataVencimento = new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc), Tipo = LancamentoTipo.Pagar, Realizado = false });
        await test.Db.SaveChangesAsync();

        var service = new RelatorioFinanceiroService(test.Db);
        var result = await service.LancamentosPorCategoriaAsync(new RelatorioFinanceiroRequest
        {
            Ano = 2026,
            Mes = 5,
            Quantidade = 1,
            Regime = RegimeContabil.Competencia,
            ApenasRealizados = true
        }, UserId);

        result.Data!.TotalDespesas.Should().Be(100m);
    }
}
