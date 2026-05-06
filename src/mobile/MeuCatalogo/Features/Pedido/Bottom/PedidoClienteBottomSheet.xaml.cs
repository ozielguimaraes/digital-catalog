namespace MeuCatalogo.Features.Pedido.Bottom;

public partial class PedidoClienteBottomSheet
{
    public PedidoClienteBottomSheet(PedidoClienteBottomSheetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
