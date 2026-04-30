using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Cliente;
using MeuCatalogo.Features.Financeiro;
using MeuCatalogo.Features.Fornecedor;
using MeuCatalogo.Features.Home;
using MeuCatalogo.Features.Mais;
using MeuCatalogo.Features.Pedido;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.Presentation;

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
        catch (Exception)
        {
            throw;
        }
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(MaisPage), typeof(MaisPage));

        Routing.RegisterRoute(nameof(CatalogoListaPage), typeof(CatalogoListaPage));
        Routing.RegisterRoute(nameof(CatalogoAdicionarPage), typeof(CatalogoAdicionarPage));
        Routing.RegisterRoute(nameof(CatalogoDetalhePage), typeof(CatalogoDetalhePage));
        Routing.RegisterRoute(nameof(CatalogoPublicaPage), typeof(CatalogoPublicaPage));

        Routing.RegisterRoute(nameof(ProdutoListaPage), typeof(ProdutoListaPage));
        Routing.RegisterRoute(nameof(ProdutoAdicionarPage), typeof(ProdutoAdicionarPage));
        Routing.RegisterRoute(nameof(ProdutoDetalhePage), typeof(ProdutoDetalhePage));

        Routing.RegisterRoute(nameof(PedidoListaPage), typeof(PedidoListaPage));
        Routing.RegisterRoute(nameof(ClienteListaPage), typeof(ClienteListaPage));
        Routing.RegisterRoute(nameof(FornecedorListaPage), typeof(FornecedorListaPage));
        Routing.RegisterRoute(nameof(FinanceiroPage), typeof(FinanceiroPage));
        Routing.RegisterRoute(nameof(ReceberPage), typeof(ReceberPage));
        Routing.RegisterRoute(nameof(PagarPage), typeof(PagarPage));
        Routing.RegisterRoute(nameof(ContasListaPage), typeof(ContasListaPage));
        Routing.RegisterRoute(nameof(ContaEdicaoPage), typeof(ContaEdicaoPage));
        Routing.RegisterRoute(nameof(CategoriasFinanceirasPage), typeof(CategoriasFinanceirasPage));
        Routing.RegisterRoute(nameof(CategoriaFinanceiraEdicaoPage), typeof(CategoriaFinanceiraEdicaoPage));
        Routing.RegisterRoute(nameof(RecorrenciasPage), typeof(RecorrenciasPage));
        Routing.RegisterRoute(nameof(TransferenciaPage), typeof(TransferenciaPage));
        Routing.RegisterRoute(nameof(BaixaPage), typeof(BaixaPage));
        Routing.RegisterRoute(nameof(FaturaPage), typeof(FaturaPage));
        Routing.RegisterRoute(nameof(RelatoriosPage), typeof(RelatoriosPage));
        Routing.RegisterRoute(nameof(ExtratoPage), typeof(ExtratoPage));
    }
}
