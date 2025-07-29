using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Settings.Services;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto;

public sealed partial class ProdutoAdicionarPageViewModel : BasePageViewModel
{
    private readonly ILogger<ProdutoAdicionarPageViewModel> _logger;
    private readonly IProdutoService _produtoService;
    private readonly ISettingsService _settingsService;

    public ProdutoAdicionarPageViewModel(ILogger<ProdutoAdicionarPageViewModel> logger, IProdutoService produtoService, ISettingsService settingsService)
    {
        _logger = logger;
        _produtoService = produtoService;
        _settingsService = settingsService;
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
    //
    // [ObservableProperty] private CategoriaResponse? _categoria;
    // [ObservableProperty] private CategoriaResponse? _categoriaErrorMessage;

    partial void OnPrecoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _preco = 0;
            PrecoErrorMessage = string.Empty;
            return;
        }

        if (TentarConverterPreco(value, out decimal precoComDesconto))
        {
            _precoComDesconto = precoComDesconto;
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
            PrecoErrorMessage = string.Empty;
            return;
        }

        if (TentarConverterPreco(value, out decimal precoComDesconto))
        {
            _precoComDesconto = precoComDesconto;
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

        bool valido = decimal.TryParse(
            sanitized,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol,
            culture,
            out preco);

        return valido;
    }

    [RelayCommand]
    private async Task Salvar()
    {
        try
        {
            //validations

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
                    await Application.Current.MainPage.DisplayAlert("Erro", "Campo Preço com desconto deve ser maior que '0'",
                        "OK");
                    return;
                }

                if (_precoComDesconto > Preco)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro",
                        "Preço com desconto deve ser menor que o preço original", "OK");
                    return;
                }
            }

            var request = new ProdutoCreateRequest(Nome, Guid.Empty, Guid.Empty, Preco, _precoComDesconto,null);

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
            _logger.LogError(ex, "Error salvar produto");
        }
    }
}
