using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Models;
using MeuCatalogo.Features.Settings.Services;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Categoria;

public partial class CategoriaBottomSheetViewModel : BasePageViewModel, INavigationAware
{
    private readonly IBottomSheetNavigationService _bottomSheetNavigationService;
    private readonly ICategoriaService _categoriaService;
    private readonly ISettingsService _settingsService;

    private CategoriaModel? _itemSelecionado;

    public CategoriaBottomSheetViewModel(IBottomSheetNavigationService bottomSheetNavigationService, ICategoriaService categoriaService, ISettingsService settingsService)
    {
        _bottomSheetNavigationService = bottomSheetNavigationService;
        _categoriaService = categoriaService;
        _settingsService = settingsService;
    }

    [ObservableProperty] private ObservableCollection<CategoriaModel> _categorias;
    [ObservableProperty] private View _currentContent;
    [ObservableProperty] private string _titulo = "Selecione uma categoria";
    [ObservableProperty] private string _novaCategoria;
    [ObservableProperty] private string _novaCategoriaErrorMessage;

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

    [RelayCommand]
    private async Task Salvar()
    {
        if (string.IsNullOrWhiteSpace(NovaCategoria))
        {
            NovaCategoriaErrorMessage = "O nome da categoria é obrigatório.";
            return;
        }

        if (NovaCategoria.Length > 50)
        {
            NovaCategoriaErrorMessage = "Máximo 50 caracteres permitido.";
            return;
        }

        if (Categorias.Any(c => c.Nome.Equals(NovaCategoria, StringComparison.OrdinalIgnoreCase)))
        {
            NovaCategoriaErrorMessage = "Já existe uma categoria com esse nome.";
            return;
        }

        var requestModel = new CategoriaModel(
            nome: NovaCategoria.Trim(),
            descricao: string.Empty,
            catalogoId: _settingsService.CatalogoFavorito!.Id
        );
        var responseModel = await _categoriaService.AdicionarAsync(requestModel);
        if (responseModel.RetornouComErro)
        {
            NovaCategoriaErrorMessage = string.Join("\n", ObterErros(responseModel));
            return;
        }

        Categorias.Add(responseModel.Dados!);

        NovaCategoria = string.Empty;
        NovaCategoriaErrorMessage = string.Empty;
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
