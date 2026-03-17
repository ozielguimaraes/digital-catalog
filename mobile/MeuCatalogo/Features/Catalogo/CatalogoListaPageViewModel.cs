using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Catalogo.UseCases;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalogoListaPageViewModel> _logger;
    private readonly GetCatalogosLocalUseCase _getCatalogosLocalUseCase;
    private readonly SyncCatalogosUseCase _syncCatalogosUseCase;
    private readonly DeleteCatalogoUseCase _deleteCatalogoUseCase;
    private readonly SetCatalogoFavoritoUseCase _setCatalogoFavoritoUseCase;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private bool _backgroundSyncStarted;
    private bool _isLoading;

    public CatalogoListaPageViewModel(
        ILogger<CatalogoListaPageViewModel> logger,
        GetCatalogosLocalUseCase getCatalogosLocalUseCase,
        SyncCatalogosUseCase syncCatalogosUseCase,
        DeleteCatalogoUseCase deleteCatalogoUseCase,
        SetCatalogoFavoritoUseCase setCatalogoFavoritoUseCase,
        ISettingsService settingsService,
        INavigationService navigationService)
    {
        _logger = logger;
        _getCatalogosLocalUseCase = getCatalogosLocalUseCase;
        _syncCatalogosUseCase = syncCatalogosUseCase;
        _deleteCatalogoUseCase = deleteCatalogoUseCase;
        _setCatalogoFavoritoUseCase = setCatalogoFavoritoUseCase;
        _settingsService = settingsService;
        _navigationService = navigationService;
        Catalogos = [];
    }

    [ObservableProperty] private ObservableCollection<CatalogoInfo> _catalogos;
    [ObservableProperty] private bool _hasCatalogos;
    [ObservableProperty] private bool _showEmptyState;
    [ObservableProperty] private bool _isSyncing;

    [RelayCommand]
    private async Task CarregarCatalogos()
    {
        try
        {
            if (_isLoading) return;

            _isLoading = true;
            var local = await _getCatalogosLocalUseCase.ExecuteAsync();
            Catalogos.Clear();
            foreach (var c in local)
                Catalogos.Add(c);

            HasCatalogos = Catalogos.Count > 0;
            ShowEmptyState = !HasCatalogos;
            IsSyncing = false;

            if (_settingsService.CatalogoFavorito == null && local.Count == 1)
            {
                FavoritarCommand.Execute(local.FirstOrDefault());
            }

            if (!_backgroundSyncStarted && HasInternetConnection())
            {
                _backgroundSyncStarted = true;
                if (!HasCatalogos)
                {
                    IsSyncing = true;
                    ShowEmptyState = false;
                }
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _syncCatalogosUseCase.ExecuteAsync();
                        var updated = await _getCatalogosLocalUseCase.ExecuteAsync();
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Catalogos.Clear();
                            foreach (var c in updated)
                                Catalogos.Add(c);

                            if (_settingsService.CatalogoFavorito == null && updated.Count > 0)
                                FavoritarCommand.Execute(updated[0]);

                            HasCatalogos = Catalogos.Count > 0;
                            IsSyncing = false;
                            ShowEmptyState = !HasCatalogos;
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao sincronizar catálogos em background.");
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            HasCatalogos = Catalogos.Count > 0;
                            IsSyncing = false;
                            ShowEmptyState = !HasCatalogos;
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar catalogos.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task Excluir(CatalogoInfo catalogo)
    {
        try
        {
            var response = await _deleteCatalogoUseCase.ExecuteAsync(catalogo.Id);
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, response.ProblemDetails!.Detail, "OK");
                return;
            }

            Catalogos.Remove(catalogo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir catalogo.");
            await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task Favoritar(CatalogoInfo catalogo)
    {
        try
        {
            await _setCatalogoFavoritoUseCase.ExecuteAsync(catalogo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao favoritar o catalogo.");
            await Application.Current.MainPage.DisplayAlert("Erro", "Erro ao favoritar o catalogo", "OK");
        }
    }

    [RelayCommand]
    private async Task Adicionar()
    {
        await _navigationService.NavigateToAsync($"/{nameof(CatalogoAdicionarPage)}");
    }
}
