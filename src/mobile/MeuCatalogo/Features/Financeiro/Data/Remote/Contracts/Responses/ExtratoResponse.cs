using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Responses;

public sealed record ExtratoResponse
{
    public DateTime PeriodoInicio { get; init; }
    public DateTime PeriodoFim { get; init; }
    public List<Guid> ContaIds { get; init; } = new();
    public decimal SaldoInicial { get; init; }
    public decimal SaldoFinal { get; init; }
    public decimal TotalEntradas { get; init; }
    public decimal TotalSaidas { get; init; }
    public List<ExtratoMovimentoResponse> Movimentos { get; init; } = new();
    public List<ExtratoSaldoDiarioResponse> SaldosDiarios { get; init; } = new();
}

public sealed record ExtratoMovimentoResponse
{
    public Guid Id { get; init; }
    public ExtratoMovimentoOrigem Origem { get; init; }
    public Guid LancamentoId { get; init; }
    public Guid ContaId { get; init; }
    public string ContaNome { get; init; } = string.Empty;
    public DateTime Data { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public ExtratoMovimentoTipo Tipo { get; init; }
    public decimal Valor { get; init; }
    public decimal SaldoApos { get; init; }
    public string? CategoriaNome { get; init; }
    public string? CategoriaIcone { get; init; }
    public string? CategoriaCor { get; init; }
    public string? Observacoes { get; init; }
}

public sealed record ExtratoSaldoDiarioResponse
{
    public DateTime Data { get; init; }
    public decimal Entradas { get; init; }
    public decimal Saidas { get; init; }
    public decimal SaldoFinalDia { get; init; }
}
