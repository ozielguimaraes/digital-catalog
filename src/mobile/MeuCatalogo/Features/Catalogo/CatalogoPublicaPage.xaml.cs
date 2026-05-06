namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoPublicaPage : ContentPage
{
    public CatalogoPublicaPage(CatalogoPublicaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
