using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Auth.Data.Remote;

public interface IAuthRemoteDataSource
{
    Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request, CancellationToken ct = default);
    Task<ApiResponse<SigninResponse>> SigninAsync(SigninRequest request, CancellationToken ct = default);
    Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
}
