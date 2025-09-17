using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Categoria;
using MeuCatalogo.Features.Estoque;
using MeuCatalogo.Features.Produto.Models;
using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Settings.Services;
using Microsoft.Extensions.Logging;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Produto;

public sealed partial class ProdutoAdicionarPageViewModel : BasePageViewModel, INavigationAware
{
    private readonly ILogger<ProdutoAdicionarPageViewModel> _logger;
    private readonly IProdutoService _produtoService;
    private readonly ICategoriaService _categoriaService;
    private readonly ISettingsService _settingsService;
    private readonly IBottomSheetNavigationService _bottomSheetNavigationService;

    private CancellationTokenSource? _ctsCategorias;
    private Task<ApiResponse<List<CategoriaModel>>>? _taskCarregaCategorias;

    public ProdutoAdicionarPageViewModel(
        ILogger<ProdutoAdicionarPageViewModel> logger,
        IProdutoService produtoService,
        ISettingsService settingsService,
        IBottomSheetNavigationService bottomSheetNavigationService,
        ICategoriaService categoriaService)
    {
        _logger = logger;
        _produtoService = produtoService;
        _settingsService = settingsService;
        _bottomSheetNavigationService = bottomSheetNavigationService;
        _categoriaService = categoriaService;
    }

    [ObservableProperty] private string _nome;
    [ObservableProperty] private string _nomeErrorMessage;

    [ObservableProperty] private decimal _preco;
    [ObservableProperty] private string _precoString = string.Empty;
    [ObservableProperty] private string _precoErrorMessage;

    private decimal? _precoComDesconto;
    [ObservableProperty] private string _precoComDescontoString = string.Empty;
    [ObservableProperty] private string _precoComDescontoErrorMessage;

    [ObservableProperty] private string? _informacoesAdicionais;
    [ObservableProperty] private string? _informacoesAdicionaisErrorMessage;

    [ObservableProperty] private CategoriaModel? _categoria;
    [ObservableProperty] private string? _categoriaErrorMessage;

    [ObservableProperty] private int? _estoque;
    [ObservableProperty] private string? _estoqueErrorMessage;

    #region Conversão Preços
    partial void OnNomeChanged(string value) => ValidateNome();

    partial void OnPrecoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Preco = 0;
        }
        else if (TentarConverterPreco(value, out decimal preco))
        {
            Preco = preco;
        }
        ValidatePreco();
    }

    partial void OnPrecoComDescontoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _precoComDesconto = null;
        }
        else if (TentarConverterPreco(value, out decimal preco))
        {
            _precoComDesconto = preco;
        }
        ValidatePrecoComDesconto();
    }

    private static bool TentarConverterPreco(string? value, out decimal preco)
    {
        var culture = CultureInfo.CurrentCulture;
        string? sanitized = value?.Trim();

        return decimal.TryParse(
            sanitized,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol,
            culture,
            out preco);
    }
    #endregion

    #region Navegação
    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters)
    {
    }

    public void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        try
        {
            if (parameters.TryGetValue(BottomSheetParameters.CategoriaSelectionada, out object? categoriaSelecionada) &&
                categoriaSelecionada is CategoriaModel categoria)
            {
                Categoria = categoria;
                CategoriaErrorMessage = string.Empty;
            }
            else
            {
                CategoriaErrorMessage = "Categoria é obrigatória";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao entrar na tela de adicionar produto");
        }
    }
    #endregion

    #region Categorias
    [RelayCommand]
    private async Task ExibirCategorias()
    {
        try
        {
            var categoriasResponse = await _taskCarregaCategorias;

            if (categoriasResponse.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(categoriasResponse));
                await Application.Current.MainPage.DisplayAlert("Erro", mensagemErro, "OK");
                return;
            }

            var parametros = new BottomSheetNavigationParameters
            {
                { BottomSheetParameters.Categorias, categoriasResponse.Dados! }
            };

            await _bottomSheetNavigationService.NavigateToAsync<CategoriaBottomSheetViewModel>(
                BottomSheetKeys.ListaCategoria, parametros);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Carregamento de categorias cancelado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exibir as categorias");
            await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível exibir as categorias", "OK");
        }
    }

    [RelayCommand]
    private async Task CarregarCategorias()
    {
        if (_settingsService.CatalogoFavorito is null)
        {
            _logger.LogWarning("Nenhum catálogo favorito encontrado.");

            await Application.Current.MainPage.DisplayAlert("Erro",
                "Nenhum catálogo favorito encontrado. Por favor, selecione um catálogo.", "OK");

            await Shell.Current.GoToAsync(nameof(CatalogoListaPage));
            return;
        }

        _ctsCategorias = new CancellationTokenSource();
        var ct = _ctsCategorias.Token;

        _taskCarregaCategorias = _categoriaService.ObterPorCatalogoIdAsync(_settingsService.CatalogoFavorito.Id, ct);
    }

    private void CancelarCarregamentoCategorias()
    {
        if (_ctsCategorias is { IsCancellationRequested: false })
        {
            _ctsCategorias.Cancel();
            _ctsCategorias.Dispose();
        }
        _ctsCategorias = null;
        _taskCarregaCategorias = null;
    }
    #endregion

    #region Estoque
    [RelayCommand]
    private async Task ExibirEstoque()
    {
        try
        {
            var parametros = new BottomSheetNavigationParameters();

            await _bottomSheetNavigationService.NavigateToAsync<EstoqueBottomSheet>(
                BottomSheetKeys.Estoque, parametros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exibir o estoque");
            await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível exibir o estoque", "OK");
        }
    }
    #endregion

    #region Produto
    [RelayCommand]
    private async Task Salvar()
    {
        try
        {
            if (IsBusy) return;

            IsBusy = true;

            if (!ValidateAll()) return;

            var request = new ProdutoCreateRequest(Nome, Categoria!.Id, catalogoId: _settingsService.CatalogoFavorito!.Id, Preco, _precoComDesconto, null);

            var response = await _produtoService.CreateAsync(request);
            if (response.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(response));
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, mensagemErro, "OK");
                return;
            }

            CancelarCarregamentoCategorias();
            await Shell.Current.GoToAsync($"//{nameof(ProdutoListaPage)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar produto");
        }
    }
    #endregion
}
