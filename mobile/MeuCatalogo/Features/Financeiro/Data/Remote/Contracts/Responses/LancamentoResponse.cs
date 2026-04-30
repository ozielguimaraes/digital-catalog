using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Responses;

public sealed record LancamentoResponse
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime? DataPagamento { get; init; }
    public LancamentoTipo Tipo { get; init; }
    public LancamentoStatus Status { get; init; }
    public string? Observacoes { get; init; }
    public Guid? PedidoId { get; init; }
    public Guid? FornecedorId { get; init; }
    public string? FornecedorNome { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid? ContaId { get; init; }
    public string? ContaNome { get; init; }
    public Guid? CategoriaFinanceiraId { get; init; }
    public string? CategoriaFinanceiraNome { get; init; }
    public string? CategoriaFinanceiraIcone { get; init; }
    public string? CategoriaFinanceiraCor { get; init; }
    public Guid? SubcategoriaFinanceiraId { get; init; }
    public string? SubcategoriaFinanceiraNome { get; init; }
    public LancamentoOperacao Operacao { get; init; }
    public LancamentoTipoTransferencia? TipoTransferencia { get; init; }
    public Guid? LancamentoTransferenciaId { get; init; }
    public short? ParcelaAtual { get; init; }
    public short? ParcelaTotal { get; init; }
    public Guid? FaturaId { get; init; }
    public Guid? RecorrenciaId { get; init; }
    public Guid? ComprovanteFinanceiroId { get; init; }
    public bool Realizado { get; init; }
    public decimal ValorBaixado { get; init; }
    public decimal ValorEmAberto { get; init; }
}

public sealed record FinanceiroResumoResponse
{
    public decimal TotalAReceber { get; init; }
    public decimal TotalAPagar { get; init; }
    public decimal SaldoPrevisto { get; init; }
    public decimal RecebidoNoMes { get; init; }
    public decimal PagoNoMes { get; init; }
    public string PeriodoLabel { get; init; } = string.Empty;
}

public sealed record ContaResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public ContaTipo Tipo { get; init; }
    public string Cor { get; init; } = string.Empty;
    public byte? Ordem { get; init; }
    public decimal? Limite { get; init; }
    public byte? DiaFechamento { get; init; }
    public byte? DiaVencimento { get; init; }
    public decimal SaldoInicial { get; init; }
    public bool Ativo { get; init; }
}

public sealed record CategoriaFinanceiraResponse
{
    public Guid Id { get; init; }
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string IconeNome { get; init; } = string.Empty;
    public string Cor { get; init; } = string.Empty;
    public byte? Ordem { get; init; }
    public bool Ativo { get; init; }
    public List<SubcategoriaFinanceiraResponse> Subcategorias { get; init; } = new();
}

public sealed record SubcategoriaFinanceiraResponse
{
    public Guid Id { get; init; }
    public Guid CategoriaFinanceiraId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? IconeNome { get; init; }
    public string? Cor { get; init; }
    public byte? Ordem { get; init; }
    public bool Ativo { get; init; }
}

public sealed record FaturaResponse
{
    public Guid Id { get; init; }
    public Guid ContaId { get; init; }
    public string ContaNome { get; init; } = string.Empty;
    public int Mes { get; init; }
    public int Ano { get; init; }
    public DateTime DataInicio { get; init; }
    public DateTime DataFim { get; init; }
    public DateTime DataVencimento { get; init; }
    public decimal ValorTotal { get; init; }
    public decimal? ValorPago { get; init; }
    public decimal ValorEmAberto { get; init; }
    public bool Quitada { get; init; }
    public List<LancamentoResponse> Lancamentos { get; init; } = new();
}

public sealed record RecorrenciaResponse
{
    public Guid Id { get; init; }
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public Guid ContaId { get; init; }
    public string? ContaNome { get; init; }
    public Guid CategoriaFinanceiraId { get; init; }
    public string? CategoriaFinanceiraNome { get; init; }
    public Guid? SubcategoriaFinanceiraId { get; init; }
    public string? SubcategoriaFinanceiraNome { get; init; }
    public decimal? ValorPadrao { get; init; }
    public RecorrenciaPeriodicidade Periodicidade { get; init; }
    public byte? DiaDoMes { get; init; }
    public DayOfWeek? DiaDaSemana { get; init; }
    public DateTime DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public DateTime ProximaData { get; init; }
    public bool Ativo { get; init; }
}

public sealed record LancamentoBaixaResponse
{
    public Guid Id { get; init; }
    public Guid LancamentoId { get; init; }
    public DateTime Data { get; init; }
    public decimal Valor { get; init; }
    public Guid ContaId { get; init; }
    public string? ContaNome { get; init; }
    public Guid? ComprovanteFinanceiroId { get; init; }
    public string? Observacoes { get; init; }
}

public sealed record ComprovanteFinanceiroResponse
{
    public Guid Id { get; init; }
    public string? Descricao { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Url { get; init; } = string.Empty;
}

public sealed record RelatorioFinanceiroResponse
{
    public int Ano { get; init; }
    public int Mes { get; init; }
    public int Quantidade { get; init; }
    public List<RelatorioMesItem> Meses { get; init; } = new();
    public List<RelatorioCategoriaItem> Receitas { get; init; } = new();
    public List<RelatorioCategoriaItem> Despesas { get; init; } = new();
    public decimal TotalReceitas { get; init; }
    public decimal TotalDespesas { get; init; }
    public decimal Saldo { get; init; }
}

public sealed record RelatorioMesItem
{
    public int Ano { get; init; }
    public int Mes { get; init; }
    public string Label { get; init; } = string.Empty;
    public decimal Receitas { get; init; }
    public decimal Despesas { get; init; }
    public decimal Saldo { get; init; }
}

public sealed record RelatorioCategoriaItem
{
    public Guid? CategoriaFinanceiraId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Icone { get; init; }
    public string? Cor { get; init; }
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public decimal Total { get; init; }
    public List<RelatorioMesItem> Mensal { get; init; } = new();
}
