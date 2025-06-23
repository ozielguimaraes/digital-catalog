using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.DTOs.Requests;

public class ProdutoRequest
{
    public string Nome { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string InformacoesAdicionais { get; set; }
    public EstoqueRequest Estoque { get; set; }
    public List<VariacaoRequest> Variacoes { get; set; }
}