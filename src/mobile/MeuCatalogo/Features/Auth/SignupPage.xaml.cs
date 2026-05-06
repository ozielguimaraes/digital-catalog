namespace MeuCatalogo.Features.Auth;

public partial class SignupPage : ContentPage
{
    public SignupPage(SignupPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

