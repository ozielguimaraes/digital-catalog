using System;

namespace MeuCatalogo.Application.DTOs.Requests;

public class EstoqueRequest
{
    public bool Disponivel { get; set; }
    public int? Quantidade { get; set; }
    public bool Ilimitado { get; set; }
}