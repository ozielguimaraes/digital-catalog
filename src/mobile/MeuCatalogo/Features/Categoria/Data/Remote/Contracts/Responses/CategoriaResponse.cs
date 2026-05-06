namespace MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Responses;

public sealed record CategoriaResponse
{
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public Guid CatalogoId { get; init; }
}

