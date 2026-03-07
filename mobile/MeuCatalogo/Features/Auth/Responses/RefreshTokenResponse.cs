namespace MeuCatalogo.Features.Auth.Responses;

public class RefreshTokenResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
