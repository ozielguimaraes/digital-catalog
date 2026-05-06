namespace MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;

public sealed record PedidoCreateRequest
{
    public required Guid ClienteId { get; init; }
    public List<ItemPedidoCreateRequest> Itens { get; init; } = [];
}

public sealed record ItemPedidoCreateRequest
{
    public required Guid ProdutoId { get; init; }
    public Guid? VariacaoId { get; init; }
    public required int Quantidade { get; init; }
}
