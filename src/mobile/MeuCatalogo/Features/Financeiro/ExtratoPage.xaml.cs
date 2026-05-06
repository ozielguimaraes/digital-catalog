namespace MeuCatalogo.Features.Financeiro;

public partial class ExtratoPage : ContentPage
{
    public ExtratoPage(ExtratoPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ExtratoPageViewModel vm) await vm.InicializarCommand.ExecuteAsync(null);
    }
}
