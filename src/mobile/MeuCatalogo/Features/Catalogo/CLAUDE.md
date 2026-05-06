# Catalogo Feature

User's catalogs (stores/shops). Offline-first: the list page shows cached SQLite data instantly, then pulls fresh data in the background.

## Files

- **`CatalogoListaPage.xaml` / `CatalogoListaPageViewModel.cs`**: Catalog list with favorite selection.
- **`CatalogoAdicionarPage.xaml` / `CatalogoAdicionarPageViewModel.cs`**: Create/edit catalog form.
- **`Data/CatalogoRepository.cs` (`ICatalogoRepository`)**: Thin wrapper that delegates to the remote data source.
- **`Data/CatalogoLocalRepository.cs` (`ICatalogoLocalRepository`)**: SQLite-backed list/upsert/delete of `CatalogoEntity`.
- **`Data/Remote/CatalogoRemoteDataSource.cs` (`ICatalogoRemoteDataSource`)**: Wraps `ICatalogoApi` Refit calls.
- **`Data/Remote/ICatalogoApi.cs`**: Refit interface — list, get, create, update, delete.
- **`Data/Remote/Contracts/`**: `CatalogoCreateRequest`, `CatalogoUpdateRequest`, `CatalogoResponse`.
- **`Data/Sync/CatalogoPullSyncHandler.cs` (`ISyncHandler`)**: Pull-only handler — fetches latest from API and upserts into SQLite. Matches `SyncEntityTypes.Catalogos`.
- **`Domain/Catalogo.cs`**: Public domain DTO returned by use cases.
- **`Domain/CatalogoEntity.cs`**: SQLite row.
- **`UseCases/GetCatalogosUseCase.cs`**: Remote fetch.
- **`UseCases/GetCatalogosLocalUseCase.cs`**: Local SQLite read; manually maps entity → `Catalogo`.
- **`UseCases/SyncCatalogosUseCase.cs`**: Enqueues a pull via `ISyncEngine`.
- **`UseCases/CreateCatalogoUseCase.cs`**: Remote create + local upsert.
- **`UseCases/DeleteCatalogoUseCase.cs`**: Remote delete + local delete.
- **`UseCases/SetCatalogoFavoritoUseCase.cs`**: Updates `ISettingsService.CatalogoFavorito`.

## Patterns

- **Local-first read**: `CatalogoListaPageViewModel` calls `GetCatalogosLocalUseCase` to populate the UI immediately, then fires `SyncCatalogosUseCase` in `Task.Run()` and re-queries on completion.
- **Background → main-thread UI**: Sync callback uses `MainThread.BeginInvokeOnMainThread` to update `ObservableCollection`.
- **Auto-favorite**: If exactly one catalog exists locally and no favorite is set, auto-select it. This is load-bearing for product creation flows.
- **Thin repository**: `CatalogoRepository` does not persist — local upsert happens inside the pull sync handler or individual use cases. Do not expect `CatalogoRepository` to cache.

## Integration

- Sync: `CatalogoPullSyncHandler` is registered as `ISyncHandler` in `ServiceCollectionExtension`. `ISyncEngine` dispatches queue items with `EntityType = SyncEntityTypes.Catalogos` here.
- Settings: `SetCatalogoFavoritoUseCase` writes to `ISettingsService`, consumed by `Features/Produto` and `AppShell`.
- DB: `CatalogoEntity` is in the `AppDbContext.InitializeAsync` CREATE TABLE list.

## Gotchas

- **`ShowEmptyState` flips on `HasCatalogos` count** — if you render the list before the local read completes, the UI will flicker through empty state. Keep the initial load synchronous with `GetCatalogosLocalUseCase` before any async sync.
- **Auto-favorite only triggers with exactly one catalog** — users with zero or many must pick manually.
- **`CatalogoRepository` does not write to SQLite** on success — only the pull handler does, which means `CreateCatalogoUseCase` must also call `CatalogoLocalRepository` explicitly.
- **Entity → Domain mapping is manual** in `GetCatalogosLocalUseCase`. No AutoMapper.

## When Adding Code

- New read path → `GetXxxLocalUseCase` for cached, `GetXxxUseCase` for live.
- New write path → update local immediately after remote success to keep the local cache consistent with UI expectations.
- New sync handler → register as `ISyncHandler` via `AddTransient<ISyncHandler, YourHandler>()`.
