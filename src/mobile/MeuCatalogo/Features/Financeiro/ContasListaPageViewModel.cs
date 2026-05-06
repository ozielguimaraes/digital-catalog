using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class ContasListaPageViewModel(
    ILogger<ContasListaPageViewModel> logger,
    GetContasUseCase getContasUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<ContaInfo> _contas = [];
    [ObservableProperty] private bool _isLoading;

    [RelayCommand]
    private async Task Carregar()
    {
        try
        {
            if (IsLoading) return;
            IsLoading = true;

            if (!HasInternetConnection())
            {
                await Toast.Make("Sem conexão com a internet", ToastDuration.Long).Show();
                return;
            }

            var resp = await getContasUseCase.ExecuteAsync();
            if (resp.RetornouComErro)
            {
                await Toast.Make(resp.MensagemDeErro ?? "Erro ao listar contas", ToastDuration.Long).Show();
                return;
            }

            Contas.Clear();
            foreach (var c in resp.Dados!) Contas.Add(c);
        }
        catch (Exception ex) { logger.LogError(ex, "Erro contas"); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task NovaConta() => await navigationService.NavigateToAsync(nameof(ContaEdicaoPage));

    [RelayCommand]
    private async Task Voltar() => await navigationService.PopAsync();
}
