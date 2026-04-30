namespace MeuCatalogo.Features.Financeiro;

public partial class BaixaPage : ContentPage
{
    public BaixaPage(BaixaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
