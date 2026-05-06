using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.UseCases;
using MeuCatalogo.Features.Cliente;
using MeuCatalogo.Features.Financeiro;
using MeuCatalogo.Features.Fornecedor;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Mais;

public sealed partial class MaisPageViewModel(
    ILogger<MaisPageViewModel> logger,
    IAppInfo appInfo,
    GetCurrentUserUseCase getCurrentUserUseCase,
    LogoutUseCase logoutUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private string _nomeUsuario = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _versionName = string.Empty;
    [ObservableProperty] private string _iniciais = "?";

    [RelayCommand]
    private async Task Carregar()
    {
        try
        {
            var user = await getCurrentUserUseCase.ExecuteAsync();
            if (user is not null)
            {
                NomeUsuario = user.Nome;
                Email = user.Email;
                Iniciais = ExtrairIniciais(user.Nome);
            }
            VersionName = $"{appInfo.VersionString} ({appInfo.BuildString})";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar dados do usuário em Mais");
        }
    }

    [RelayCommand]
    private async Task IrClientes()
        => await navigationService.NavigateToAsync(nameof(ClienteListaPage));

    [RelayCommand]
    private async Task IrFornecedores()
        => await navigationService.NavigateToAsync(nameof(FornecedorListaPage));

    [RelayCommand]
    private async Task IrFinanceiro()
        => await navigationService.NavigateToAsync(nameof(FinanceiroPage));

    [RelayCommand]
    private async Task Logout()
    {
        await logoutUseCase.ExecuteAsync();
        var page = Application.Current!.Windows[0].Page!.Handler!.MauiContext!.Services.GetService<LoginPage>();
        Application.Current.Windows[0].Page = page;
        await Task.CompletedTask;
    }

    private static string ExtrairIniciais(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "?";
        if (partes.Length == 1) return partes[0][..1].ToUpperInvariant();
        return string.Concat(partes[0][..1], partes[^1][..1]).ToUpperInvariant();
    }
}
