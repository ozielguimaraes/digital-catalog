using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Catalogo.Requests;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoAdicionarPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalogoListaPageViewModel> _logger;
    private readonly ICatalogoService _catalogoService;

    public CatalogoAdicionarPageViewModel(ILogger<CatalogoListaPageViewModel> logger, ICatalogoService catalogoService)
    {
        _logger = logger;
        _catalogoService = catalogoService;
    }

    [ObservableProperty] private string _nome;

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

            var request = new CatalogoCreateRequest(Nome);

            var response = await _catalogoService.CreateCatalogoAsync(request);
            if (response.RetornouComErro)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", response.ProblemDetails!.Title, "OK");
                return;
            }

            await Shell.Current.GoToAsync($"/{nameof(CatalogoListaPage)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error salvar catálogo");
        }
    }
}
