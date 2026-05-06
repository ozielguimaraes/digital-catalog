namespace MeuCatalogo.Features.Produto;

public partial class ProdutoListaPage : ContentPage
{
    public ProdutoListaPage(ProdutoListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
