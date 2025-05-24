using System;

namespace MeuCatalogo.Application.DTOs.Responses;

public class VariacaoResponse
{
    public Guid Id { get; set; }
    public string TipoNome { get; set; }
    public string Opcao { get; set; }
}