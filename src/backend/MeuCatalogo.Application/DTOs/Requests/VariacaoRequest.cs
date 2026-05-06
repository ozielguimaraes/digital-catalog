using System;

namespace MeuCatalogo.Application.DTOs.Requests;

public class VariacaoRequest
{
    public string TipoNome { get; set; }
    public string[] Opcoes { get; set; }
}