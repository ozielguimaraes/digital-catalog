namespace MeuCatalogo.Features.Produto.Presentation;

public partial class ProdutoDetalhePage : ContentPage
{
    public ProdutoDetalhePage(ProdutoDetalhePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
