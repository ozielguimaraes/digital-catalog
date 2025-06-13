using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Catalogo.Responses;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalogoListaPageViewModel> _logger;
    private readonly ICatalogoService _catalogoService;

    public CatalogoListaPageViewModel(ILogger<CatalogoListaPageViewModel> logger, ICatalogoService catalogoService)
    {
        _logger = logger;
        _catalogoService = catalogoService;
        Catalogos = [];
    }

    [ObservableProperty] private ObservableCollection<CatalogoResponse> _catalogos;

    [RelayCommand]
    private async Task CarregarCatalogosAsync()
    {
        try
        {
            var response = await _catalogoService.GetCatalogosByUserIdAsync();
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", response.ProblemDetails!.Title, "OK");
                return;
            }

            Catalogos.Clear();
            foreach (var item in response.Dados!)
                Catalogos.Add(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar catálogos.");
        }
    }

    [RelayCommand]
    private async Task Adicionar()
    {
        await Shell.Current.GoToAsync($"/{nameof(CatalogoAdicionarPage)}");
    }
}
