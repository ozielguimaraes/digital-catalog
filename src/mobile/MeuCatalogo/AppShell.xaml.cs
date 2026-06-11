using MeuCatalogo.Core.Abstractions;
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

    // Guarda central de "alterações não salvas": qualquer saída de uma tela de
    // formulário (back do navbar, back físico ou troca de aba) passa por aqui.
    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        if (!args.CanCancel ||
            CurrentPage?.BindingContext is not IDirtyAware dirty ||
            !dirty.TemAlteracoesNaoSalvas)
        {
            return;
        }

        ShellNavigatingDeferral deferral = args.GetDeferral();
        try
        {
            bool descartar = await CurrentPage.DisplayAlert(
                "Descartar alterações?",
                "Você tem alterações não salvas nesta tela. Deseja sair e descartá-las?",
                "Descartar",
                "Continuar editando");

            if (!descartar)
                args.Cancel();
        }
        finally
        {
            deferral.Complete();
        }
    }

    private static void RegisterRoutes()
    {
        // As 5 páginas-aba (HomePage, ProdutoListaPage, CatalogoListaPage,
        // PedidoListaPage, MaisPage) NÃO são registradas aqui: já são rotas via
        // ShellContent Route="..." no AppShell.xaml. Registrá-las também como rota
        // global criava ambiguidade que travava o back ao empilhar páginas sobre elas.

        Routing.RegisterRoute(nameof(CatalogoAdicionarPage), typeof(CatalogoAdicionarPage));
        Routing.RegisterRoute(nameof(CatalogoDetalhePage), typeof(CatalogoDetalhePage));
        Routing.RegisterRoute(nameof(CatalogoPublicaPage), typeof(CatalogoPublicaPage));

        Routing.RegisterRoute(nameof(ProdutoAdicionarPage), typeof(ProdutoAdicionarPage));
        Routing.RegisterRoute(nameof(ProdutoDetalhePage), typeof(ProdutoDetalhePage));

        Routing.RegisterRoute(nameof(PedidoNovoPage), typeof(PedidoNovoPage));
        Routing.RegisterRoute(nameof(ClienteListaPage), typeof(ClienteListaPage));
        Routing.RegisterRoute(nameof(ClienteUpsertPage), typeof(ClienteUpsertPage));
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
