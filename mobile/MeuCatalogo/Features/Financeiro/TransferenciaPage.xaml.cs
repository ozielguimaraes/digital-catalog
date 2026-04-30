namespace MeuCatalogo.Features.Financeiro;

public partial class TransferenciaPage : ContentPage
{
    public TransferenciaPage(TransferenciaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TransferenciaPageViewModel vm) await vm.CarregarContasCommand.ExecuteAsync(null);
    }
}
