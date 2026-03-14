using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Produto.Presentation;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace MeuCatalogo.Features.Produto;

public partial class ProdutoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<ProdutoListaPageViewModel> _logger;
    private readonly IProdutoLocalRepository _produtoLocalRepository;
    private readonly IProdutoImagemLocalRepository _produtoImagemLocalRepository;
    private readonly SyncProdutosByCatalogoUseCase _syncProdutosByCatalogoUseCase;
    private readonly GetProdutoByIdUseCase _getProdutoByIdUseCase;
    private readonly DeleteProdutoRemoteUseCase _deleteProdutoRemoteUseCase;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private bool _backgroundSyncStarted;

    public ProdutoListaPageViewModel(
        ILogger<ProdutoListaPageViewModel> logger,
        IProdutoLocalRepository produtoLocalRepository,
        IProdutoImagemLocalRepository produtoImagemLocalRepository,
        SyncProdutosByCatalogoUseCase syncProdutosByCatalogoUseCase,
        GetProdutoByIdUseCase getProdutoByIdUseCase,
        DeleteProdutoRemoteUseCase deleteProdutoRemoteUseCase,
        ISettingsService settingsService,
        INavigationService navigationService)
    {
        _logger = logger;
        _produtoLocalRepository = produtoLocalRepository;
        _produtoImagemLocalRepository = produtoImagemLocalRepository;
        _syncProdutosByCatalogoUseCase = syncProdutosByCatalogoUseCase;
        _getProdutoByIdUseCase = getProdutoByIdUseCase;
        _deleteProdutoRemoteUseCase = deleteProdutoRemoteUseCase;
        _settingsService = settingsService;
        _navigationService = navigationService;

        WeakReferenceMessenger.Default.Register<ProdutoUpsertedMessage>(this, async (_, message) =>
        {
            try
            {
                await AtualizarProdutoNaListaAsync(message.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto na lista");
            }
        });
    }

    [ObservableProperty] private ObservableCollection<ProdutoEntity> _produtos = [];
    [ObservableProperty] private bool _isRefreshing;

    [RelayCommand]
    private async Task CarregarDados()
    {
        try
        {
            if (IsBusy)
                return;

            IsBusy = true;
            var catalogo = _settingsService.CatalogoFavorito;

            if (catalogo == null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Por favor, selecione um catálogo como favorito primeiro", "OK");
                await _navigationService.NavigateToAsync($"//{nameof(CatalogoListaPage)}");
                return;
            }

            var localProdutos = await _produtoLocalRepository.GetByCatalogoIdAsync(catalogo.Id.ToString());
            Produtos.Clear();
            Produtos = new ObservableCollection<ProdutoEntity>(localProdutos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar catalogos.");
        }
        finally
        {
            IsBusy = false;
        }

        await MaybeSyncInBackgroundAsync();
    }

    private async Task MaybeSyncInBackgroundAsync()
    {
        try
        {
            var catalogo = _settingsService.CatalogoFavorito;
            if (catalogo == null)
                return;

            var forceSync = IsRefreshing;
            var localProdutos = await _produtoLocalRepository.GetByCatalogoIdAsync(catalogo.Id.ToString());

            var shouldSync =
                forceSync ||
                (!_backgroundSyncStarted && HasInternetConnection() && (!localProdutos.Any() || localProdutos.Any(p => string.IsNullOrWhiteSpace(p.ThumbnailUrl))));

            if (!shouldSync)
            {
                IsRefreshing = false;
                return;
            }

            _backgroundSyncStarted = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _syncProdutosByCatalogoUseCase.ExecuteAsync(catalogo.Id);
                    var updated = await _produtoLocalRepository.GetByCatalogoIdAsync(catalogo.Id.ToString());

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Produtos.Clear();
                        foreach (var p in updated)
                            Produtos.Add(p);
                        IsRefreshing = false;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar produtos em background.");
                    MainThread.BeginInvokeOnMainThread(() => IsRefreshing = false);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao preparar sincronização de produtos.");
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task Adicionar()
    {
        await _navigationService.NavigateToAsync($"{nameof(ProdutoAdicionarPage)}");
    }

    [RelayCommand]
    private async Task Editar(ProdutoEntity produto)
    {
        var catalogoId = Guid.TryParse(produto.CatalogoId, out var parsedCatalogoId) ? parsedCatalogoId : Guid.Empty;
        var categoriaId = Guid.TryParse(produto.CategoriaId, out var parsedCategoriaId) ? parsedCategoriaId : Guid.Empty;
        var produtoId = Guid.TryParse(produto.Id, out var parsedProdutoId) ? parsedProdutoId : Guid.Empty;

        var imagens = await _produtoImagemLocalRepository.GetByProdutoIdAsync(produto.Id);
        if (!imagens.Any() && HasInternetConnection() && Guid.TryParse(produto.Id, out var remoteId) && remoteId != Guid.Empty)
        {
            var remote = await _getProdutoByIdUseCase.ExecuteAsync(remoteId);
            if (remote is { RetornouComSucesso: true, Dados: not null })
            {
                var now = DateTime.UtcNow;
                var imagensRemote = remote.Dados.Imagens.Select(i => new ProdutoImagemEntity
                {
                    Id = i.Id.ToString(),
                    ProdutoId = produto.Id,
                    CatalogoId = remote.Dados.CatalogoId.ToString(),
                    Url = i.Url,
                    Thumbnail = i.Images.Thumbnail,
                    Medium = i.Images.Medium,
                    Full = i.Images.Full,
                    IsPrincipal = i.IsPrincipal,
                    Ordem = i.Ordem,
                    SyncStatus = i.SyncStatus,
                    LastModified = now
                }).ToList();

                await _produtoImagemLocalRepository.ReplaceByProdutoIdAsync(produto.Id, imagensRemote);
                imagens = imagensRemote;
            }
        }

        var imagensResponse = imagens.Select(i => new ProdutoImagemResponse
        {
            Id = Guid.TryParse(i.Id, out var imageId) ? imageId : Guid.Empty,
            Url = i.Url,
            Images = new ProdutoImagemLinksResponse
            {
                Thumbnail = i.Thumbnail,
                Medium = i.Medium,
                Full = i.Full
            },
            IsPrincipal = i.IsPrincipal,
            Ordem = i.Ordem,
            SyncStatus = i.SyncStatus
        }).ToList();

        var navigationParameter = new Dictionary<string, object>
        {
            {
                "Produto",
                new ProdutoResponse
                {
                    Id = produtoId,
                    Nome = produto.Nome,
                    Preco = produto.Preco,
                    PrecoComDesconto = produto.PrecoComDesconto,
                    InformacoesAdicionais = produto.InformacoesAdicionais,
                    CategoriaId = categoriaId,
                    CategoriaNome = produto.CategoriaNome ?? string.Empty,
                    CatalogoId = catalogoId,
                    Estoque = null,
                    Imagens = imagensResponse,
                    SyncStatus = SyncStatus.Completed
                }
            }
        };

        await Shell.Current.GoToAsync($"{nameof(ProdutoAdicionarPage)}", true, navigationParameter);
    }

    private async Task AtualizarProdutoNaListaAsync(string produtoId)
    {
        var entity = await _produtoLocalRepository.GetByIdAsync(produtoId);
        if (entity == null)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existing = Produtos.FirstOrDefault(p => p.Id == produtoId);
            if (existing == null)
            {
                Produtos.Insert(0, entity);
                return;
            }

            var index = Produtos.IndexOf(existing);
            if (index >= 0)
                Produtos[index] = entity;
        });
    }

    [RelayCommand]
    private async Task Deletar(ProdutoEntity produto)
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja realmente excluir o produto '{produto.Nome}'?", "Sim", "Não");
        if (!confirm) return;

        try
        {
            IsBusy = true;
            var response = await _deleteProdutoRemoteUseCase.ExecuteAsync(Guid.Parse(produto.Id));

            if (response.RetornouComSucesso)
            {
                Produtos.Remove(produto);
                await _produtoLocalRepository.DeleteAsync(produto);
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
