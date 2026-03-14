using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Domain;

namespace MeuCatalogo.Features.Auth.Data;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetCurrentUserAsync();
    Task SetCurrentUserAsync(UserEntity user);
    Task ClearUserAsync();
}
