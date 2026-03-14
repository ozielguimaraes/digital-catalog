namespace MeuCatalogo.Features.Auth.Data.Local;

public interface IAuthLocalDataSource
{
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string?> GetRefreshTokenAsync(CancellationToken ct = default);
    Task SetTokensAsync(string accessToken, string? refreshToken, CancellationToken ct = default);
    void ClearTokens();
    bool GetIsAuthenticatedFlag();
    void SetIsAuthenticatedFlag(bool isAuthenticated);
}

