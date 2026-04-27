namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoDetalhePage : ContentPage
{
    public CatalogoDetalhePage(CatalogoDetalhePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
