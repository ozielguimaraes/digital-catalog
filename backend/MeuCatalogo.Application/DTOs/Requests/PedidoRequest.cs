using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.DTOs.Requests;

public class PedidoRequest
{
    public Guid ClienteId { get; set; }
    public List<ItemPedidoRequest>? Itens { get; set; }
}
