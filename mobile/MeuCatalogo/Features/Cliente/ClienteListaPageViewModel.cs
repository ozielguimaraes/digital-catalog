using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Cliente.Domain;
using MeuCatalogo.Features.Cliente.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Cliente;

public sealed partial class ClienteListaPageViewModel(
    ILogger<ClienteListaPageViewModel> logger,
    GetClientesUseCase getClientesUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<ClienteInfo> _clientes = [];
    [ObservableProperty] private bool _hasClientes;
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
                ShowEmptyState = !HasClientes;
                return;
            }

            var response = await getClientesUseCase.ExecuteAsync();
            if (response.RetornouComErro)
            {
                logger.LogWarning("Erro ao carregar clientes: {Mensagem}", response.MensagemDeErro);
                await Toast.Make(response.MensagemDeErro ?? "Erro ao carregar clientes", ToastDuration.Long).Show();
                ShowEmptyState = !HasClientes;
                return;
            }

            Clientes.Clear();
            foreach (var c in response.Dados!.OrderBy(c => c.Nome))
                Clientes.Add(c);

            HasClientes = Clientes.Count > 0;
            ShowEmptyState = !HasClientes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao carregar clientes");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Voltar() => await navigationService.PopAsync();
}
