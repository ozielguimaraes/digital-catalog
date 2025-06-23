using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Mappers;

public static class PedidoMapper
{
    public static PedidoResponse MapToResponse(this Pedido pedido)
    {
        var response = new PedidoResponse
        {
            Id = pedido.Id,
            ClienteId = pedido.ClienteId,
            ClienteNome = pedido.Cliente!.Nome,
            ValorTotal = pedido.ValorTotal,
            CreatedAt = pedido.DataCriacao,
            UpdatedAt = pedido.DataAtualizacao,
            Itens = new List<ItemPedidoResponse>()
        };

        foreach (var item in pedido.Itens)
        {
            response.Itens.Add(new ItemPedidoResponse
            {
                Id = item.Id,
                ProdutoId = item.ProdutoId,
                ProdutoNome = item.Produto.Nome,
                Quantidade = item.Quantidade,
                ValorUnitario = item.PrecoUnitario,
                Subtotal = item.Subtotal
            });
        }

        return response;
    }

    public static List<PedidoResponse> MapToResponse(this IEnumerable<Pedido> pedidos)
    {
        var response = pedidos.Select(pedido => pedido.MapToResponse()).ToList();
        return response;
    }

    public static List<PedidoResponse> MapToResponse(this ICollection<Pedido> pedidos)
    {
        var response = pedidos.Select(pedido => pedido.MapToResponse()).ToList();
        return response;
    }
}
