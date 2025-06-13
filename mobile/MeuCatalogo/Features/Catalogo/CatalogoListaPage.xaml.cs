namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoListaPage
{
    public CatalogoListaPage(CatalagoListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
