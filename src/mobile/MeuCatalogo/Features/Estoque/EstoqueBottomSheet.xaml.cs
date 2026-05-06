namespace MeuCatalogo.Features.Estoque;

public partial class EstoqueBottomSheet
{
    public EstoqueBottomSheet(EstoqueBottomSheetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
