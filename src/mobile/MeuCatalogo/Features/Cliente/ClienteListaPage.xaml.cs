namespace MeuCatalogo.Features.Cliente;

public partial class ClienteListaPage : ContentPage
{
    public ClienteListaPage(ClienteListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
