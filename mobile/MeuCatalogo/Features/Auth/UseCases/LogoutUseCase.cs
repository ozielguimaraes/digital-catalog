using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Auth.Data;

namespace MeuCatalogo.Features.Auth.UseCases;

public class LogoutUseCase : IUseCaseActivity
{
    private readonly IAuthRepository _authRepository;

    public LogoutUseCase(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task ExecuteAsync()
    {
        await _authRepository.LogoutAsync();
    }
}
