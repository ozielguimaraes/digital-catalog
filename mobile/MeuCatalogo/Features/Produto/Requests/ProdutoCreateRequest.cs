using System;

namespace MeuCatalogo.Features.Produto.Requests;

public sealed class ProdutoCreateRequest
{
    public ProdutoCreateRequest(
        string nome,
        Guid categoriaId,
        Guid catalogoId,
        decimal preco,
        decimal? precoComDesconto = null,
        string? informacoesAdicionais = null,
        EstoqueCreateRequest? estoque = null)
    {
        Nome = nome;
        CategoriaId = categoriaId;
        CatalogoId = catalogoId;
        Preco = preco;
        PrecoComDesconto = precoComDesconto;
        InformacoesAdicionais = informacoesAdicionais;
        Estoque = estoque;
    }

    public string Nome { get; init; }
    public Guid CategoriaId { get; init; }
    public Guid CatalogoId { get; init; }
    public decimal Preco { get; init; }
    public decimal? PrecoComDesconto { get; init; }
    public string? InformacoesAdicionais { get; init; }
    public EstoqueCreateRequest? Estoque { get; init; }
}

public sealed class EstoqueCreateRequest
{
    public EstoqueCreateRequest(int quantidadeInicial, int? quantidadeMinima = null)
    {
        QuantidadeInicial = quantidadeInicial;
        QuantidadeMinima = quantidadeMinima;
    }

    public int QuantidadeInicial { get; init; }
    public int? QuantidadeMinima { get; init; }
}
