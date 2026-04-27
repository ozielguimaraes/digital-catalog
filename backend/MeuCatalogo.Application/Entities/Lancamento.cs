namespace MeuCatalogo.Application.Entities;

public enum LancamentoTipo
{
    Receber,
    Pagar
}

public enum LancamentoStatus
{
    Pendente,
    Pago,
    Atrasado,
    Cancelado
}

public class Lancamento : BaseEntity
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public LancamentoTipo Tipo { get; set; }
    public LancamentoStatus Status { get; set; } = LancamentoStatus.Pendente;
    public string? Observacoes { get; set; }

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
