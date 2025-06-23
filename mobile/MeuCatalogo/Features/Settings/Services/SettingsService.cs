using System.Text.Json;
using MeuCatalogo.Features.Catalogo.Responses;

namespace MeuCatalogo.Features.Settings.Services;

public sealed class SettingsService : ISettingsService
{
    public CatalogoResponse? CatalogoFavorito
    {
        get
        {
            string? raw = Preferences.Get(nameof(CatalogoFavorito), null);
            return string.IsNullOrWhiteSpace(raw)
                ? null
                : JsonSerializer.Deserialize<CatalogoResponse>(raw);
        }
        set
        {
            if (value == null)
                Preferences.Remove(nameof(CatalogoFavorito));
            else
            {
                string json = JsonSerializer.Serialize(value);
                Preferences.Set(nameof(CatalogoFavorito), json);
            }
        }
    }

    public Task ClearAllAsync()
    {
        Preferences.Clear();
        return Task.CompletedTask;
    }
}
