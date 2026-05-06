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

public sealed partial class ContaEdicaoPageViewModel(
    ILogger<ContaEdicaoPageViewModel> logger,
    CriarContaUseCase criarContaUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string _cor = "#3F51B5";
    [ObservableProperty] private string _diaFechamento = string.Empty;
    [ObservableProperty] private string _diaVencimento = string.Empty;
    [ObservableProperty] private string _limite = string.Empty;
    [ObservableProperty] private string _saldoInicial = string.Empty;
    [ObservableProperty] private string _tipoSelecionado = "Conta Corrente";

    public IReadOnlyList<string> Tipos { get; } = new[]
    {
        "Carteira", "Conta Corrente", "Cartão de Crédito", "Poupança",
        "Carteira Digital", "Cartão Benefício", "Outros"
    };

    public bool IsCartao => TipoSelecionado == "Cartão de Crédito";
    public bool IsContaSaldo => !IsCartao;

    partial void OnTipoSelecionadoChanged(string value)
    {
        OnPropertyChanged(nameof(IsCartao));
        OnPropertyChanged(nameof(IsContaSaldo));
    }

    [RelayCommand]
    private async Task Salvar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            await Toast.Make("Nome obrigatório", ToastDuration.Short).Show();
            return;
        }

        var tipo = MapTipo(TipoSelecionado);
        byte? diaFech = byte.TryParse(DiaFechamento, out var f) ? f : null;
        byte? diaVenc = byte.TryParse(DiaVencimento, out var v) ? v : null;
        decimal? lim = decimal.TryParse(Limite, out var l) ? l : null;
        decimal saldo = decimal.TryParse(SaldoInicial, out var s) ? s : 0m;

        var resp = await criarContaUseCase.ExecuteAsync(new ContaRequest
        {
            Nome = Nome.Trim(),
            Tipo = tipo,
            Cor = Cor,
            DiaFechamento = diaFech,
            DiaVencimento = diaVenc,
            Limite = lim,
            SaldoInicial = saldo
        });

        if (resp.RetornouComErro)
        {
            await Toast.Make(resp.MensagemDeErro ?? "Erro ao salvar", ToastDuration.Long).Show();
            return;
        }

        await Toast.Make("Conta criada", ToastDuration.Short).Show();
        await navigationService.PopAsync();
    }

    private static ContaTipo MapTipo(string s) => s switch
    {
        "Carteira" => ContaTipo.Carteira,
        "Conta Corrente" => ContaTipo.ContaCorrente,
        "Cartão de Crédito" => ContaTipo.CartaoCredito,
        "Poupança" => ContaTipo.Poupanca,
        "Carteira Digital" => ContaTipo.CarteiraDigital,
        "Cartão Benefício" => ContaTipo.CartaoBeneficio,
        _ => ContaTipo.Outros
    };
}
