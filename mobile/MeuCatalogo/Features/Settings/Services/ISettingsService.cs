using MeuCatalogo.Features.Catalogo.Responses;

namespace MeuCatalogo.Features.Settings.Services;

public interface ISettingsService
{
    CatalogoResponse? CatalogoFavorito { get; set; }
    Task ClearAllAsync();
}
