using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.UseCases;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class RelatoriosPageViewModel(
    ILogger<RelatoriosPageViewModel> logger,
    GetRelatorioPorCategoriaUseCase getRelatorioUseCase) : BasePageViewModel
{
    [ObservableProperty] private string _ano = DateTime.Today.Year.ToString();
    [ObservableProperty] private string _quantidade = "1";
    [ObservableProperty] private int _mesIndex = DateTime.Today.Month - 1;
    [ObservableProperty] private string _regimeSelecionado = "Caixa";
    [ObservableProperty] private decimal _totalReceitas;
    [ObservableProperty] private decimal _totalDespesas;
    [ObservableProperty] private decimal _saldo;
    [ObservableProperty] private ObservableCollection<RelatorioCategoriaInfo> _receitas = [];
    [ObservableProperty] private ObservableCollection<RelatorioCategoriaInfo> _despesas = [];

    public IReadOnlyList<string> Regimes { get; } = new[] { "Caixa", "Competencia" };
    public IReadOnlyList<string> Meses { get; } = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };

    [RelayCommand]
    private async Task Gerar()
    {
        if (!int.TryParse(Ano, out var ano)) return;
        if (!int.TryParse(Quantidade, out var qtd) || qtd < 1) qtd = 1;
        var regime = RegimeSelecionado == "Caixa" ? RegimeContabil.Caixa : RegimeContabil.Competencia;

        var resp = await getRelatorioUseCase.ExecuteAsync(new RelatorioFinanceiroRequest
        {
            Ano = ano,
            Mes = MesIndex + 1,
            Quantidade = qtd,
            Regime = regime
        });

        if (resp.RetornouComErro)
        {
            await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
            return;
        }

        var d = resp.Dados!;
        TotalReceitas = d.TotalReceitas;
        TotalDespesas = d.TotalDespesas;
        Saldo = d.Saldo;
        Receitas.Clear();
        foreach (var r in d.Receitas) Receitas.Add(r);
        Despesas.Clear();
        foreach (var ds in d.Despesas) Despesas.Add(ds);
    }
}
