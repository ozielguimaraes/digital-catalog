using System;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using MeuCatalogo.Infrastructure.Database;
using Microsoft.Maui;

namespace MeuCatalogo;

public partial class App
{
    private readonly IAuthRepository _authRepository;
    private readonly INavigationService _navigationService;

    public App(IAuthRepository authRepository, INavigationService navigationService)
    {
        _authRepository = authRepository;
        _navigationService = navigationService;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (_authRepository.IsAuthenticated())
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
                var db = IPlatformApplication.Current?.Services.GetService<AppDbContext>();
                if (db != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await db.InitializeAsync();
                        }
                        catch
                        {
                        }
                    });
                }

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
