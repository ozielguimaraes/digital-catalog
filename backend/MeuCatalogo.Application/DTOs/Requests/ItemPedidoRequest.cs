using System;

namespace MeuCatalogo.Application.DTOs.Requests;

public class ItemPedidoRequest
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}