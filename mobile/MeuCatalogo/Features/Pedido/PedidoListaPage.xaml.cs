namespace MeuCatalogo.Features.Pedido;

public partial class PedidoListaPage : ContentPage
{
    public PedidoListaPage(PedidoListaPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
