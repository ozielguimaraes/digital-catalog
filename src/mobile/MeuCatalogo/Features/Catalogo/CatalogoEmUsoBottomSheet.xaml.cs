namespace MeuCatalogo.Features.Catalogo;

public partial class CatalogoEmUsoBottomSheet
{
    public CatalogoEmUsoBottomSheet(CatalogoEmUsoBottomSheetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
