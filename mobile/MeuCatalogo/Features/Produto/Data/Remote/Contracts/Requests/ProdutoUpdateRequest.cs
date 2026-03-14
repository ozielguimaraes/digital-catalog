namespace MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;

public sealed record ProdutoUpdateRequest
{
    public required string Nome { get; init; }
    public Guid CategoriaId { get; init; }
    public decimal Preco { get; init; }
    public decimal? PrecoComDesconto { get; init; }
    public string? InformacoesAdicionais { get; init; }
}

