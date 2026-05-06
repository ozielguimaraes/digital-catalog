using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Settings.Services;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class SetCatalogoEmUsoUseCase(ISettingsService settingsService) : IUseCase<CatalogoInfo>
{
    public Task ExecuteAsync(CatalogoInfo request)
    {
        settingsService.CatalogoEmUso = request;
        return Task.CompletedTask;
    }
}
