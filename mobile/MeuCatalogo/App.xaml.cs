using System;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Maui;

namespace MeuCatalogo;

public partial class App
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public App(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (_authService.IsAuthenticated())
        {
            var appShellViewModel = IPlatformApplication.Current!.Services.GetService<AppShellViewModel>()
                                    ?? throw new InvalidOperationException("AppShellViewModel is not registered in the DI container.");

            var settingsService = IPlatformApplication.Current.Services.GetService<ISettingsService>()
                                  ?? throw new InvalidOperationException("SettingsService is not registered in the DI container.");

            var shell = new AppShell(appShellViewModel);

            return new Window(shell);
        }

        var loginPageViewModel = IPlatformApplication.Current!.Services.GetService<LoginPageViewModel>()
                                 ?? throw new InvalidOperationException("LoginPageViewModel is not registered in the DI container.");

        return new Window(new LoginPage(loginPageViewModel));
    }


    protected override async void OnHandlerChanged()
    {
        try
        {
            base.OnHandlerChanged();

            if (Handler is not null)
            {
                // await _navigationService.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
