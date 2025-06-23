using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.DTOs.Responses;

public class PedidoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; }
    public List<ItemPedidoResponse> Itens { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}