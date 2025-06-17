using System.Reflection;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Responses;

namespace MeuCatalogo;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IAppInfo _appInfo;

    [ObservableProperty] private string _versionName;
    [ObservableProperty] private string _userDisplayName;
    [ObservableProperty] private string _email;

    public AppShellViewModel(IAppInfo appInfo)
    {
        _appInfo = appInfo;
    }

    public async Task UpdateUserInfo()
    {
        var userInfo = JsonSerializer.Deserialize<UserResponse>(await SecureStorage.GetAsync(BaseApiService.UserInfoKey));

        UserDisplayName = userInfo!.Nome.Split(' ')[0];
        Email = $"{userInfo!.Email}";
        VersionName = $"{_appInfo.VersionString} ({_appInfo.BuildString})";
    }

    [RelayCommand]
    private async Task Logout()
    {

        var page = Application.Current!.MainPage!.Handler!.MauiContext!.Services.GetService<LoginPage>();
        Application.Current.MainPage = page;
    }
}
