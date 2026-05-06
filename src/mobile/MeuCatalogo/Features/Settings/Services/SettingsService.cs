using System.Text.Json;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Settings.Services;

public sealed class SettingsService : ISettingsService
{
    public CatalogoInfo? CatalogoFavorito
    {
        get => Ler(nameof(CatalogoFavorito));
        set => Escrever(nameof(CatalogoFavorito), value);
    }

    public CatalogoInfo? CatalogoEmUso
    {
        get => Ler(nameof(CatalogoEmUso));
        set => Escrever(nameof(CatalogoEmUso), value);
    }

    public Task ClearAllAsync()
    {
        Preferences.Clear();
        return Task.CompletedTask;
    }

    private static CatalogoInfo? Ler(string chave)
    {
        string? raw = Preferences.Get(chave, null);
        return string.IsNullOrWhiteSpace(raw)
            ? null
            : JsonSerializer.Deserialize<CatalogoInfo>(raw);
    }

    private static void Escrever(string chave, CatalogoInfo? valor)
    {
        if (valor is null)
            Preferences.Remove(chave);
        else
            Preferences.Set(chave, JsonSerializer.Serialize(valor));
    }
}
