using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Settings.Services;

public interface ISettingsService
{
    /// <summary>Catálogo marcado como favorito do usuário (vem do backend no login).</summary>
    CatalogoInfo? CatalogoFavorito { get; set; }

    /// <summary>Catálogo em uso na sessão atual. Padrão = CatalogoFavorito ao logar; muda quando o usuário seleciona outro.</summary>
    CatalogoInfo? CatalogoEmUso { get; set; }

    Task ClearAllAsync();
}
