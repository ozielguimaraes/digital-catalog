using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Auth.Domain;

namespace MeuCatalogo.Features.Auth.UseCases;

public class GetCurrentUserUseCase : IUseCaseOut<UserEntity?>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserEntity?> ExecuteAsync()
    {
        return await _userRepository.GetCurrentUserAsync();
    }
}
