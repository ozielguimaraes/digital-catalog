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
