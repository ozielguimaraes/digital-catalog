using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;

namespace MeuCatalogo;

public partial class App
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        _authService = authService;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPageViewModel = Current!.MainPage!.Handler!.MauiContext!.Services.GetService<LoginPageViewModel>();
        return _authService.IsAuthenticated() ? new Window(new AppShell()) : new Window( new LoginPage(loginPageViewModel));
    }
}
