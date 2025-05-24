using System;

namespace MeuCatalogo.Application.DTOs.Responses;

public class EstoqueResponse
{
    public Guid Id { get; set; }
    public bool Disponivel { get; set; }
    public int? Quantidade { get; set; }
    public bool Ilimitado { get; set; }
}