using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.DTOs.Responses;

public class ProdutoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string InformacoesAdicionais { get; set; }
    public EstoqueResponse Estoque { get; set; }
    public List<VariacaoResponse> Variacoes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}