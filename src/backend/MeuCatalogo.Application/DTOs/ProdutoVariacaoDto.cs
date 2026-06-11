using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.DTOs;

public class ProdutoVariacaoDto
{
    public Guid Id { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public decimal? Preco { get; set; }
    public int Quantidade { get; set; }
}

public class ProdutoVariacaoCreateDto
{
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public decimal? Preco { get; set; }
    public int Quantidade { get; set; }
}

// Cores/Tamanhos já usados no catálogo, para reuso/autocomplete no cadastro.
public class VariacaoSugestoesDto
{
    public List<string> Cores { get; set; } = new();
    public List<string> Tamanhos { get; set; } = new();
}
