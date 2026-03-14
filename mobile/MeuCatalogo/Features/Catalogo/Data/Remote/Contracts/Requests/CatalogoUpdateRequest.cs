namespace MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;

public sealed record CatalogoUpdateRequest
{
    public required string Nome { get; init; }
    public required string NomeCurto { get; init; }
    public required string NumeroWhatsapp { get; init; }
    public required string Email { get; init; }
    public string? Descricao { get; init; }
}

