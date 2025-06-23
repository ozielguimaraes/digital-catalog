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
            //tentando navegar para listaproduto ao invés da primeira no appshell
            // string targetPage = _appShellViewModel.ObterPaginaInicial();
            //
            // string produtoAdicionarPage = nameof(ProdutoAdicionarPage);
            // if (produtoAdicionarPage != targetPage)
            // {
            //     return;
            // }
            //
            // var page = Items.FirstOrDefault(x => x.CurrentItem.Items.Any(i => i.Route.Equals(produtoAdicionarPage)));
            //
            // if (page is not null)
            //     CurrentItem = page;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(CatalogoListaPage), typeof(CatalogoListaPage));
        Routing.RegisterRoute(nameof(CatalogoAdicionarPage), typeof(CatalogoAdicionarPage));

        Routing.RegisterRoute(nameof(ProdutoListaPage), typeof(ProdutoListaPage));
        Routing.RegisterRoute(nameof(ProdutoAdicionarPage), typeof(ProdutoAdicionarPage));
    }
}
