using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo;
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

    [ObservableProperty] private ObservableCollection<ProdutoResponse> _produtos = [];
    [ObservableProperty] private bool _isRefreshing;

    [RelayCommand]
    private async Task CarregarDados()
    {
        try
        {
            IsBusy = true;
            var catalogo = _settingsService.CatalogoFavorito;

            if (catalogo == null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Por favor, selecione um catálogo como favorito primeiro", "OK");
                await Shell.Current.GoToAsync($"//{nameof(CatalogoListaPage)}");
                return;
            }

            var response = await _produtoService.ObterPorCatalogoIdAsync(catalogo.Id);
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, response.ProblemDetails!.Detail, "OK");
                return;
            }

            if (response.Dados != null)
            {
                foreach (var produto in response.Dados)
                {
                    foreach (var imagem in produto.Imagens)
                    {
                        if (string.IsNullOrWhiteSpace(imagem.Images.Thumbnail))
                            imagem.Images.Thumbnail = imagem.Url;
                        if (string.IsNullOrWhiteSpace(imagem.Images.Medium))
                            imagem.Images.Medium = imagem.Url;
                        if (string.IsNullOrWhiteSpace(imagem.Images.Full))
                            imagem.Images.Full = imagem.Url;
                    }
                }
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
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task Adicionar()
    {
        await Shell.Current.GoToAsync($"{nameof(ProdutoAdicionarPage)}", true);
    }

    [RelayCommand]
    private async Task Editar(ProdutoResponse produto)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "Produto", produto }
        };
        await Shell.Current.GoToAsync($"{nameof(ProdutoAdicionarPage)}", true, navigationParameter);
    }

    [RelayCommand]
    private async Task Deletar(ProdutoResponse produto)
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja realmente excluir o produto '{produto.Nome}'?", "Sim", "Não");
        if (!confirm) return;

        try
        {
            IsBusy = true;
            var response = await _produtoService.DeleteAsync(produto.Id);

            if (response.RetornouComSucesso)
            {
                Produtos.Remove(produto);
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Produto removido com sucesso.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Erro", response.ProblemDetails?.Detail ?? "Erro ao remover produto.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover produto");
            await Application.Current.MainPage.DisplayAlert("Erro", "Ocorreu um erro inesperado ao remover o produto.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
