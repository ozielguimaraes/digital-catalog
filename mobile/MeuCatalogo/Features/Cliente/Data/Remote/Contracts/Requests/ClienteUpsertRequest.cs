namespace MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;

public sealed record ClienteUpsertRequest
{
    public required string Nome { get; init; }
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? InformacoesAdicionais { get; init; }
}
