using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Financeiro.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Financeiro.Domain;
using MeuCatalogo.Features.Financeiro.Icons;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Financeiro;

public sealed partial class CategoriaFinanceiraEdicaoPageViewModel(
    ILogger<CategoriaFinanceiraEdicaoPageViewModel> logger,
    CriarCategoriaFinanceiraUseCase criarUseCase,
    INavigationService navigationService) : BasePageViewModel
{
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string _iconeNome = "tag";
    [ObservableProperty] private string _cor = "#9E9E9E";
    [ObservableProperty] private string _tipoSelecionado = "Despesa";

    public IReadOnlyList<string> Tipos { get; } = new[] { "Receita", "Despesa" };
    public IReadOnlyList<string> Icones => IconCatalog.AvailableNames.ToList();

    [RelayCommand]
    private async Task Salvar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            await Toast.Make("Nome obrigatório", ToastDuration.Short).Show();
            return;
        }
        var tipo = TipoSelecionado == "Receita" ? CategoriaFinanceiraTipo.Receita : CategoriaFinanceiraTipo.Despesa;
        var resp = await criarUseCase.ExecuteAsync(new CategoriaFinanceiraRequest
        {
            Nome = Nome.Trim(),
            Tipo = tipo,
            IconeNome = IconeNome,
            Cor = Cor
        });
        if (resp.RetornouComErro)
        {
            await Toast.Make(resp.MensagemDeErro ?? "Erro", ToastDuration.Long).Show();
            return;
        }
        await Toast.Make("Categoria criada", ToastDuration.Short).Show();
        await navigationService.PopAsync();
    }
}
