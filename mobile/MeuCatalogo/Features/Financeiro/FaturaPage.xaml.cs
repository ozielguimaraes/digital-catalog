namespace MeuCatalogo.Features.Financeiro;

public partial class FaturaPage : ContentPage
{
    public FaturaPage(FaturaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FaturaPageViewModel vm) await vm.CarregarCartoesCommand.ExecuteAsync(null);
    }
}
