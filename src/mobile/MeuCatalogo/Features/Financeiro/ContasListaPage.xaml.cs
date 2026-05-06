namespace MeuCatalogo.Features.Financeiro;

public partial class ContasListaPage : ContentPage
{
    public ContasListaPage(ContasListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ContasListaPageViewModel vm) await vm.CarregarCommand.ExecuteAsync(null);
    }
}
