using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllRefreshTokensAsync(string userId);
    Task<bool> IsRefreshTokenValidAsync(RefreshToken refreshToken);
    Task<SigninResponse> RefreshAccessTokenAsync(string refreshToken);
}
