using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class SetCatalogoFavoritoUseCase : IUseCase<CatalogoInfo>
{
    private readonly ISettingsService _settingsService;

    public SetCatalogoFavoritoUseCase(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public Task ExecuteAsync(CatalogoInfo request)
    {
        _settingsService.CatalogoFavorito = request;
        return Task.CompletedTask;
    }
}
