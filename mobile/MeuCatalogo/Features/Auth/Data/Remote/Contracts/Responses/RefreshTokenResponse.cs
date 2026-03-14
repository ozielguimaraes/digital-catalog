namespace MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;

public sealed record RefreshTokenResponse
{
    public required string Token { get; init; }
    public string? RefreshToken { get; init; }
}
