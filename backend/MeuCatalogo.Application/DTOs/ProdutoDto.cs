using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.DTOs;

public class ProdutoImagemDto
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
}

public class ProdutoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; }
    public Guid CatalogoId { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }
    public EstoqueDto Estoque { get; set; }
    public ICollection<VariacaoDto> Variacoes { get; set; }
    public ICollection<ProdutoImagemDto> Imagens { get; set; } = new List<ProdutoImagemDto>();
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class ProdutoCreateDto
{
    public string Nome { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid CatalogoId { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }
    public EstoqueCreateDto? Estoque { get; set; }
}

public class ProdutoUpdateDto
{
    public string Nome { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }
    public EstoqueUpdateDto? Estoque { get; set; }
}
