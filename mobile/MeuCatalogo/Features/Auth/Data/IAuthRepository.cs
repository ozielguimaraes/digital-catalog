using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Auth.Data;

public interface IAuthRepository
{
    bool IsAuthenticated();
    Task<bool> IsAuthenticatedAsync(CancellationToken ct = default);
    Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken ct = default);
    Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken ct = default);
    Task<bool> RefreshTokenAsync(CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
