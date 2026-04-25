using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Produto.Data.Local;
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
    private readonly SyncProdutosByCatalogoUseCase _syncProdutosByCatalogoUseCase;
    private readonly GetProdutoForEditOfflineFirstUseCase _getProdutoForEditOfflineFirstUseCase;
    private readonly DeleteProdutoOfflineFirstUseCase _deleteProdutoOfflineFirstUseCase;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private bool _backgroundSyncStarted;
    private bool _isLoading;

    public ProdutoListaPageViewModel(
        ILogger<ProdutoListaPageViewModel> logger,
        IProdutoLocalRepository produtoLocalRepository,
        SyncProdutosByCatalogoUseCase syncProdutosByCatalogoUseCase,
        GetProdutoForEditOfflineFirstUseCase getProdutoForEditOfflineFirstUseCase,
        DeleteProdutoOfflineFirstUseCase deleteProdutoOfflineFirstUseCase,
        ISettingsService settingsService,
        INavigationService navigationService)
    {
        _logger = logger;
        _produtoLocalRepository = produtoLocalRepository;
        _syncProdutosByCatalogoUseCase = syncProdutosByCatalogoUseCase;
        _getProdutoForEditOfflineFirstUseCase = getProdutoForEditOfflineFirstUseCase;
        _deleteProdutoOfflineFirstUseCase = deleteProdutoOfflineFirstUseCase;
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
    [ObservableProperty] private bool _hasProdutos;
    [ObservableProperty] private bool _showEmptyState;
    [ObservableProperty] private bool _isSyncing;

    [RelayCommand]
    private async Task CarregarDados()
    {
        try
        {
            if (_isLoading)
                return;

            _isLoading = true;
            var catalogo = _settingsService.CatalogoFavorito;

            if (catalogo == null)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Por favor, selecione um catálogo como favorito primeiro", "OK");
                await _navigationService.NavigateToAsync($"//{nameof(CatalogoListaPage)}");
                return;
            }

            var localProdutos = await _produtoLocalRepository.GetByCatalogoIdAsync(catalogo.Id.ToString());
            Produtos.Clear();
            foreach (var p in localProdutos)
                Produtos.Add(p);

            HasProdutos = Produtos.Count > 0;
            ShowEmptyState = !HasProdutos;
            IsSyncing = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar catalogos.");
        }
        finally
        {
            _isLoading = false;
            IsRefreshing = false;
        }

        _ = MaybeSyncInBackgroundAsync();
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
            if (!HasProdutos)
            {
                IsSyncing = true;
                ShowEmptyState = false;
            }

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

                        HasProdutos = Produtos.Count > 0;
                        IsSyncing = false;
                        ShowEmptyState = !HasProdutos;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar produtos em background.");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        IsRefreshing = false;
                        HasProdutos = Produtos.Count > 0;
                        IsSyncing = false;
                        ShowEmptyState = !HasProdutos;
                    });
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
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await Task.Yield();

            var sw = Stopwatch.StartNew();
            await _navigationService.NavigateToAsync($"{nameof(ProdutoAdicionarPage)}");
            _logger.LogInformation("Navegação Adicionar -> ProdutoAdicionarPage em {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Editar(ProdutoEntity produto)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await Task.Yield();

            var sw = Stopwatch.StartNew();
            var produtoResponse = await _getProdutoForEditOfflineFirstUseCase.ExecuteAsync(produto);

            var navigationParameter = new Dictionary<string, object>
            {
                {
                    "Produto",
                    produtoResponse
                }
            };

            await Shell.Current.GoToAsync($"{nameof(ProdutoAdicionarPage)}", true, navigationParameter);
            _logger.LogInformation("Navegação Editar -> ProdutoAdicionarPage em {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
        finally
        {
            IsBusy = false;
        }
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
            var hadInternet = HasInternetConnection();
            var response = await _deleteProdutoOfflineFirstUseCase.ExecuteAsync(Guid.Parse(produto.Id));

            if (response.RetornouComSucesso)
            {
                Produtos.Remove(produto);
                if (hadInternet)
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Produto removido com sucesso.", "OK");
                else
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Produto removido localmente e será sincronizado quando houver internet.", "OK");
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
