using System;

namespace MeuCatalogo.Application.DTOs.Responses;

public class CatalogoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string NomeCurto { get; set; }
    public string TelefoneWhatsapp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}