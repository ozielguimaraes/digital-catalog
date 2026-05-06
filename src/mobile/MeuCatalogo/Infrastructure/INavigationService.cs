using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo.Infrastructure;

public interface INavigationService
{
    Task InitializeAsync();
    Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters = null);
    Task PopAsync();
}

public sealed class NavigationService(IAuthRepository authRepository, ISettingsService settingsService)
    : INavigationService
{
    public async Task InitializeAsync()
    {
        bool isAuthenticated =  authRepository.IsAuthenticated();
        bool possuiCatalogoEmUso = settingsService.CatalogoEmUso is not null;

        string targetPage = isAuthenticated
            ? (possuiCatalogoEmUso ? nameof(ProdutoAdicionarPage) : nameof(CatalogoListaPage))
            : nameof(LoginPage);

        await NavigateToAsync($"//{targetPage}");
    }

    public Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters = null)
    {
        var shellNavigation = new ShellNavigationState(route);

        return routeParameters != null
            ? Shell.Current.GoToAsync(shellNavigation, routeParameters)
            : Shell.Current.GoToAsync(shellNavigation);
    }

    public Task PopAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}
