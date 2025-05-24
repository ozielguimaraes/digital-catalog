using System;

namespace MeuCatalogo.Application.DTOs.Responses;

public class CategoriaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}