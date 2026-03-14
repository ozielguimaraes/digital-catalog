using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;
using MeuCatalogo.Features.Categoria.Models;
using MeuCatalogo.Features.Categoria.UseCases;
using MeuCatalogo.Features.Settings.Services;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Categoria;

public partial class CategoriaBottomSheetViewModel(
    IBottomSheetNavigationService bottomSheetNavigationService,
    CreateCategoriaUseCase createCategoriaUseCase,
    UpdateCategoriaUseCase updateCategoriaUseCase,
    ISettingsService settingsService)
    : BasePageViewModel, INavigationAware
{
    private CategoriaModel? _itemSelecionado;

    [ObservableProperty] private ObservableCollection<CategoriaModel> _categorias;
    [ObservableProperty] private View _currentContent;
    [ObservableProperty] private string _titulo = "Selecione uma categoria";
    [ObservableProperty] private string _novaCategoria;
    [ObservableProperty] private string _novaCategoriaErrorMessage;
    [ObservableProperty] private bool _isEditing;
    private CategoriaModel? _categoriaEmEdicao;

    partial void OnNovaCategoriaChanged(string value)
    {
        NovaCategoriaEstaValida();
    }

    [RelayCommand]
    private void EditarCategoria(CategoriaModel categoria)
    {
        _categoriaEmEdicao = categoria;
        NovaCategoria = categoria.Nome;
        IsEditing = true;
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

        if (IsEditing && _categoriaEmEdicao != null)
        {
            var request = new CategoriaUpsertRequest
            {
                Nome = NovaCategoria!.Trim(),
                Descricao = _categoriaEmEdicao.Descricao ?? string.Empty,
                CatalogoId = _categoriaEmEdicao.CatalogoId
            };

            var response = await updateCategoriaUseCase.ExecuteAsync((_categoriaEmEdicao.Id, request));
            if (response.RetornouComErro)
            {
                NovaCategoriaErrorMessage = string.Join("\n", ObterErros(response));
                return;
            }

            var updated = ToModel(response.Dados!);
            
            var index = Categorias.IndexOf(_categoriaEmEdicao);
            if (index >= 0)
            {
                Categorias[index] = updated;
                if (_itemSelecionado?.Id == _categoriaEmEdicao.Id)
                {
                    _itemSelecionado = updated;
                    _itemSelecionado.IsSelected = true;
                }
            }
        }
        else
        {
            var request = new CategoriaUpsertRequest
            {
                Nome = NovaCategoria!.Trim(),
                Descricao = string.Empty,
                CatalogoId = settingsService.CatalogoFavorito?.Id ?? Guid.Empty
            };

            var response = await createCategoriaUseCase.ExecuteAsync(request);
            if (response.RetornouComErro)
            {
                NovaCategoriaErrorMessage = string.Join("\n", ObterErros(response));
                return;
            }

            var created = ToModel(response.Dados!);

            Categorias.Add(created);
        }

        NovaCategoria = string.Empty;
        NovaCategoriaErrorMessage = string.Empty;
        IsEditing = false;
        _categoriaEmEdicao = null;
    }

    private static CategoriaModel ToModel(CategoriaInfo categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        CatalogoId = categoria.CatalogoId
    };

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

        if (parameters.TryGetValue(BottomSheetParameters.CategoriaSelectionada, out object? selecionadoObj) && selecionadoObj is CategoriaModel selecionado)
        {
            var naLista = categorias.FirstOrDefault(c => c.Id == selecionado.Id);
            if (naLista != null)
            {
                naLista.IsSelected = true;
                _itemSelecionado = naLista;
            }
        }

        Categorias = new ObservableCollection<CategoriaModel>(categorias);
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters)
    {

    }
}
