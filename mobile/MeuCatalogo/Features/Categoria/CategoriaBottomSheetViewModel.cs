using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Models;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Categoria;

public partial class CategoriaBottomSheetViewModel : ObservableObject, INavigationAware
{
    private readonly IBottomSheetNavigationService _bottomSheetNavigationService;

    private CategoriaModel? _itemSelecionado;

    public CategoriaBottomSheetViewModel(IBottomSheetNavigationService bottomSheetNavigationService)
    {
        _bottomSheetNavigationService = bottomSheetNavigationService;
    }

    [ObservableProperty]
    private ObservableCollection<CategoriaModel> _categorias;
    [ObservableProperty] private View _currentContent;
    [ObservableProperty] private string _titulo = "Selecione uma categoria";

    [RelayCommand]
    private async Task SelecionarCategoria(CategoriaModel categoria)
    {
        _itemSelecionado = categoria;

        var parametros = new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.CategoriaSelectionada, _itemSelecionado }
        };

        await _bottomSheetNavigationService.GoBackAsync(parametros);
    }

    public void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        var categorias = new List<CategoriaModel>();

        if (parameters.TryGetValue(BottomSheetParameters.Categorias, out object? items) && items is List<CategoriaModel> lista)
        {
            categorias = lista;
        }

        Categorias = new ObservableCollection<CategoriaModel>(categorias);
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters)
    {

    }
}
