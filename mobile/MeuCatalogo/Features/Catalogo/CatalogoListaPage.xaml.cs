namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoListaPage
{
    private readonly CatalogoListaPageViewModel _viewModel;

    public CatalogoListaPage(CatalogoListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarCatalogosCommand.ExecuteAsync(null);
    }
}
