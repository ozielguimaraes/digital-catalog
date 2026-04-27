using System;

namespace MeuCatalogo.Application.DTOs.Requests;

public class ItemPedidoRequest
{
    public Guid ProdutoId { get; set; }
    public Guid? VariacaoId { get; set; }
    public int Quantidade { get; set; }
}