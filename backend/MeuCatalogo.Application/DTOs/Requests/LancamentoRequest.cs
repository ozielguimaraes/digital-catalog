using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Requests;

public class LancamentoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public LancamentoTipo Tipo { get; set; }
    public LancamentoStatus Status { get; set; } = LancamentoStatus.Pendente;
    public string? Observacoes { get; set; }
    public Guid? PedidoId { get; set; }
    public Guid? FornecedorId { get; set; }
}
