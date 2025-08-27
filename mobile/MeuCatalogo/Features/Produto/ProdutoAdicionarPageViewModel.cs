using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Categoria;
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

    #region Conversão Preços
    partial void OnPrecoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _preco = 0;
            PrecoErrorMessage = string.Empty;
            return;
        }

        if (TentarConverterPreco(value, out decimal preco))
        {
            _preco = preco;
            PrecoErrorMessage = string.Empty;
        }
        else
        {
            PrecoErrorMessage = "Valor inválido para o campo Preço";
        }
    }

    partial void OnPrecoComDescontoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _precoComDesconto = null;
            PrecoComDescontoErrorMessage = string.Empty;
            return;
        }

        if (TentarConverterPreco(value, out decimal preco))
        {
            _precoComDesconto = preco;
            PrecoComDescontoErrorMessage = string.Empty;
        }
        else
        {
            PrecoComDescontoErrorMessage = "Valor inválido para o campo Preço com desconto";
        }
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
        CancelarCarregamentoCategorias();
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

            if (_settingsService.CatalogoFavorito is null)
            {
                _logger.LogWarning("Nenhum catálogo favorito encontrado.");
                Application.Current.MainPage.DisplayAlert("Erro",
                    "Nenhum catálogo favorito encontrado. Por favor, selecione um catálogo.", "OK");
                return;
            }

            _ctsCategorias = new CancellationTokenSource();
            var ct = _ctsCategorias.Token;

            _taskCarregaCategorias = _categoriaService.ObterPorCatalogoIdAsync(_settingsService.CatalogoFavorito.Id, ct);

            // dispara o carregamento em background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _taskCarregaCategorias;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao carregar categorias");
                }
            });
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
            if (_taskCarregaCategorias == null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Carregamento das categorias não foi iniciado.", "OK");
                return;
            }

            // garante que o carregamento terminou
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

    #region Produto
    [RelayCommand]
    private async Task Salvar()
    {
        try
        {
            // validações básicas
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo Nome é obrigatório", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PrecoString))
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo Preço é obrigatório", "OK");
                return;
            }

            if (Preco <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Preço deve ser maior que '0'", "OK");
                return;
            }

            if (_precoComDesconto != null)
            {
                if (_precoComDesconto <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Campo Preço com desconto deve ser maior que '0'", "OK");
                    return;
                }

                if (_precoComDesconto > Preco)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Preço com desconto deve ser menor que o preço original", "OK");
                    return;
                }
            }

            if (Categoria == null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Selecione uma categoria", "OK");
                CategoriaErrorMessage = "Categoria é obrigatória";
                return;
            }

            var request = new ProdutoCreateRequest(Nome, Guid.Empty, Guid.Empty, Preco, _precoComDesconto, null);

            var response = await _produtoService.CreateAsync(request);
            if (response.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(response));
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, mensagemErro, "OK");
                return;
            }

            await Shell.Current.GoToAsync($"//{nameof(ProdutoListaPage)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar produto");
        }
    }
    #endregion
}
