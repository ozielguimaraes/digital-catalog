namespace MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;

public sealed record CategoriaUpsertRequest
{
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public Guid CatalogoId { get; init; }
}

