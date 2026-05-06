namespace MeuCatalogo.Features.Categoria.Domain;

public sealed record CategoriaInfo
{
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public Guid CatalogoId { get; init; }
}
