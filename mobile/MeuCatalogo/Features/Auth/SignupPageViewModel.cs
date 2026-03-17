using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Auth.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Auth.UseCases;
using MeuCatalogo.Features.Auth.Validators;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Auth;

public partial class SignupPageViewModel(
    ILogger<SignupPageViewModel> logger,
    SignupUseCase signupUseCase,
    INavigationService navigationService)
    : BasePageViewModel
{
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

            var response = await signupUseCase.ExecuteAsync(request);

            if (response.RetornouComErro)
            {
                var errors = ObterErros(response);
                await Shell.Current.DisplayAlert("Atenção", string.Join("\n", errors), "OK");
                return;
            }

            await navigationService.NavigateToAsync($"//{nameof(CatalogoListaPage)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar os dados do sistema.");
            throw;
        }
    }
}
