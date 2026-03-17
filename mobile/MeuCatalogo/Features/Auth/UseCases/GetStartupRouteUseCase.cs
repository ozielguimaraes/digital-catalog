using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo.Features.Auth.UseCases;

public class GetStartupRouteUseCase(IAuthRepository authRepository, ISettingsService settingsService)
    : IUseCaseOut<string>
{
    public async Task<string> ExecuteAsync()
    {
        bool isAuthenticated = authRepository.IsAuthenticated();
        bool possuiCatalogoFavorito = settingsService.CatalogoFavorito is not null;

        string targetPage = isAuthenticated
            ? possuiCatalogoFavorito ? nameof(ProdutoAdicionarPage) : nameof(CatalogoListaPage)
            : nameof(LoginPage);

        return await Task.FromResult(targetPage);
    }
}
