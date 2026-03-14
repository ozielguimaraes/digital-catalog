using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Settings.Services;

public interface ISettingsService
{
    CatalogoInfo? CatalogoFavorito { get; set; }
    Task ClearAllAsync();
}
