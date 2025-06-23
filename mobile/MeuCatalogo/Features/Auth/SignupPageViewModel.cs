using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Validators;
using MeuCatalogo.Features.Catalogo;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Auth;

public partial class SignupPageViewModel : BasePageViewModel
{
    private readonly ILogger<SignupPageViewModel> _logger;
    private readonly IAuthService _authService;

    public SignupPageViewModel(ILogger<SignupPageViewModel> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [ObservableProperty]private string _nome;
    [ObservableProperty]private string _email;
    [ObservableProperty]private string _password;

    [RelayCommand]
    private async Task CreateAccount()
    {
        try
        {
            var request = new SignupRequest(Nome, Email, Email, Password);
            var validator = new SignupValidator(request);
            if (!validator.IsValid)
            {
                var messages = validator.Notifications.Select(x => x.Message);
                var sb = new StringBuilder();

                foreach (var message in messages)
                    sb.Append($"{message}\n");

                await Shell.Current.DisplayAlert("Atenção", sb.ToString(), "OK");
                return;
            }

            var response = await _authService.SignupAsync(request);

            if (response == null)
            {
                await Shell.Current.DisplayAlert("Atenção", "Login e/ou senha está incorreto", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"//{nameof(CatalogoListaPage)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar os dados do sistema.");
            throw;
        }
    }
}
