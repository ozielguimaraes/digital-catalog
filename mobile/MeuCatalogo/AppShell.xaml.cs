using MeuCatalogo.Features.Auth;

namespace MeuCatalogo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        InitializeRouting();
    }

    private static void InitializeRouting()
    {
        //Routing.RegisterRoute(nameof(Catal), typeof(MainPage));
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}
