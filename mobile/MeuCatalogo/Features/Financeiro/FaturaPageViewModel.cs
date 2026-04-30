using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.UseCases;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class FaturaPageViewModel(
    ILogger<FaturaPageViewModel> logger,
    GetContasUseCase getContasUseCase,
    GetFaturaUseCase getFaturaUseCase) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<ContaInfo> _contas = [];
    [ObservableProperty] private ContaInfo? _contaSelecionada;
    [ObservableProperty] private int _mesIndex = DateTime.Today.Month - 1;
    [ObservableProperty] private string _ano = DateTime.Today.Year.ToString();
    [ObservableProperty] private decimal _valorTotal;
    [ObservableProperty] private decimal? _valorPago;
    [ObservableProperty] private decimal _valorEmAberto;
    [ObservableProperty] private DateTime _dataVencimento;
    [ObservableProperty] private ObservableCollection<LancamentoInfo> _lancamentos = [];

    public IReadOnlyList<string> Meses { get; } = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };

    partial void OnContaSelecionadaChanged(ContaInfo? value) => _ = Carregar();
    partial void OnMesIndexChanged(int value) => _ = Carregar();
    partial void OnAnoChanged(string value) => _ = Carregar();

    [RelayCommand]
    private async Task CarregarCartoes()
    {
        var resp = await getContasUseCase.ExecuteAsync();
        if (resp.RetornouComErro || resp.Dados is null) return;
        Contas.Clear();
        foreach (var c in resp.Dados.Where(c => c.EhCartaoCredito)) Contas.Add(c);
        ContaSelecionada ??= Contas.FirstOrDefault();
    }

    private async Task Carregar()
    {
        if (ContaSelecionada == null) return;
        if (!int.TryParse(Ano, out var ano)) return;
        var resp = await getFaturaUseCase.ExecuteAsync(ContaSelecionada.Id, MesIndex + 1, ano);
        if (resp.RetornouComErro || resp.Dados is null)
        {
            await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
            return;
        }
        var f = resp.Dados;
        ValorTotal = f.ValorTotal;
        ValorPago = f.ValorPago;
        ValorEmAberto = f.ValorEmAberto;
        DataVencimento = f.DataVencimento;
        Lancamentos.Clear();
        foreach (var l in f.Lancamentos) Lancamentos.Add(l);
    }
}
