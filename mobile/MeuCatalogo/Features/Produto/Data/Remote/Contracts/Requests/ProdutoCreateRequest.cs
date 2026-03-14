namespace MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;

public sealed record ProdutoCreateRequest
{
    public required string Nome { get; init; }
    public Guid CategoriaId { get; init; }
    public Guid CatalogoId { get; init; }
    public decimal Preco { get; init; }
    public decimal? PrecoComDesconto { get; init; }
    public string? InformacoesAdicionais { get; init; }
    public EstoqueCreateRequest? Estoque { get; init; }
}

public sealed record EstoqueCreateRequest
{
    public int QuantidadeInicial { get; init; }
    public int? QuantidadeMinima { get; init; }
}

