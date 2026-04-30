using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;

public sealed record LancamentoRequest
{
    public string Descricao { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime? DataPagamento { get; init; }
    public LancamentoTipo Tipo { get; init; }
    public LancamentoStatus Status { get; init; } = LancamentoStatus.Pendente;
    public string? Observacoes { get; init; }
    public Guid? PedidoId { get; init; }
    public Guid? FornecedorId { get; init; }
    public Guid? ContaId { get; init; }
    public Guid? CategoriaFinanceiraId { get; init; }
    public Guid? SubcategoriaFinanceiraId { get; init; }
    public Guid? ComprovanteFinanceiroId { get; init; }
    public short? ParcelaTotal { get; init; }
    public bool Realizado { get; init; }
}

public sealed record ContaRequest
{
    public string Nome { get; init; } = string.Empty;
    public ContaTipo Tipo { get; init; }
    public string Cor { get; init; } = "#3F51B5";
    public byte? Ordem { get; init; }
    public decimal? Limite { get; init; }
    public byte? DiaFechamento { get; init; }
    public byte? DiaVencimento { get; init; }
    public decimal SaldoInicial { get; init; }
}

public sealed record CategoriaFinanceiraRequest
{
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string IconeNome { get; init; } = "tag";
    public string Cor { get; init; } = "#9E9E9E";
    public byte? Ordem { get; init; }
}

public sealed record SubcategoriaFinanceiraRequest
{
    public string Nome { get; init; } = string.Empty;
    public string? IconeNome { get; init; }
    public string? Cor { get; init; }
    public byte? Ordem { get; init; }
}

public sealed record TransferenciaRequest
{
    public DateTime Data { get; init; }
    public Guid ContaOrigemId { get; init; }
    public Guid ContaDestinoId { get; init; }
    public decimal Valor { get; init; }
    public string? Descricao { get; init; }
    public int? FaturaMes { get; init; }
    public int? FaturaAno { get; init; }
}

public sealed record RecorrenciaRequest
{
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public Guid ContaId { get; init; }
    public Guid CategoriaFinanceiraId { get; init; }
    public Guid? SubcategoriaFinanceiraId { get; init; }
    public decimal? ValorPadrao { get; init; }
    public RecorrenciaPeriodicidade Periodicidade { get; init; }
    public byte? DiaDoMes { get; init; }
    public DayOfWeek? DiaDaSemana { get; init; }
    public DateTime DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
}

public sealed record LancamentoBaixaRequest
{
    public DateTime Data { get; init; }
    public decimal Valor { get; init; }
    public Guid ContaId { get; init; }
    public Guid? ComprovanteFinanceiroId { get; init; }
    public string? Observacoes { get; init; }
}

public sealed record RelatorioFinanceiroRequest
{
    public int Ano { get; init; }
    public int Mes { get; init; }
    public int Quantidade { get; init; } = 1;
    public RegimeContabil Regime { get; init; } = RegimeContabil.Caixa;
    public List<Guid>? ContaIds { get; init; }
    public List<Guid>? CategoriaFinanceiraIds { get; init; }
    public CategoriaFinanceiraTipo? Tipo { get; init; }
    public bool ApenasRealizados { get; init; } = false;
}
