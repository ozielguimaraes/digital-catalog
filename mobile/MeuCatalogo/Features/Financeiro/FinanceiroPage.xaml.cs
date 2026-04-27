namespace MeuCatalogo.Features.Financeiro;

public partial class FinanceiroPage : ContentPage
{
    public FinanceiroPage(FinanceiroPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
