using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;
using Refit;

namespace MeuCatalogo.Features.Auth.ApiClients;

public interface IAuthApi
{
    [Post("/api/auth/register")]
    Task<UserResponse> SignupAsync([Body] SignupRequest request, CancellationToken ct = default);

    [Post("/api/auth/login")]
    Task<SigninResponse> SigninAsync([Body] SigninRequest request, CancellationToken ct = default);

    [Get("/api/auth/me")]
    Task<UserResponse> GetCurrentUserAsync([Header("Authorization")] string bearerToken, CancellationToken ct = default);
}
