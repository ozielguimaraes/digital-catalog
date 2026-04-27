namespace MeuCatalogo.Features.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
