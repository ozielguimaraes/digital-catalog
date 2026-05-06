using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class FinanceiroServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static Lancamento NovoLancamento(
        string userId = UserId,
        LancamentoTipo tipo = LancamentoTipo.Receber,
        LancamentoStatus status = LancamentoStatus.Pendente,
        decimal valor = 100m,
        DateTime? dataVencimento = null,
        DateTime? dataPagamento = null,
        bool ativo = true,
        string descricao = "Teste",
        Guid? fornecedorId = null) => new()
    {
        Descricao = descricao,
        Valor = valor,
        DataVencimento = dataVencimento ?? DateTime.UtcNow,
        DataPagamento = dataPagamento,
        Tipo = tipo,
        Status = status,
        UserId = userId,
        Ativo = ativo,
        FornecedorId = fornecedorId
    };

    [Fact]
    public async Task GetAllAsync_RetornaApenasLancamentosDoUserOrdenadosPorVencimento()
    {
        await using var test = new TestDbContext();
        var amanha = DateTime.UtcNow.AddDays(1);
        var hoje = DateTime.UtcNow;
        var ontem = DateTime.UtcNow.AddDays(-1);

        test.Db.Lancamentos.AddRange(
            NovoLancamento(dataVencimento: amanha, descricao: "C"),
            NovoLancamento(dataVencimento: ontem, descricao: "A"),
            NovoLancamento(dataVencimento: hoje, descricao: "B"),
            NovoLancamento(userId: OutroUserId, descricao: "outro user"));
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetAllAsync(UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data!.Select(l => l.Descricao).Should().ContainInOrder("A", "B", "C");
    }

    [Fact]
    public async Task GetAllAsync_FiltraPorTipo()
    {
        await using var test = new TestDbContext();
        test.Db.Lancamentos.AddRange(
            NovoLancamento(tipo: LancamentoTipo.Receber),
            NovoLancamento(tipo: LancamentoTipo.Receber),
            NovoLancamento(tipo: LancamentoTipo.Pagar));
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetAllAsync(UserId, LancamentoTipo.Pagar);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Tipo.Should().Be(LancamentoTipo.Pagar);
    }

    [Fact]
    public async Task GetAllAsync_IgnoraLancamentosInativos()
    {
        await using var test = new TestDbContext();
        test.Db.Lancamentos.AddRange(
            NovoLancamento(ativo: true),
            NovoLancamento(ativo: false));
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetAllAsync(UserId);

        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_IncluiNomeDoFornecedor()
    {
        await using var test = new TestDbContext();
        var fornecedor = new Fornecedor { Nome = "ACME", UserId = UserId };
        test.Db.Fornecedores.Add(fornecedor);
        test.Db.Lancamentos.Add(NovoLancamento(fornecedorId: fornecedor.Id));
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetAllAsync(UserId);

        result.Data![0].FornecedorNome.Should().Be("ACME");
    }

    [Fact]
    public async Task GetByIdAsync_RetornaLancamento_QuandoExiste()
    {
        await using var test = new TestDbContext();
        var lancamento = NovoLancamento(descricao: "Aluguel");
        test.Db.Lancamentos.Add(lancamento);
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetByIdAsync(lancamento.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Descricao.Should().Be("Aluguel");
    }

    [Fact]
    public async Task GetByIdAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);

        var result = await service.GetByIdAsync(Guid.NewGuid(), UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Lançamento não encontrado");
    }

    [Fact]
    public async Task GetByIdAsync_RetornaErro_QuandoLancamentoEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var lancamento = NovoLancamento(userId: OutroUserId);
        test.Db.Lancamentos.Add(lancamento);
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetByIdAsync(lancamento.Id, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAsync_RetornaErro_QuandoDescricaoVazia(string? descricao)
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);
        var request = new LancamentoRequest
        {
            Descricao = descricao!,
            Valor = 100m,
            DataVencimento = DateTime.UtcNow,
            Tipo = LancamentoTipo.Receber
        };

        var result = await service.CreateAsync(request, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Descrição é obrigatória");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public async Task CreateAsync_RetornaErro_QuandoValorNaoPositivo(decimal valor)
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);
        var request = new LancamentoRequest
        {
            Descricao = "Teste",
            Valor = valor,
            DataVencimento = DateTime.UtcNow,
            Tipo = LancamentoTipo.Receber
        };

        var result = await service.CreateAsync(request, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Valor deve ser maior que zero");
    }

    [Fact]
    public async Task CreateAsync_PersisteLancamentoVinculadoAoUser()
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);
        var request = new LancamentoRequest
        {
            Descricao = "Mensalidade",
            Valor = 500m,
            DataVencimento = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Tipo = LancamentoTipo.Pagar,
            Status = LancamentoStatus.Pendente,
            Observacoes = "obs"
        };

        var result = await service.CreateAsync(request, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Descricao.Should().Be("Mensalidade");
        result.Data.Valor.Should().Be(500m);
        result.Data.Tipo.Should().Be(LancamentoTipo.Pagar);

        var persistido = test.Db.Lancamentos.Single();
        persistido.UserId.Should().Be(UserId);
        persistido.Observacoes.Should().Be("obs");
    }

    [Fact]
    public async Task UpdateAsync_AtualizaCamposEPersisteDataAtualizacao()
    {
        await using var test = new TestDbContext();
        var lancamento = NovoLancamento(valor: 100m, descricao: "antigo");
        test.Db.Lancamentos.Add(lancamento);
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var request = new LancamentoRequest
        {
            Descricao = "novo",
            Valor = 250m,
            DataVencimento = lancamento.DataVencimento,
            Tipo = LancamentoTipo.Pagar,
            Status = LancamentoStatus.Pago,
            DataPagamento = DateTime.UtcNow
        };

        var result = await service.UpdateAsync(lancamento.Id, request, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Descricao.Should().Be("novo");
        result.Data.Valor.Should().Be(250m);
        result.Data.Tipo.Should().Be(LancamentoTipo.Pagar);
        result.Data.Status.Should().Be(LancamentoStatus.Pago);

        var persistido = await test.Db.Lancamentos.FindAsync(lancamento.Id);
        persistido!.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);
        var request = new LancamentoRequest
        {
            Descricao = "x",
            Valor = 1m,
            DataVencimento = DateTime.UtcNow,
            Tipo = LancamentoTipo.Receber
        };

        var result = await service.UpdateAsync(Guid.NewGuid(), request, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Lançamento não encontrado");
    }

    [Fact]
    public async Task UpdateAsync_RetornaErro_QuandoLancamentoEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var lancamento = NovoLancamento(userId: OutroUserId);
        test.Db.Lancamentos.Add(lancamento);
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var request = new LancamentoRequest
        {
            Descricao = "x",
            Valor = 1m,
            DataVencimento = DateTime.UtcNow,
            Tipo = LancamentoTipo.Receber
        };

        var result = await service.UpdateAsync(lancamento.Id, request, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemoveLancamento()
    {
        await using var test = new TestDbContext();
        var lancamento = NovoLancamento();
        test.Db.Lancamentos.Add(lancamento);
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.DeleteAsync(lancamento.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        test.Db.Lancamentos.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);

        var result = await service.DeleteAsync(Guid.NewGuid(), UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Lançamento não encontrado");
    }

    [Fact]
    public async Task GetResumoAsync_CalculaTotaisESaldoCorretamente()
    {
        await using var test = new TestDbContext();
        var hoje = DateTime.UtcNow;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var mesPassado = inicioMes.AddDays(-5);

        test.Db.Lancamentos.AddRange(
            // a receber: pendente + atrasado contam; pago e cancelado não
            NovoLancamento(tipo: LancamentoTipo.Receber, status: LancamentoStatus.Pendente, valor: 100m),
            NovoLancamento(tipo: LancamentoTipo.Receber, status: LancamentoStatus.Atrasado, valor: 50m),
            NovoLancamento(tipo: LancamentoTipo.Receber, status: LancamentoStatus.Pago, valor: 999m, dataPagamento: hoje),
            NovoLancamento(tipo: LancamentoTipo.Receber, status: LancamentoStatus.Cancelado, valor: 10m),
            // a pagar: pendente + atrasado contam
            NovoLancamento(tipo: LancamentoTipo.Pagar, status: LancamentoStatus.Pendente, valor: 80m),
            NovoLancamento(tipo: LancamentoTipo.Pagar, status: LancamentoStatus.Atrasado, valor: 20m),
            // recebido no mês (atual) — pago dentro do mês
            NovoLancamento(tipo: LancamentoTipo.Receber, status: LancamentoStatus.Pago, valor: 200m, dataPagamento: inicioMes.AddDays(1)),
            // pago mês passado — não conta
            NovoLancamento(tipo: LancamentoTipo.Receber, status: LancamentoStatus.Pago, valor: 1000m, dataPagamento: mesPassado),
            // pago no mês (atual) — pago dentro do mês
            NovoLancamento(tipo: LancamentoTipo.Pagar, status: LancamentoStatus.Pago, valor: 70m, dataPagamento: inicioMes.AddDays(2)),
            // outro user — ignorado
            NovoLancamento(userId: OutroUserId, tipo: LancamentoTipo.Receber, valor: 9999m),
            // inativo — ignorado
            NovoLancamento(ativo: false, tipo: LancamentoTipo.Receber, valor: 7777m));
        await test.Db.SaveChangesAsync();

        var service = new FinanceiroService(test.Db);
        var result = await service.GetResumoAsync(UserId);

        result.IsSuccess.Should().BeTrue();
        // 999m do "Pago" do hoje conta no recebidoNoMes mas NÃO em totalAReceber (status Pago)
        result.Data!.TotalAReceber.Should().Be(150m); // 100 + 50
        result.Data.TotalAPagar.Should().Be(100m); // 80 + 20
        result.Data.SaldoPrevisto.Should().Be(50m);
        result.Data.RecebidoNoMes.Should().Be(999m + 200m); // ambos pagos dentro do mês corrente
        result.Data.PagoNoMes.Should().Be(70m);
        result.Data.PeriodoLabel.Should().NotBeNullOrEmpty();
        result.Data.PeriodoLabel.Should().Contain(hoje.Year.ToString());
    }

    [Fact]
    public async Task GetResumoAsync_SemLancamentos_RetornaZeros()
    {
        await using var test = new TestDbContext();
        var service = new FinanceiroService(test.Db);

        var result = await service.GetResumoAsync(UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TotalAReceber.Should().Be(0m);
        result.Data.TotalAPagar.Should().Be(0m);
        result.Data.SaldoPrevisto.Should().Be(0m);
        result.Data.RecebidoNoMes.Should().Be(0m);
        result.Data.PagoNoMes.Should().Be(0m);
    }

    [Fact]
    public async Task CreateAsync_ParcelamentoEmCartao_GeraNLancamentosVinculadosAFaturas()
    {
        await using var test = new TestDbContext();
        var cartao = new Conta
        {
            UserId = UserId,
            Nome = "Cartão",
            Tipo = ContaTipo.CartaoCredito,
            Cor = "#000",
            DiaFechamento = 15,
            DiaVencimento = 25,
            Limite = 5000m
        };
        test.Db.Contas.Add(cartao);
        await test.Db.SaveChangesAsync();

        var faturaService = new FaturaService(test.Db);
        var recorrenciaService = new RecorrenciaService(test.Db);
        var service = new FinanceiroService(test.Db, recorrenciaService, faturaService);

        var result = await service.CreateAsync(new LancamentoRequest
        {
            Descricao = "TV",
            Valor = 100m,
            DataVencimento = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Tipo = LancamentoTipo.Pagar,
            ContaId = cartao.Id,
            ParcelaTotal = 11
        }, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.Lancamentos.Should().HaveCount(11);
        test.Db.Faturas.Should().HaveCount(11);
        test.Db.Lancamentos.Should().AllSatisfy(l =>
        {
            l.ParcelaTotal.Should().Be(11);
            l.FaturaId.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task ListarAsync_ChamaGerarPendentesAntesDeListar_QuandoIncluirRecorrenciasFuturas()
    {
        await using var test = new TestDbContext();
        var conta = new Conta { UserId = UserId, Nome = "B", Tipo = ContaTipo.ContaCorrente, Cor = "#000" };
        var cat = new CategoriaFinanceira { UserId = UserId, Nome = "Casa", Tipo = CategoriaFinanceiraTipo.Despesa, Cor = "#000", IconeNome = "home" };
        test.Db.Contas.Add(conta);
        test.Db.CategoriasFinanceiras.Add(cat);
        await test.Db.SaveChangesAsync();

        var recorrenciaService = new RecorrenciaService(test.Db);
        await recorrenciaService.CreateAsync(new LancamentoRequestForRecorrencia()
        {
            Tipo = CategoriaFinanceiraTipo.Despesa,
            Descricao = "Internet",
            ContaId = conta.Id,
            CategoriaFinanceiraId = cat.Id,
            ValorPadrao = 100m,
            Periodicidade = RecorrenciaPeriodicidade.Mensal,
            DiaDoMes = 10,
            DataInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
        }.ToReq(), UserId);

        var faturaService = new FaturaService(test.Db);
        var service = new FinanceiroService(test.Db, recorrenciaService, faturaService);

        var result = await service.ListarAsync(UserId, new LancamentoFiltro
        {
            DataFim = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            IncluirRecorrenciasFuturas = true
        });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().BeGreaterThan(0);
    }
}

internal class LancamentoRequestForRecorrencia
{
    public CategoriaFinanceiraTipo Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public Guid ContaId { get; set; }
    public Guid CategoriaFinanceiraId { get; set; }
    public decimal? ValorPadrao { get; set; }
    public RecorrenciaPeriodicidade Periodicidade { get; set; }
    public byte? DiaDoMes { get; set; }
    public DateTime DataInicio { get; set; }

    public RecorrenciaRequest ToReq() => new()
    {
        Tipo = Tipo,
        Descricao = Descricao,
        ContaId = ContaId,
        CategoriaFinanceiraId = CategoriaFinanceiraId,
        ValorPadrao = ValorPadrao,
        Periodicidade = Periodicidade,
        DiaDoMes = DiaDoMes,
        DataInicio = DataInicio
    };
}
