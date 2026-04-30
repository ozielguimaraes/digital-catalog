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

public sealed partial class BaixaPageViewModel(
    ILogger<BaixaPageViewModel> logger,
    GetContasUseCase getContasUseCase,
    RegistrarBaixaUseCase registrarBaixaUseCase,
    INavigationService navigationService) : BasePageViewModel, IQueryAttributable
{
    [ObservableProperty] private string _lancamentoDescricao = string.Empty;
    [ObservableProperty] private decimal _valorEmAberto;
    [ObservableProperty] private string _valor = string.Empty;
    [ObservableProperty] private DateTime _data = DateTime.Today;
    [ObservableProperty] private string _observacoes = string.Empty;
    [ObservableProperty] private ObservableCollection<ContaInfo> _contas = [];
    [ObservableProperty] private ContaInfo? _contaSelecionada;

    private Guid _lancamentoId;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("lancamentoId", out var idObj) && Guid.TryParse(idObj.ToString(), out var id))
            _lancamentoId = id;
        if (query.TryGetValue("descricao", out var d)) LancamentoDescricao = d.ToString() ?? string.Empty;
        if (query.TryGetValue("valorEmAberto", out var v) && decimal.TryParse(v.ToString(), out var dec))
            ValorEmAberto = dec;

        var resp = await getContasUseCase.ExecuteAsync();
        if (!resp.RetornouComErro && resp.Dados is not null)
        {
            Contas.Clear();
            foreach (var c in resp.Dados.Where(x => !x.EhCartaoCredito)) Contas.Add(c);
            ContaSelecionada = Contas.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task Salvar()
    {
        if (_lancamentoId == Guid.Empty) return;
        if (ContaSelecionada == null)
        {
            await Toast.Make("Escolha uma conta", ToastDuration.Short).Show();
            return;
        }
        if (!decimal.TryParse(Valor, out var v) || v <= 0)
        {
            await Toast.Make("Valor inválido", ToastDuration.Short).Show();
            return;
        }
        var resp = await registrarBaixaUseCase.ExecuteAsync(_lancamentoId, new LancamentoBaixaRequest
        {
            Data = Data,
            Valor = v,
            ContaId = ContaSelecionada.Id,
            Observacoes = Observacoes
        });
        if (resp.RetornouComErro)
        {
            await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
            return;
        }
        await Toast.Make("Baixa registrada", ToastDuration.Short).Show();
        await navigationService.PopAsync();
    }
}
