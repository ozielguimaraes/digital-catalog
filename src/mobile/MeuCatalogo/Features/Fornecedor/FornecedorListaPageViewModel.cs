using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Fornecedor.Domain;
using MeuCatalogo.Features.Fornecedor.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Fornecedor;

public sealed partial class FornecedorListaPageViewModel(
    ILogger<FornecedorListaPageViewModel> logger,
    GetFornecedoresUseCase getFornecedoresUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<FornecedorInfo> _fornecedores = [];
    [ObservableProperty] private bool _hasFornecedores;
    [ObservableProperty] private bool _showEmptyState;
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
                ShowEmptyState = !HasFornecedores;
                return;
            }

            var response = await getFornecedoresUseCase.ExecuteAsync();
            if (response.RetornouComErro)
            {
                logger.LogWarning("Erro ao carregar fornecedores: {Mensagem}", response.MensagemDeErro);
                await Toast.Make(response.MensagemDeErro ?? "Erro ao carregar fornecedores", ToastDuration.Long).Show();
                ShowEmptyState = !HasFornecedores;
                return;
            }

            Fornecedores.Clear();
            foreach (var f in response.Dados!)
                Fornecedores.Add(f);

            HasFornecedores = Fornecedores.Count > 0;
            ShowEmptyState = !HasFornecedores;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao carregar fornecedores");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Voltar() => await navigationService.PopAsync();
}
