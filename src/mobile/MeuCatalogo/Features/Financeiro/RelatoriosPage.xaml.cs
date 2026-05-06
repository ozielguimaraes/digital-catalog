namespace MeuCatalogo.Features.Financeiro;

public partial class RelatoriosPage : ContentPage
{
    public RelatoriosPage(RelatoriosPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
