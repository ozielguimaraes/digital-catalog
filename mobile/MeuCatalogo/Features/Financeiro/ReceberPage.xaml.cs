namespace MeuCatalogo.Features.Financeiro;

public partial class ReceberPage : ContentPage
{
    public ReceberPage(ReceberPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
