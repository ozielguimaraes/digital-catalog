namespace MeuCatalogo.Features.Cliente;

public partial class ClienteUpsertPage : ContentPage
{
    public ClienteUpsertPage(ClienteUpsertPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
