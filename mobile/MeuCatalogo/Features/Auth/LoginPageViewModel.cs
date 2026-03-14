using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.UseCases;
using MeuCatalogo.Features.Catalogo;
using Microsoft.Extensions.Logging;
using Plugin.Fingerprint.Abstractions;

namespace MeuCatalogo.Features.Auth;

public sealed partial class LoginPageViewModel : BasePageViewModel
{
    private readonly ILogger<LoginPageViewModel> _logger;
    private readonly SigninUseCase _signinUseCase;
    private readonly SyncAfterLoginUseCase _syncAfterLoginUseCase;
    private readonly IFingerprint _fingerprint;

    public LoginPageViewModel(
        ILogger<LoginPageViewModel> logger,
        SigninUseCase signinUseCase,
        SyncAfterLoginUseCase syncAfterLoginUseCase,
        IFingerprint fingerprint)
    {
        _logger = logger;
        _signinUseCase = signinUseCase;
        _syncAfterLoginUseCase = syncAfterLoginUseCase;
        _fingerprint = fingerprint;
#if  DEBUG
        Email = "microzapple@gmail.com";
        Password = "Asdf@1234";
#endif
    }

    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private string? _busyMessage;

    [RelayCommand]
    private async Task Login()
    {
        try
        {
            if (IsBusy)
                return;

            if (!HasInternetConnection())
            {
                await Toast.Make("Sem conexão com a internet", ToastDuration.Long).Show();
                return;
            }

            IsBusy = true;
            BusyMessage = "Entrando…";

            var request = new SigninRequest(Email: Email, Password: Password);

            var signinResponse = await _signinUseCase.ExecuteAsync(request);

            if (signinResponse.RetornouComErro)
            {
                string? errorMessage = signinResponse.MensagemDeErro ?? "Erro ao efetuar login";
                if (signinResponse.ProblemDetails != null && !string.IsNullOrEmpty(signinResponse.ProblemDetails.Detail))
                {
                    errorMessage = signinResponse.ProblemDetails.Detail;
                }

                await Toast.Make(errorMessage, ToastDuration.Long).Show();
                return;
            }

            if (string.IsNullOrEmpty(signinResponse.Dados!.Token))
            {
                _logger.LogError("Token de autenticação retornado é nulo ou vazio.");
                await Toast.Make("A requisição falhou, tente novamente.", ToastDuration.Long).Show();
                return;
            }

            BusyMessage = "Sincronizando dados…";
            await _syncAfterLoginUseCase.ExecuteAsync();

            var appShellViewModel = Application.Current.MainPage.Handler.MauiContext.Services.GetService<AppShellViewModel>();
            Application.Current.MainPage = new AppShell(appShellViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            await Application.Current.MainPage.DisplayAlert("Oops", "Ocorreu um erro inesperado, se persistir contacte o desenvolvedor", "OK");
        }
        finally
        {
            BusyMessage = null;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateAccount()
    {
        await Task.CompletedTask;
        //await Shell.Current.GoToAsync($"//{nameof(SignupPage)}");
    }

    [RelayCommand]
    private async Task ValidateBiometric()
    {
        bool isValid = await _fingerprint.IsAvailableAsync();

        if (!isValid)
        {
            await Shell.Current.DisplayAlert("Atenção!", "Dispositivo não contém sensor biométrica", "OK");
            return;
        }

        if (!string.IsNullOrEmpty(Preferences.Default.Get("token", string.Empty)))
        {
            var request = new AuthenticationRequestConfiguration("Validação biométrica", "Precisamos validar sua biometria para prosseguir");
            var result = await _fingerprint.AuthenticateAsync(request);
            if (result.Authenticated)
            {
                await Shell.Current.GoToAsync($"//{nameof(CatalogoListaPage)}");
            }
            else
                await Shell.Current.DisplayAlert("Não autenticado", "Acesso negado", "OK");
        }
    }
}
