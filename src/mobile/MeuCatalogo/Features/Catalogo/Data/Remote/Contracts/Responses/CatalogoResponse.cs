namespace MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Responses;

public sealed record CatalogoResponse
{
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? NomeCurto { get; init; }
    public string? NumeroWhatsapp { get; init; }
    public string? Email { get; init; }
    public string? Descricao { get; init; }
}

