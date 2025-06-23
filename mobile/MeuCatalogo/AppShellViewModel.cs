using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Auth.Responses;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IAppInfo _appInfo;
    private readonly IAuthService _authService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private string _versionName;
    [ObservableProperty] private string _userDisplayName;
    [ObservableProperty] private string _email;

    public AppShellViewModel(IAppInfo appInfo, IAuthService authService, ISettingsService settingsService)
    {
        _appInfo = appInfo;
        _authService = authService;
        _settingsService = settingsService;
    }

    public async Task UpdateUserInfo()
    {
        var userInfo = JsonSerializer.Deserialize<UserResponse>(await SecureStorage.GetAsync(BaseApiService.UserInfoKey));

        UserDisplayName = userInfo!.Nome.Split(' ')[0];
        Email = $"{userInfo!.Email}";
        VersionName = $"{_appInfo.VersionString} ({_appInfo.BuildString})";
    }

    public string ObterPaginaInicial()
    {
        bool isAuthenticated =  _authService.IsAuthenticated();
        bool possuiCatalogoFavorito = _settingsService.CatalogoFavorito is not null;

        string targetPage = isAuthenticated
            ? (possuiCatalogoFavorito ? nameof(ProdutoAdicionarPage) : nameof(CatalogoListaPage))
            : nameof(LoginPage);

        return targetPage;
    }

    [RelayCommand]
    private async Task Logout()
    {
        var page = Application.Current!.MainPage!.Handler!.MauiContext!.Services.GetService<LoginPage>();
        Application.Current.MainPage = page;
        await Task.CompletedTask;
    }
}
