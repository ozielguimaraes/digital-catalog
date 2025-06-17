using System;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using Microsoft.Maui;

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
        if (_authService.IsAuthenticated())
        {
            var appShellViewModel = IPlatformApplication.Current!.Services.GetService<AppShellViewModel>()
                                    ?? throw new InvalidOperationException("LoginPageViewModel is not registered in the DI container.");
            return new Window(new AppShell(appShellViewModel));
        }

        var loginPageViewModel = IPlatformApplication.Current!.Services.GetService<LoginPageViewModel>()
                                 ?? throw new InvalidOperationException("LoginPageViewModel is not registered in the DI container.");

        return new Window(new LoginPage(loginPageViewModel));
    }
}
