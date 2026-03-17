using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class SetCatalogoFavoritoUseCase(ISettingsService settingsService) : IUseCase<CatalogoInfo>
{
    public Task ExecuteAsync(CatalogoInfo request)
    {
        settingsService.CatalogoFavorito = request;
        return Task.CompletedTask;
    }
}
