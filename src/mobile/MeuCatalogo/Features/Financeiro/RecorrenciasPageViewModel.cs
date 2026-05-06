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

public sealed partial class RecorrenciasPageViewModel(
    ILogger<RecorrenciasPageViewModel> logger,
    GetRecorrenciasUseCase getRecorrenciasUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<RecorrenciaInfo> _recorrencias = [];
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
                await Toast.Make("Sem conexão", ToastDuration.Long).Show();
                return;
            }
            var resp = await getRecorrenciasUseCase.ExecuteAsync();
            if (resp.RetornouComErro)
            {
                await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
                return;
            }
            Recorrencias.Clear();
            foreach (var r in resp.Dados!) Recorrencias.Add(r);
        }
        catch (Exception ex) { logger.LogError(ex, "Erro recorrências"); }
        finally { IsLoading = false; }
    }
}
