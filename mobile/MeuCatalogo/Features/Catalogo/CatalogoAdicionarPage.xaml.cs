namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoAdicionarPage
{
    public CatalogoAdicionarPage(CatalogoAdicionarPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

