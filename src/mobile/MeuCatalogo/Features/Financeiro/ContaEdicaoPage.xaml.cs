namespace MeuCatalogo.Features.Financeiro;

public partial class ContaEdicaoPage : ContentPage
{
    public ContaEdicaoPage(ContaEdicaoPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
