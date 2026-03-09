using MeuCatalogo.Features.Auth.Responses;

namespace MeuCatalogo.Features.Auth.Local;

public interface IAuthLocalRepository
{
    Task SaveUserSessionAsync(UserResponse user, CancellationToken ct = default);
}
