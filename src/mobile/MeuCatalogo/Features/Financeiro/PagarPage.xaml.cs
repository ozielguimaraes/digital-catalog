namespace MeuCatalogo.Features.Financeiro;

public partial class PagarPage : ContentPage
{
    public PagarPage(PagarPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
