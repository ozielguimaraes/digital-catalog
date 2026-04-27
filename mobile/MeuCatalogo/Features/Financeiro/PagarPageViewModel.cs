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

public sealed partial class PagarPageViewModel(
    ILogger<PagarPageViewModel> logger,
    GetLancamentosUseCase getLancamentosUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<LancamentoInfo> _lancamentos = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _showEmptyState;

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

            var response = await getLancamentosUseCase.ExecuteAsync(LancamentoTipo.Pagar);
            if (response.RetornouComErro)
            {
                logger.LogWarning("Erro ao carregar a pagar: {Mensagem}", response.MensagemDeErro);
                await Toast.Make(response.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
                return;
            }

            Lancamentos.Clear();
            foreach (var l in response.Dados!)
                Lancamentos.Add(l);

            ShowEmptyState = Lancamentos.Count == 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar a pagar");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Voltar() => await navigationService.PopAsync();
}
