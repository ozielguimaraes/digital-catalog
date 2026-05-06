# Settings Feature

Per-user app settings. Currently only holds the favorite catalog selection.

## Files

- **`Services/ISettingsService.cs`**: Contract — `CatalogoFavorito` (get/set), `ClearAllAsync()`.
- **`Services/SettingsService.cs`**: Backed by `Preferences` (MAUI) for persistence. `CatalogoFavorito` serializes to JSON.

## Patterns

- **Singleton DI**: Registered once in `ServiceCollectionExtension`. Any feature can inject and read/write.
- **JSON serialization**: `CatalogoFavorito` is a `CatalogoInfo` (id + nome) stored as a single JSON string under a known key.
- **Set during auth**: `SigninUseCase` writes `CatalogoFavorito` when the login response carries a favorite. `LogoutUseCase` clears it.

## Integration

- Consumers:
  - `Features/Auth/UseCases/SigninUseCase` — sets on login.
  - `Features/Auth/UseCases/LogoutUseCase` — clears.
  - `Features/Catalogo/UseCases/SetCatalogoFavoritoUseCase` — updates when user switches.
  - `Features/Produto/UseCases/UpsertProdutoOfflineFirstUseCase` — guards product save on this being non-null.
  - `AppShellViewModel` — reads for UI.

## Gotchas

- **Load-bearing for product creation**: If `CatalogoFavorito` is null, product save fails. Always set it when the user has at least one catalog.
- **`Preferences` is not secure storage** — this is fine for a non-sensitive identifier, but don't add tokens or PII here. Secrets go in `SecureStorage` via `IAuthLocalDataSource`.
- **Clearing on logout is mandatory** — otherwise the next user inherits the previous user's favorite.
- **Key serialization** is JSON — changing the `CatalogoInfo` shape breaks stored values on upgrade; use additive changes or clear the key on version bump.

## When Adding Code

- New setting → add to `ISettingsService` interface, implement in `SettingsService`, use a stable string key, and clear it in `ClearAllAsync`.
- Do not store tokens or credentials here — those go in `IAuthLocalDataSource` (SecureStorage).
