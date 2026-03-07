using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;
using Microsoft.Maui.Controls.Internals;
using Refit;

namespace MeuCatalogo.Features.Auth.ApiClients;

[Preserve(AllMembers = true)]
public interface IAuthApi
{
    [Post("/auth/register")]
    Task<UserResponse> SignupAsync([Body] SignupRequest request, CancellationToken ct = default);

    [Post("/auth/login")]
    Task<SigninResponse> SigninAsync([Body] SigninRequest request, CancellationToken ct = default);

    [Post("/auth/refresh-token")]
    Task<RefreshTokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request, CancellationToken ct = default);

    [Get("/auth/me")]
    Task<UserResponse> GetCurrentUserAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
