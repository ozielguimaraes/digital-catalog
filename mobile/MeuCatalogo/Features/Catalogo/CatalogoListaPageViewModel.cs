using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Catalogo.UseCases;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalogoListaPageViewModel> _logger;
    private readonly GetCatalogosUseCase _getCatalogosUseCase;
    private readonly DeleteCatalogoUseCase _deleteCatalogoUseCase;
    private readonly SetCatalogoFavoritoUseCase _setCatalogoFavoritoUseCase;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;

    public CatalogoListaPageViewModel(
        ILogger<CatalogoListaPageViewModel> logger,
        GetCatalogosUseCase getCatalogosUseCase,
        DeleteCatalogoUseCase deleteCatalogoUseCase,
        SetCatalogoFavoritoUseCase setCatalogoFavoritoUseCase,
        ISettingsService settingsService,
        INavigationService navigationService)
    {
        _logger = logger;
        _getCatalogosUseCase = getCatalogosUseCase;
        _deleteCatalogoUseCase = deleteCatalogoUseCase;
        _setCatalogoFavoritoUseCase = setCatalogoFavoritoUseCase;
        _settingsService = settingsService;
        _navigationService = navigationService;
        Catalogos = [];
    }

    [ObservableProperty] private ObservableCollection<CatalogoInfo> _catalogos;

    [RelayCommand]
    public async Task CarregarCatalogos()
    {
        try
        {
            if (IsBusy) return;

            IsBusy = true;
            var response = await _getCatalogosUseCase.ExecuteAsync();
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, response.ProblemDetails!.Detail, "OK");
                return;
            }
            Catalogos.Clear();
            Catalogos = new ObservableCollection<CatalogoInfo>(response.Dados!);

            if (_settingsService.CatalogoFavorito == null && response.Dados!.Count == 1)
            {
                FavoritarCommand.Execute(response.Dados.FirstOrDefault());
            }
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
