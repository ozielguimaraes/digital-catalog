using System;

namespace MeuCatalogo.Application.DTOs.Requests;

public class ClienteRequest
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public string InformacoesAdicionais { get; set; }
}