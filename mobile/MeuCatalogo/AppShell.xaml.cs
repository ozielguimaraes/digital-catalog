using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;

namespace MeuCatalogo;

public partial class AppShell : Shell
{
    private readonly AppShellViewModel _appShellViewModel;

    public AppShell(AppShellViewModel appShellViewModel)
    {
        InitializeComponent();
        RegisterRoutes();
        Application.Current!.UserAppTheme = AppTheme.Light;
        BindingContext = _appShellViewModel = appShellViewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            await _appShellViewModel.UpdateUserInfo();
        }
        catch (Exception ex)
        {
            throw; // TODO handle exception
        }
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(CatalogoListaPage), typeof(CatalogoListaPage));
        Routing.RegisterRoute(nameof(CatalogoAdicionarPage), typeof(CatalogoAdicionarPage));

        Routing.RegisterRoute(nameof(ProdutoListaPage), typeof(ProdutoListaPage));
    }
}
