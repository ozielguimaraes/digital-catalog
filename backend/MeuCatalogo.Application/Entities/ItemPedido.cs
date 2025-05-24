namespace MeuCatalogo.Application.Entities;

public class ItemPedido : BaseEntity
{
    public Guid PedidoId { get; set; }
    public Pedido Pedido { get; set; }
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; }
    public Guid? VariacaoId { get; set; }
    public Variacao Variacao { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public ItemPedido()
    {
    }

    public ItemPedido(Guid pedidoId, Guid produtoId, int quantidade, decimal precoUnitario, Guid? variacaoId = null) : this()
    {
        PedidoId = pedidoId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        VariacaoId = variacaoId;
        CalcularSubtotal();
    }

    public void CalcularSubtotal()
    {
        Subtotal = Quantidade * PrecoUnitario;
    }

    public void AtualizarQuantidade(int novaQuantidade)
    {
        Quantidade = novaQuantidade;
        CalcularSubtotal();
    }
}
