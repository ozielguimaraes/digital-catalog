using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Pedido.Domain;
using MeuCatalogo.Features.Pedido.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Pedido;

public sealed partial class PedidoListaPageViewModel(
    ILogger<PedidoListaPageViewModel> logger,
    GetPedidosUseCase getPedidosUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<PedidoInfo> _pedidos = [];
    [ObservableProperty] private bool _hasPedidos;
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
                ShowEmptyState = !HasPedidos;
                return;
            }

            var response = await getPedidosUseCase.ExecuteAsync();
            if (response.RetornouComErro)
            {
                logger.LogWarning("Erro ao carregar pedidos: {Mensagem}", response.MensagemDeErro);
                await Toast.Make(response.MensagemDeErro ?? "Erro ao carregar pedidos", ToastDuration.Long).Show();
                ShowEmptyState = !HasPedidos;
                return;
            }

            Pedidos.Clear();
            foreach (var p in response.Dados!.OrderByDescending(p => p.DataCriacao))
                Pedidos.Add(p);

            HasPedidos = Pedidos.Count > 0;
            ShowEmptyState = !HasPedidos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao carregar pedidos");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NovoPedido()
    {
        await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Em breve",
            "Criação de pedidos pelo app será liberada em breve.",
            "OK");
    }
}
