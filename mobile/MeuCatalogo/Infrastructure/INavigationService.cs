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

public sealed class NavigationService : INavigationService
{
    private readonly IAuthRepository _authRepository;
    private readonly ISettingsService _settingsService;

    public NavigationService(IAuthRepository authRepository, ISettingsService settingsService)
    {
        _authRepository = authRepository;
        _settingsService = settingsService;
    }

    public async Task InitializeAsync()
    {
        bool isAuthenticated =  _authRepository.IsAuthenticated();
        bool possuiCatalogoFavorito = _settingsService.CatalogoFavorito is not null;

        string targetPage = isAuthenticated
            ? (possuiCatalogoFavorito ? nameof(ProdutoAdicionarPage) : nameof(CatalogoListaPage))
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
