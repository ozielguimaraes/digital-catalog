using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Cliente.Domain;
using MeuCatalogo.Features.Pedido.Bottom;
using MeuCatalogo.Features.Pedido.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Pedido.UseCases;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Pedido;

public sealed partial class PedidoNovoPageViewModel(
    ILogger<PedidoNovoPageViewModel> logger,
    CreatePedidoUseCase createPedidoUseCase,
    ISettingsService settingsService,
    INavigationService navigationService,
    IBottomSheetNavigationService bottomSheetNavigationService) : BasePageViewModel, INavigationAware
{
    [ObservableProperty] private ClienteInfo? _clienteSelecionado;
    [ObservableProperty] private ObservableCollection<PedidoNovoItemModel> _itens = [];
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _hasItens;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string? _erro;

    public string CatalogoEmUsoNome => settingsService.CatalogoEmUso?.Nome ?? "—";

    [RelayCommand]
    private async Task Inicializar()
    {
        if (settingsService.CatalogoEmUso is null)
            await AbrirSelecaoCatalogo();
        OnPropertyChanged(nameof(CatalogoEmUsoNome));
    }

    [RelayCommand]
    private Task SelecionarCliente()
        => bottomSheetNavigationService.NavigateToAsync<PedidoClienteBottomSheetViewModel>(BottomSheetKeys.PedidoCliente);

    [RelayCommand]
    private async Task AdicionarItem()
    {
        if (settingsService.CatalogoEmUso is null)
        {
            await AbrirSelecaoCatalogo();
            if (settingsService.CatalogoEmUso is null) return;
        }

        await bottomSheetNavigationService.NavigateToAsync<PedidoProdutoBottomSheetViewModel>(BottomSheetKeys.PedidoProduto);
    }

    [RelayCommand]
    private void RemoverItem(PedidoNovoItemModel item)
    {
        Itens.Remove(item);
        RecalcularTotal();
    }

    [RelayCommand]
    private void IncrementarItem(PedidoNovoItemModel item) => item.Quantidade += 1;

    [RelayCommand]
    private void DecrementarItem(PedidoNovoItemModel item)
    {
        if (item.Quantidade > 1) item.Quantidade -= 1;
    }

    [RelayCommand]
    private async Task TrocarCatalogo() => await AbrirSelecaoCatalogo();

    [RelayCommand]
    private async Task Salvar()
    {
        try
        {
            if (IsSaving) return;

            if (ClienteSelecionado is null)
            {
                Erro = "Selecione um cliente.";
                return;
            }
            if (Itens.Count == 0)
            {
                Erro = "Adicione ao menos um item.";
                return;
            }
            if (Itens.Any(i => i.Quantidade <= 0))
            {
                Erro = "Quantidade dos itens deve ser maior que zero.";
                return;
            }
            if (!HasInternetConnection())
            {
                await Toast.Make("Sem conexão com a internet", ToastDuration.Long).Show();
                return;
            }

            IsSaving = true;
            Erro = null;

            var request = new PedidoCreateRequest
            {
                ClienteId = ClienteSelecionado.Id,
                Itens = Itens.Select(i => new ItemPedidoCreateRequest
                {
                    ProdutoId = i.Produto.Id,
                    VariacaoId = null,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            var resp = await createPedidoUseCase.ExecuteAsync(request);
            if (resp.RetornouComErro)
            {
                await Toast.Make(resp.MensagemDeErro ?? "Erro ao criar pedido", ToastDuration.Long).Show();
                return;
            }

            await navigationService.PopAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar pedido");
        }
        finally
        {
            IsSaving = false;
        }
    }

    public void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        if (parameters.TryGetValue(BottomSheetParameters.ClienteSelecionado, out var c) && c is ClienteInfo cliente)
        {
            ClienteSelecionado = cliente;
            Erro = null;
        }

        if (parameters.TryGetValue(BottomSheetParameters.ProdutoSelecionado, out var p) && p is ProdutoResponse produto)
        {
            var existente = Itens.FirstOrDefault(i => i.Produto.Id == produto.Id);
            if (existente is not null)
            {
                existente.Quantidade += 1;
            }
            else
            {
                var novo = new PedidoNovoItemModel(produto);
                novo.PropertyChanged += (_, _) => RecalcularTotal();
                Itens.Add(novo);
            }
            RecalcularTotal();
        }

        if (parameters.TryGetValue(BottomSheetParameters.CatalogoSelecionado, out var k) && k is CatalogoInfo)
        {
            OnPropertyChanged(nameof(CatalogoEmUsoNome));
        }
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters) { }

    private Task AbrirSelecaoCatalogo()
        => bottomSheetNavigationService.NavigateToAsync<CatalogoEmUsoBottomSheetViewModel>(BottomSheetKeys.CatalogoEmUso);

    private void RecalcularTotal()
    {
        Total = Itens.Sum(i => i.Subtotal);
        HasItens = Itens.Count > 0;
    }
}
