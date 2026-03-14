using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Settings.Services;

using MeuCatalogo.Features.Auth.UseCases;

namespace MeuCatalogo;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IAppInfo _appInfo;
    private readonly GetCurrentUserUseCase _getCurrentUserUseCase;
    private readonly GetStartupRouteUseCase _getStartupRouteUseCase;
    private readonly LogoutUseCase _logoutUseCase;

    [ObservableProperty] private string _versionName;
    [ObservableProperty] private string _userDisplayName;
    [ObservableProperty] private string _email;

    public AppShellViewModel(
        IAppInfo appInfo, 
        GetCurrentUserUseCase getCurrentUserUseCase, 
        GetStartupRouteUseCase getStartupRouteUseCase, 
        LogoutUseCase logoutUseCase)
    {
        _appInfo = appInfo;
        _getCurrentUserUseCase = getCurrentUserUseCase;
        _getStartupRouteUseCase = getStartupRouteUseCase;
        _logoutUseCase = logoutUseCase;
    }

    public async Task UpdateUserInfo()
    {
        var userInfo = await _getCurrentUserUseCase.ExecuteAsync();
        if (userInfo == null) return;

        UserDisplayName = userInfo.Nome.Split(' ')[0];
        Email = $"{userInfo.Email}";
        VersionName = $"{_appInfo.VersionString} ({_appInfo.BuildString})";

        await Task.CompletedTask;
    }

    public async Task<string> ObterPaginaInicialAsync()
    {
        return await _getStartupRouteUseCase.ExecuteAsync();
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _logoutUseCase.ExecuteAsync();
        var page = Application.Current!.MainPage!.Handler!.MauiContext!.Services.GetService<LoginPage>();
        Application.Current.MainPage = page;
        await Task.CompletedTask;
    }
}
