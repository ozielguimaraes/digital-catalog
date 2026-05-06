namespace MeuCatalogo.Features.Pedido;

public partial class PedidoNovoPage : ContentPage
{
    public PedidoNovoPage(PedidoNovoPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
