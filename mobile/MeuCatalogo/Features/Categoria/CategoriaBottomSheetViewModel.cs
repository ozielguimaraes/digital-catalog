using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Models;
using MeuCatalogo.Features.Settings.Services;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Categoria;

public partial class CategoriaBottomSheetViewModel(
    IBottomSheetNavigationService bottomSheetNavigationService,
    ICategoriaService categoriaService,
    ISettingsService settingsService)
    : BasePageViewModel, INavigationAware
{
    private CategoriaModel? _itemSelecionado;

    [ObservableProperty] private ObservableCollection<CategoriaModel> _categorias;
    [ObservableProperty] private View _currentContent;
    [ObservableProperty] private string _titulo = "Selecione uma categoria";
    [ObservableProperty] private string _novaCategoria;
    [ObservableProperty] private string _novaCategoriaErrorMessage;

    partial void OnNovaCategoriaChanged(string value)
    {
        NovaCategoriaEstaValida();
    }

    [RelayCommand]
    private async Task SelecionarCategoria(CategoriaModel categoria)
    {
        _itemSelecionado = categoria;

        var parametros = new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.CategoriaSelectionada, _itemSelecionado }
        };

        await bottomSheetNavigationService.GoBackAsync(parametros);
    }

    [RelayCommand]
    private async Task Salvar()
    {
       if (!NovaCategoriaEstaValida())
           return;

        var requestModel = new CategoriaModel(
            nome: NovaCategoria.Trim(),
            descricao: string.Empty,
            catalogoId: settingsService.CatalogoFavorito!.Id
        );
        var responseModel = await categoriaService.AdicionarAsync(requestModel);
        if (responseModel.RetornouComErro)
        {
            NovaCategoriaErrorMessage = string.Join("\n", ObterErros(responseModel));
            return;
        }

        Categorias.Add(responseModel.Dados!);

        NovaCategoria = string.Empty;
        NovaCategoriaErrorMessage = string.Empty;
    }

    private bool NovaCategoriaEstaValida()
    {
        if (string.IsNullOrWhiteSpace(NovaCategoria))
        {
            NovaCategoriaErrorMessage = "O nome da categoria é obrigatório.";
            return false;
        }

        if (NovaCategoria.Length > 50)
        {
            NovaCategoriaErrorMessage = "Máximo 50 caracteres permitido.";
            return false;
        }

        if (Categorias.Any(c => c.Nome.Equals(NovaCategoria, StringComparison.OrdinalIgnoreCase)))
        {
            NovaCategoriaErrorMessage = "Já existe uma categoria com esse nome.";
            return false;
        }

        NovaCategoriaErrorMessage = string.Empty;
        return true;
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
