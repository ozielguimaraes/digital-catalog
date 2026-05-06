using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Auth.Domain;

namespace MeuCatalogo.Features.Auth.UseCases;

public class GetCurrentUserUseCase(IUserRepository userRepository) : IUseCaseOut<UserEntity?>
{
    public async Task<UserEntity?> ExecuteAsync()
    {
        return await userRepository.GetCurrentUserAsync();
    }
}
