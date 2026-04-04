namespace MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;

public sealed record SigninResponse
{
    public required string Token { get; init; }
    public string? RefreshToken { get; init; }
    public UserResponse? User { get; init; }
    public CatalogoFavoritoResponse? CatalogoFavorito => User?.CatalogoFavorito;
}
