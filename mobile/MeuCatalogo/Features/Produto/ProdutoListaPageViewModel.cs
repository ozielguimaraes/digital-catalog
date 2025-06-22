using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Catalogo.Responses;
using MeuCatalogo.Features.Produto.Responses;
using MeuCatalogo.Features.Settings.Services;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto;

public partial class ProdutoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<ProdutoListaPageViewModel> _logger;
    private readonly IProdutoService _produtoService;
    private readonly ISettingsService _settingsService;

    public ProdutoListaPageViewModel(ILogger<ProdutoListaPageViewModel> logger, IProdutoService produtoService, ISettingsService settingsService)
    {
        _logger = logger;
        _produtoService = produtoService;
        _settingsService = settingsService;
    }
    [ObservableProperty] private ObservableCollection<ProdutoResponse> _produtos;

    [RelayCommand]
    public async Task CarregarDados()
    {
        try
        {
            if (IsBusy) return;

            IsBusy = true;
            var catalogo = _settingsService.CatalogoFavorito;

            if (catalogo == null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Por favor, selecione um catálogo primeiro", "OK");
                //TODO Navegar para catálogos ou solicitar em popup a seleção
                return;
            }

            var response = await _produtoService.ObterPorCatalogoIdAsync(catalogo.Id);
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", response.ProblemDetails!.Title, "OK");
                return;
            }
            Produtos.Clear();
            Produtos = new ObservableCollection<ProdutoResponse>(response.Dados!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar catalogos.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Adicionar()
    {
        // await Shell.Current.GoToAsync($"/{nameof(ProdutoAdicionarPage)}");
    }
}
