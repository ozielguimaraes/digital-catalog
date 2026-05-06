using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Data;

namespace MeuCatalogo.Features.Auth.UseCases;

public class LogoutUseCase(IAuthRepository authRepository) : IUseCaseActivity
{
    public async Task ExecuteAsync()
    {
        await authRepository.LogoutAsync();
    }
}
