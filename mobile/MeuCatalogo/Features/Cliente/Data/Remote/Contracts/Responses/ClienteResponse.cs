namespace MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Responses;

public sealed record ClienteResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? InformacoesAdicionais { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
