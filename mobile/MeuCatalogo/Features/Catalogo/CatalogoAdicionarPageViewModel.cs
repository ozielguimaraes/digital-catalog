using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Catalogo.UseCases;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoAdicionarPageViewModel(
    ILogger<CatalogoListaPageViewModel> logger,
    CreateCatalogoUseCase createCatalogoUseCase,
    ISettingsService settingsService,
    INavigationService navigationService)
    : BasePageViewModel
{
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

            var request = new CatalogoCreateRequest
            {
                Nome = Nome,
                NomeCurto = NomeCurto,
                NumeroWhatsapp = NumeroWhatsapp,
                Email = Email,
                Descricao = " "
            };

            var response = await createCatalogoUseCase.ExecuteAsync(request);
            if (response.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(response));
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, mensagemErro, "OK");
                return;
            }

            settingsService.CatalogoFavorito ??= response.Dados;

            await navigationService.NavigateToAsync($"//{nameof(CatalogoListaPage)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error salvar catálogo");
        }
    }
}
