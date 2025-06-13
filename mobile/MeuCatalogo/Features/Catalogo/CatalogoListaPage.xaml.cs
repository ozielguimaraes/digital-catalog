namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoListaPage
{
    public CatalogoListaPage(CatalogoListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
