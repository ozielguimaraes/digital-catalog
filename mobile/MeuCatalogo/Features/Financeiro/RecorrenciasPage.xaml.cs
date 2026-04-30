namespace MeuCatalogo.Features.Financeiro;

public partial class RecorrenciasPage : ContentPage
{
    public RecorrenciasPage(RecorrenciasPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RecorrenciasPageViewModel vm) await vm.CarregarCommand.ExecuteAsync(null);
    }
}
