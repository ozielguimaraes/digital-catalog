using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Extensions;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Auth.Requests;
using MeuCatalogo.Features.Auth.Validators;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;

namespace MeuCatalogo.Features.Auth;

public partial class LoginPageViewModel(IAuthService authService) : BasePageViewModel
{
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _password;

    [RelayCommand]
    private async Task Login()
    {
        if (Connectivity.NetworkAccess.HasInternetConnection())
        {
            await Toast.Make("Sem conexão com a internet", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            return;
        }

        var request = new SigninRequest(UserName: Email, Password: Password);

        var validator = new SigninValidator(request);
        if (!validator.IsValid)
        {
            var messages = validator.Notifications.Select(x => x.Message);
            var sb = new StringBuilder();

            foreach (var message in messages)
                sb.Append($"{message}\n");

            await Shell.Current.DisplayAlert("Atenção", sb.ToString(), "OK");
            return;
        }

        var result = await authService.SigninAsync(request);

        if (string.IsNullOrEmpty(result.Token))
        {
            await Toast.Make("A requição falhou, tente novamente.", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            return;
        }

        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    public void SetupTitle()
    {
        Title = "Login";
    }
}
