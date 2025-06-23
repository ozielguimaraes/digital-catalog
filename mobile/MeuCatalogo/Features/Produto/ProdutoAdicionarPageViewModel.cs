using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Requests;
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

    [ObservableProperty] private decimal? _preco;
    [ObservableProperty] private string _precoErrorMessage;

    [ObservableProperty] private decimal? _precoComDesconto;
    [ObservableProperty] private string _precoComDescontoErrorMessage;

    [ObservableProperty] private string? _informacoesAdicionais;
    [ObservableProperty] private string? _informacoesAdicionaisErrorMessage;
    //
    // [ObservableProperty] private CategoriaResponse? _categoria;
    // [ObservableProperty] private CategoriaResponse? _categoriaErrorMessage;

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

            if (!Preco.HasValue)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo Preço é obrigatório", "OK");
                return;
            }

            if (PrecoComDesconto != null && PrecoComDesconto < 0)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo Preço com desconto deve ser >= 0", "OK");
                return;
            }

            var request = new ProdutoCreateRequest(Nome);

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
