namespace MeuCatalogo.Features.Produto;

public partial class ProdutoAdicionarPage : ContentPage
{
    private readonly ProdutoAdicionarPageViewModel _viewModel;

    public ProdutoAdicionarPage(ProdutoAdicionarPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        Dispatcher.DispatchDelayed(TimeSpan.FromMicroseconds(400), async () =>
        {
            await _viewModel.CarregarCategoriasCommand.ExecuteAsync(null);
        });
    }
}
