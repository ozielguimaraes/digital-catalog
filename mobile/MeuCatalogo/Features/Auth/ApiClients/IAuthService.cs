using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;

namespace MeuCatalogo.Features.Auth.ApiClients;

public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    bool IsAuthenticated();
    Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken cancellationToken = default);
    Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default);
}
