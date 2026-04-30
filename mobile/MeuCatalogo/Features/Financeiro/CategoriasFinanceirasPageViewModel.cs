using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class CategoriasFinanceirasPageViewModel(
    ILogger<CategoriasFinanceirasPageViewModel> logger,
    GetCategoriasFinanceirasUseCase getCategoriasUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private ObservableCollection<CategoriaFinanceiraInfo> _categorias = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private CategoriaFinanceiraTipo _tipoSelecionado = CategoriaFinanceiraTipo.Despesa;

    public string ReceitasBg => TipoSelecionado == CategoriaFinanceiraTipo.Receita ? "#4CAF50" : "#E0E0E0";
    public string DespesasBg => TipoSelecionado == CategoriaFinanceiraTipo.Despesa ? "#F44336" : "#E0E0E0";

    partial void OnTipoSelecionadoChanged(CategoriaFinanceiraTipo value)
    {
        OnPropertyChanged(nameof(ReceitasBg));
        OnPropertyChanged(nameof(DespesasBg));
    }

    [RelayCommand]
    private void ToggleTipo(string tipo)
    {
        TipoSelecionado = tipo == "Receita" ? CategoriaFinanceiraTipo.Receita : CategoriaFinanceiraTipo.Despesa;
        _ = Carregar();
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
            var resp = await getCategoriasUseCase.ExecuteAsync(TipoSelecionado);
            if (resp.RetornouComErro)
            {
                await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
                return;
            }
            Categorias.Clear();
            foreach (var c in resp.Dados!) Categorias.Add(c);
        }
        catch (Exception ex) { logger.LogError(ex, "Erro categorias"); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task NovaCategoria() => await navigationService.NavigateToAsync(nameof(CategoriaFinanceiraEdicaoPage));
}
