using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Catalogo.Requests;
using MeuCatalogo.Features.Settings.Services;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoAdicionarPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalogoListaPageViewModel> _logger;
    private readonly ICatalogoService _catalogoService;
    private readonly ISettingsService _settingsService;

    public CatalogoAdicionarPageViewModel(ILogger<CatalogoListaPageViewModel> logger, ICatalogoService catalogoService, ISettingsService settingsService)
    {
        _logger = logger;
        _catalogoService = catalogoService;
        _settingsService = settingsService;
    }

    [ObservableProperty] private string _nome;
    [ObservableProperty] private string _nomeErrorMessage;

    [ObservableProperty] private string _nomeCurto;
    [ObservableProperty] private string _nomeCurtoErrorMessage;

    [ObservableProperty] private string _email;
    [ObservableProperty] private string _emailErrorMessage;

    [ObservableProperty] private string _numeroWhatsapp;
    [ObservableProperty] private string _numeroWhatsappErrorMessage;

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
            if (string.IsNullOrWhiteSpace(NomeCurto))
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo Nome curto é obrigatório", "OK");

                return;
            }
            if (string.IsNullOrWhiteSpace(NumeroWhatsapp))
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo número de Watsapp é obrigatório", "OK");

                return;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Campo Email é obrigatório", "OK");

                return;
            }

            var request = new CatalogoCreateRequest(Nome, NomeCurto, NumeroWhatsapp, Email, " ");

            var response = await _catalogoService.CreateCatalogoAsync(request);
            if (response.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(response));
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, mensagemErro, "OK");
                return;
            }

            _settingsService.CatalogoFavorito ??= response.Dados;

            await Shell.Current.GoToAsync($"//{nameof(CatalogoListaPage)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error salvar catálogo");
        }
    }
}
