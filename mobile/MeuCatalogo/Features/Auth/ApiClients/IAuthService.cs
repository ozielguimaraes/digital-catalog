using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Responses;

namespace MeuCatalogo.Features.Auth.ApiClients;

public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    bool IsAuthenticated();
    Task<UserResponse?> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default);
    Task<SigninResponse> SigninAsync(SigninRequest request, CancellationToken cancellationToken = default);
}
