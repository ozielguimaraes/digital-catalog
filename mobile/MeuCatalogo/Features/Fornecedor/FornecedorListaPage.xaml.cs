namespace MeuCatalogo.Features.Fornecedor;

public partial class FornecedorListaPage : ContentPage
{
    public FornecedorListaPage(FornecedorListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
