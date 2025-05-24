using System;

namespace MeuCatalogo.Application.DTOs.Requests;

public class CatalogoRequest
{
    public string Nome { get; set; }
    public string NomeCurto { get; set; }
    public string TelefoneWhatsapp { get; set; }
}