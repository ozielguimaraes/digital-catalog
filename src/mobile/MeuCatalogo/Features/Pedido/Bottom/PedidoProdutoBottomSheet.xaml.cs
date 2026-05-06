namespace MeuCatalogo.Features.Pedido.Bottom;

public partial class PedidoProdutoBottomSheet
{
    public PedidoProdutoBottomSheet(PedidoProdutoBottomSheetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
