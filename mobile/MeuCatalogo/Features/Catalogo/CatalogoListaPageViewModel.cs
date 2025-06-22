using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Catalogo.Responses;
using MeuCatalogo.Features.Settings.Services;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalogoListaPageViewModel> _logger;
    private readonly ICatalogoService _catalogoService;
    private readonly ISettingsService _settingsService;

    public CatalogoListaPageViewModel(ILogger<CatalogoListaPageViewModel> logger, ICatalogoService catalogoService, ISettingsService settingsService)
    {
        _logger = logger;
        _catalogoService = catalogoService;
        _settingsService = settingsService;
        Catalogos = [];
    }

    [ObservableProperty] private ObservableCollection<CatalogoResponse> _catalogos;

    [RelayCommand]
    public async Task CarregarCatalogos()
    {
        try
        {
            if (IsBusy) return;

            IsBusy = true;
            var response = await _catalogoService.GetCatalogosByUserIdAsync();
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", response.ProblemDetails!.Title, "OK");
                return;
            }
            Catalogos.Clear();
            Catalogos = new ObservableCollection<CatalogoResponse>(response.Dados!);
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
    private async Task Excluir(CatalogoResponse catalogo)
    {
        try
        {
            var response = await _catalogoService.DeleteCatalogoAsync(catalogo.Id);
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", response.ProblemDetails!.Title, "OK");
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
    private async Task Favoritar(CatalogoResponse catalogo)
    {
        try
        {
            _settingsService.CatalogoFavorito = catalogo;
            await Task.CompletedTask;
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
        await Shell.Current.GoToAsync($"/{nameof(CatalogoAdicionarPage)}");
    }
}
