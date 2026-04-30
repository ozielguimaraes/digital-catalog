using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class ExtratoPageViewModel(
    ILogger<ExtratoPageViewModel> logger,
    GetContasUseCase getContasUseCase,
    GetExtratoPorContaUseCase getExtratoPorContaUseCase,
    GetExtratoConsolidadoUseCase getExtratoConsolidadoUseCase) : BasePageViewModel
{
    private static readonly ContaInfo TodasContas = new()
    {
        Id = Guid.Empty,
        Nome = "Todas as contas (consolidado)"
    };

    [ObservableProperty] private ObservableCollection<ContaInfo> _contas = [];
    [ObservableProperty] private ContaInfo? _contaSelecionada;
    [ObservableProperty] private DateTime _dataInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime _dataFim = DateTime.Today;
    [ObservableProperty] private decimal _saldoInicial;
    [ObservableProperty] private decimal _saldoFinal;
    [ObservableProperty] private decimal _totalEntradas;
    [ObservableProperty] private decimal _totalSaidas;
    [ObservableProperty] private ObservableCollection<ExtratoListaItem> _itensExibidos = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _inicializado;

    [RelayCommand]
    private async Task Inicializar()
    {
        if (Inicializado) return;
        await CarregarContasAsync();
        Inicializado = true;
        await Carregar();
    }

    private async Task CarregarContasAsync()
    {
        if (!HasInternetConnection())
        {
            await Toast.Make("Sem conexão", ToastDuration.Long).Show();
            return;
        }
        var resp = await getContasUseCase.ExecuteAsync();
        if (resp.RetornouComErro || resp.Dados is null) return;

        Contas.Clear();
        Contas.Add(TodasContas);
        foreach (var c in resp.Dados.Where(c => !c.EhCartaoCredito))
            Contas.Add(c);
        ContaSelecionada ??= TodasContas;
    }

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
            if (ContaSelecionada == null) return;
            if (DataFim < DataInicio)
            {
                await Toast.Make("Data fim deve ser maior que data início", ToastDuration.Short).Show();
                return;
            }

            var resp = ContaSelecionada.Id == Guid.Empty
                ? await getExtratoConsolidadoUseCase.ExecuteAsync(DataInicio, DataFim)
                : await getExtratoPorContaUseCase.ExecuteAsync(ContaSelecionada.Id, DataInicio, DataFim);

            if (resp.RetornouComErro || resp.Dados is null)
            {
                await Toast.Make(resp.MensagemDeErro ?? "Erro ao carregar extrato", ToastDuration.Long).Show();
                return;
            }

            Atualizar(resp.Dados);
        }
        catch (Exception ex) { logger.LogError(ex, "Erro extrato"); }
        finally { IsLoading = false; }
    }

    private void Atualizar(ExtratoInfo extrato)
    {
        SaldoInicial = extrato.SaldoInicial;
        SaldoFinal = extrato.SaldoFinal;
        TotalEntradas = extrato.TotalEntradas;
        TotalSaidas = extrato.TotalSaidas;

        ItensExibidos.Clear();
        var saldosPorDia = extrato.SaldosDiarios.ToDictionary(s => s.Data.Date);
        DateTime? diaAtual = null;
        foreach (var m in extrato.Movimentos.OrderBy(m => m.Data).ThenBy(m => m.Id))
        {
            var dia = m.Data.Date;
            if (diaAtual != dia)
            {
                diaAtual = dia;
                saldosPorDia.TryGetValue(dia, out var resumo);
                ItensExibidos.Add(ExtratoListaItem.Cabecalho(dia, resumo?.SaldoFinalDia ?? 0m));
            }
            ItensExibidos.Add(ExtratoListaItem.Movimento(m));
        }
    }

    partial void OnContaSelecionadaChanged(ContaInfo? value)
    {
        if (Inicializado) _ = Carregar();
    }
}

public sealed record ExtratoListaItem
{
    public bool IsCabecalhoDia { get; init; }
    public bool IsMovimento => !IsCabecalhoDia;
    public string DataLabel { get; init; } = string.Empty;
    public decimal SaldoFinalDia { get; init; }
    public ExtratoMovimentoInfo? Movimento { get; init; }

    public string CorBarra => Movimento?.Tipo == ExtratoMovimentoTipo.Entrada ? "#2E7D32" : "#C62828";
    public string CorValor => Movimento?.Tipo == ExtratoMovimentoTipo.Entrada ? "#2E7D32" : "#C62828";

    public string LegendaSecundaria
    {
        get
        {
            if (Movimento is null) return string.Empty;
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(Movimento.CategoriaNome)) partes.Add(Movimento.CategoriaNome);
            if (!string.IsNullOrWhiteSpace(Movimento.ContaNome)) partes.Add(Movimento.ContaNome);
            return string.Join(" · ", partes);
        }
    }

    public static ExtratoListaItem Cabecalho(DateTime data, decimal saldoFinalDia) => new()
    {
        IsCabecalhoDia = true,
        DataLabel = data.ToString("dddd, dd 'de' MMMM", CultureInfo.GetCultureInfo("pt-BR")),
        SaldoFinalDia = saldoFinalDia
    };

    public static ExtratoListaItem Movimento(ExtratoMovimentoInfo m) => new()
    {
        IsCabecalhoDia = false,
        Movimento = m
    };
}
