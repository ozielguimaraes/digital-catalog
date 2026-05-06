using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class TransferenciaPageViewModel(
    ILogger<TransferenciaPageViewModel> logger,
    GetContasUseCase getContasUseCase,
    CriarTransferenciaUseCase criarTransferenciaUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<ContaInfo> _contasOrigem = [];
    [ObservableProperty] private ObservableCollection<ContaInfo> _contasDestino = [];
    [ObservableProperty] private ContaInfo? _contaOrigem;
    [ObservableProperty] private ContaInfo? _contaDestino;
    [ObservableProperty] private string _valor = string.Empty;
    [ObservableProperty] private DateTime _data = DateTime.Today;
    [ObservableProperty] private string _descricao = string.Empty;

    [RelayCommand]
    private async Task CarregarContas()
    {
        var resp = await getContasUseCase.ExecuteAsync();
        if (resp.RetornouComErro || resp.Dados is null) return;
        ContasOrigem.Clear();
        ContasDestino.Clear();
        foreach (var c in resp.Dados.Where(x => !x.EhCartaoCredito)) ContasOrigem.Add(c);
        foreach (var c in resp.Dados.Where(x => !x.EhCartaoCredito)) ContasDestino.Add(c);
    }

    [RelayCommand]
    private async Task Salvar()
    {
        if (ContaOrigem == null || ContaDestino == null)
        {
            await Toast.Make("Selecione origem e destino", ToastDuration.Short).Show();
            return;
        }
        if (ContaOrigem.Id == ContaDestino.Id)
        {
            await Toast.Make("Origem e destino devem ser diferentes", ToastDuration.Short).Show();
            return;
        }
        if (!decimal.TryParse(Valor, out var v) || v <= 0)
        {
            await Toast.Make("Valor inválido", ToastDuration.Short).Show();
            return;
        }

        var resp = await criarTransferenciaUseCase.EntreContasAsync(new TransferenciaRequest
        {
            Data = Data,
            ContaOrigemId = ContaOrigem.Id,
            ContaDestinoId = ContaDestino.Id,
            Valor = v,
            Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao
        });

        if (resp.RetornouComErro)
        {
            await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
            return;
        }
        await Toast.Make("Transferência registrada", ToastDuration.Short).Show();
        await navigationService.PopAsync();
    }
}
