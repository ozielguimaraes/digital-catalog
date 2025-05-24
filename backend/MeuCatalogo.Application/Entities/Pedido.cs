    namespace MeuCatalogo.Application.Entities;

public enum StatusPedido
{
    Novo,
    Confirmado,
    AguardandoPagamento,
    Pago,
    Preparando,
    ProntoParaRetirada,
    ProntoParaEntrega,
    ACaminho,
    Entregue,
    Finalizado,
    Cancelado
}

public enum FormaPagamento
{
    ACombinar,
    Credito,
    Debito,
    Pix,
    Dinheiro
}

public class Pedido : BaseEntity
{
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public ICollection<ItemPedido> Itens { get; set; }
    public decimal ValorTotal { get; private set; }
    public StatusPedido Status { get; set; }
    public FormaPagamento FormaPagamento { get; set; }

    protected Pedido()
    {
        Itens = new List<ItemPedido>();
        Status = StatusPedido.Novo;
        FormaPagamento = FormaPagamento.ACombinar;
    }

    public Pedido(Guid clienteId) : this()
    {
        ClienteId = clienteId;
    }

    public void CalcularValorTotal()
    {
        ValorTotal = Itens.Sum(i => i.Subtotal);
    }

    public void AtualizarStatus(StatusPedido novoStatus)
    {
        Status = novoStatus;
    }

    public void DefinirFormaPagamento(FormaPagamento formaPagamento)
    {
        FormaPagamento = formaPagamento;
    }
}
