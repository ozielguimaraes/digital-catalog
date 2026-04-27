namespace MeuCatalogo.Features.Mais;

public partial class MaisPage : ContentPage
{
    public MaisPage(MaisPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
