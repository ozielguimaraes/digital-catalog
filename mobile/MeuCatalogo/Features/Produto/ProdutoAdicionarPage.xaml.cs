namespace MeuCatalogo.Features.Produto;

public partial class ProdutoAdicionarPage : ContentPage
{
    public ProdutoAdicionarPage(ProdutoAdicionarPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
