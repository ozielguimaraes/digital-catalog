using MeuCatalogo.Features.Pedido.Domain;

namespace MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Responses;

public sealed record PedidoResponse
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public string ClienteNome { get; init; } = string.Empty;
    public List<ItemPedidoResponse> Itens { get; init; } = [];
    public decimal ValorTotal { get; init; }
    public PedidoStatus Status { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record ItemPedidoResponse
{
    public Guid Id { get; init; }
    public Guid ProdutoId { get; init; }
    public string ProdutoNome { get; init; } = string.Empty;
    public Guid? VariacaoId { get; init; }
    public string? VariacaoDescricao { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorUnitario { get; init; }
    public decimal Subtotal { get; init; }
}
