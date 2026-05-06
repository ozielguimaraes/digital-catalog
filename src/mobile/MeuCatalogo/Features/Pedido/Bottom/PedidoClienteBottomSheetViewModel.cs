using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Cliente.Domain;
using MeuCatalogo.Features.Cliente.UseCases;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Pedido.Bottom;

public sealed partial class PedidoClienteBottomSheetViewModel(
    GetClientesUseCase getClientesUseCase,
    IBottomSheetNavigationService bottomSheetNavigationService) : BasePageViewModel, INavigationAware
{
    private List<ClienteInfo> _todos = [];

    [ObservableProperty] private ObservableCollection<ClienteInfo> _filtrados = [];
    [ObservableProperty] private string? _filtro;

    public async void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        var resp = await getClientesUseCase.ExecuteAsync();
        if (resp.RetornouComErro || resp.Dados is null) return;

        _todos = resp.Dados.OrderBy(c => c.Nome).ToList();
        AplicarFiltro();
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters) { }

    partial void OnFiltroChanged(string? value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        var termo = (Filtro ?? string.Empty).Trim();
        IEnumerable<ClienteInfo> resultado = _todos;

        if (termo.Length > 0)
        {
            resultado = _todos.Where(c =>
                (c.Nome ?? string.Empty).Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (c.Email ?? string.Empty).Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (c.Telefone ?? string.Empty).Contains(termo, StringComparison.OrdinalIgnoreCase));
        }

        Filtrados = new ObservableCollection<ClienteInfo>(resultado);
    }

    [RelayCommand]
    private async Task Selecionar(ClienteInfo cliente)
    {
        var parametros = new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.ClienteSelecionado, cliente }
        };
        await bottomSheetNavigationService.GoBackAsync(parametros);
    }
}
