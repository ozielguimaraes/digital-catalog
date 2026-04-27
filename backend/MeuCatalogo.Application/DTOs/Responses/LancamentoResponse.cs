using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Responses;

public class LancamentoResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public LancamentoTipo Tipo { get; set; }
    public LancamentoStatus Status { get; set; }
    public string? Observacoes { get; set; }
    public Guid? PedidoId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FinanceiroResumoResponse
{
    public decimal TotalAReceber { get; set; }
    public decimal TotalAPagar { get; set; }
    public decimal SaldoPrevisto { get; set; }
    public decimal RecebidoNoMes { get; set; }
    public decimal PagoNoMes { get; set; }
    public string PeriodoLabel { get; set; } = string.Empty;
}
