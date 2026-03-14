namespace MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;

public sealed record UserResponse
{
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
}
