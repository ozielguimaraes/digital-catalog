using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Features.Settings.Services;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Pedido.Bottom;

public sealed partial class PedidoProdutoBottomSheetViewModel(
    GetProdutosByCatalogoIdUseCase getProdutosByCatalogoIdUseCase,
    ISettingsService settingsService,
    IBottomSheetNavigationService bottomSheetNavigationService) : BasePageViewModel, INavigationAware
{
    private List<ProdutoResponse> _todos = [];

    [ObservableProperty] private ObservableCollection<ProdutoResponse> _filtrados = [];
    [ObservableProperty] private string? _filtro;

    public async void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        var catalogoId = settingsService.CatalogoEmUso?.Id;
        if (catalogoId is null) return;

        var resp = await getProdutosByCatalogoIdUseCase.ExecuteAsync(catalogoId.Value);
        if (resp.RetornouComErro || resp.Dados is null) return;

        _todos = resp.Dados.OrderBy(p => p.Nome).ToList();
        AplicarFiltro();
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters) { }

    partial void OnFiltroChanged(string? value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        var termo = (Filtro ?? string.Empty).Trim();
        IEnumerable<ProdutoResponse> resultado = _todos;

        if (termo.Length > 0)
        {
            resultado = _todos.Where(p =>
                p.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                p.CategoriaNome.Contains(termo, StringComparison.OrdinalIgnoreCase));
        }

        Filtrados = new ObservableCollection<ProdutoResponse>(resultado);
    }

    [RelayCommand]
    private async Task Selecionar(ProdutoResponse produto)
    {
        var parametros = new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.ProdutoSelecionado, produto }
        };
        await bottomSheetNavigationService.GoBackAsync(parametros);
    }
}
