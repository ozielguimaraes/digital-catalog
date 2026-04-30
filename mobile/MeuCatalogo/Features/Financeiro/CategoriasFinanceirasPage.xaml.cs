namespace MeuCatalogo.Features.Financeiro;

public partial class CategoriasFinanceirasPage : ContentPage
{
    public CategoriasFinanceirasPage(CategoriasFinanceirasPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CategoriasFinanceirasPageViewModel vm) await vm.CarregarCommand.ExecuteAsync(null);
    }
}
