using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class FinanceiroPageViewModel(
    ILogger<FinanceiroPageViewModel> logger,
    GetFinanceiroResumoUseCase getResumoUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private decimal _totalAReceber;
    [ObservableProperty] private decimal _totalAPagar;
    [ObservableProperty] private decimal _saldoMes;
    [ObservableProperty] private string _periodoLabel = string.Empty;
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

            var response = await getResumoUseCase.ExecuteAsync();
            if (response.RetornouComErro)
            {
                logger.LogWarning("Erro ao obter resumo financeiro: {Mensagem}", response.MensagemDeErro);
                await Toast.Make(response.MensagemDeErro ?? "Erro ao obter resumo", ToastDuration.Long).Show();
                return;
            }

            var resumo = response.Dados!;
            TotalAReceber = resumo.TotalAReceber;
            TotalAPagar = resumo.TotalAPagar;
            SaldoMes = resumo.SaldoPrevisto;
            PeriodoLabel = resumo.PeriodoLabel;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao obter resumo financeiro");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task VerReceber() => await navigationService.NavigateToAsync(nameof(ReceberPage));

    [RelayCommand]
    private async Task VerPagar() => await navigationService.NavigateToAsync(nameof(PagarPage));

    [RelayCommand]
    private async Task Voltar() => await navigationService.PopAsync();
}
