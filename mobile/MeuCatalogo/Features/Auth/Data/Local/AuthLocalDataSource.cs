using Microsoft.Maui.Storage;
using MeuCatalogo.Features;

namespace MeuCatalogo.Features.Auth.Data.Local;

public sealed class AuthLocalDataSource : IAuthLocalDataSource
{
    public Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        return SecureStorage.Default.GetAsync(BaseApiService.TokenKey);
    }

    public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default)
    {
        return SecureStorage.Default.GetAsync(BaseApiService.RefreshTokenKey);
    }

    public async Task SetTokensAsync(string accessToken, string? refreshToken, CancellationToken ct = default)
    {
        await SecureStorage.Default.SetAsync(BaseApiService.TokenKey, accessToken);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await SecureStorage.Default.SetAsync(BaseApiService.RefreshTokenKey, refreshToken);
        }
    }

    public void ClearTokens()
    {
        SecureStorage.Default.Remove(BaseApiService.TokenKey);
        SecureStorage.Default.Remove(BaseApiService.RefreshTokenKey);
    }

    public bool GetIsAuthenticatedFlag()
    {
        return Preferences.Get("IsAuthenticatedFlag", false);
    }

    public void SetIsAuthenticatedFlag(bool isAuthenticated)
    {
        Preferences.Set("IsAuthenticatedFlag", isAuthenticated);
    }
}

