namespace MeuCatalogo.Features.Categoria;

public partial class CategoriaBottomSheet
{
    public CategoriaBottomSheet(CategoriaBottomSheetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
