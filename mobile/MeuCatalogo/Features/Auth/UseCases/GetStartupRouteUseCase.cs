using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo.Features.Auth.UseCases;

public class GetStartupRouteUseCase : IUseCaseOut<string>
{
    private readonly IAuthRepository _authRepository;
    private readonly ISettingsService _settingsService;

    public GetStartupRouteUseCase(IAuthRepository authRepository, ISettingsService settingsService)
    {
        _authRepository = authRepository;
        _settingsService = settingsService;
    }

    public async Task<string> ExecuteAsync()
    {
        bool isAuthenticated = _authRepository.IsAuthenticated();
        bool possuiCatalogoFavorito = _settingsService.CatalogoFavorito is not null;

        string targetPage = isAuthenticated
            ? possuiCatalogoFavorito ? nameof(ProdutoAdicionarPage) : nameof(CatalogoListaPage)
            : nameof(LoginPage);

        return await Task.FromResult(targetPage);
    }
}
