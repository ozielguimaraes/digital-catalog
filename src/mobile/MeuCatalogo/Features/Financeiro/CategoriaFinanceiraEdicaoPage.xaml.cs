namespace MeuCatalogo.Features.Financeiro;

public partial class CategoriaFinanceiraEdicaoPage : ContentPage
{
    public CategoriaFinanceiraEdicaoPage(CategoriaFinanceiraEdicaoPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
