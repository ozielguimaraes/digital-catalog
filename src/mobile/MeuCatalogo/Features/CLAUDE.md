# Features

Clean-architecture feature folders. Each feature has its own `Data/` (Local + Remote), `Domain/`, `UseCases/`, and `Presentation/` (pages + ViewModels). MVVM via `CommunityToolkit.Mvvm`.

## Submodules

- **`Auth/`** — Login, signup, token management. See `Auth/CLAUDE.md`.
- **`Catalogo/`** — User catalogs. Offline-first (local-first read + background sync). See `Catalogo/CLAUDE.md`.
- **`Categoria/`** — Category picker bottom sheet. Online-only. See `Categoria/CLAUDE.md`.
- **`Estoque/`** — Stock settings bottom sheet. UI-only, no persistence. See `Estoque/CLAUDE.md`.
- **`Produto/`** — Products. Offline-first save with queued sync; most complex feature. See `Produto/CLAUDE.md`.
- **`Settings/`** — `ISettingsService` with `CatalogoFavorito`. See `Settings/CLAUDE.md`.

## Cross-Feature Patterns

### Layering (per feature)

```
Presentation (Page.xaml + PageViewModel)
    ↓
UseCase (validates, orchestrates)
    ↓
Repository (IXxxRepository — often thin wrapper)
    ├→ Local (SQLite via IXxxLocalRepository)
    └→ Remote (IXxxRemoteDataSource → Refit IXxxApi)
```

### Data flow (offline-first features)

1. ViewModel calls UseCase.
2. UseCase runs Flunt validator (fail-fast).
3. Check `Connectivity.NetworkAccess`.
4. Online → remote call → local upsert with `SyncStatus.Completed` → return.
5. Offline → local upsert with `SyncStatus.Pending` → `ISyncEngine.QueueSyncAsync(...)` → return.
6. Background: `ISyncEngine` processes queue when online; handlers reconcile IDs.

### Naming conventions

| Kind | Suffix | Examples |
|---|---|---|
| Pages | `Page` | `LoginPage`, `CatalogoListaPage` |
| ViewModels | `PageViewModel` / `BottomSheetViewModel` | `LoginPageViewModel`, `CategoriaBottomSheetViewModel` |
| UseCases | `UseCase` | `SigninUseCase`, `UpsertProdutoOfflineFirstUseCase` |
| Repositories | `Repository` | `AuthRepository`, `ProdutoRepository` |
| Local data | `LocalRepository` / `LocalDataSource` | `ProdutoLocalRepository`, `AuthLocalDataSource` |
| Remote data | `RemoteDataSource` | `AuthRemoteDataSource` |
| Refit clients | `I*Api` | `IAuthApi`, `IProdutoApi` |
| Validators | `Validator` (Flunt `Contract<T>`) | `SigninValidator`, `UpsertProdutoValidator` |
| Entities (SQLite) | `Entity` | `ProdutoEntity`, `UserEntity` |
| API contracts | `*Request` / `*Response` | `SigninRequest`, `ProdutoResponse` |
| Messages | `Message` (WeakReferenceMessenger) | `ProdutoUpsertedMessage` |

### Navigation

- **Shell routes**: `INavigationService.NavigateToAsync($"//{nameof(XxxPage)}")`.
- **Shell query params**: `IQueryAttributable.ApplyQueryAttributes(query)`.
- **BottomSheet**: `IBottomSheetNavigationService.NavigateToAsync<TViewModel>(BottomSheetKeys.Xxx, parameters)`. Sheets implement `INavigationAware` (`OnNavigatedTo(parameters)`).

### Validation

- Flunt `Contract<T>` subclasses. Runs inside UseCases (fail-fast), not ViewModels.
- Exception: `Categoria` / `Estoque` bottom sheets do inline ViewModel validation because the UX shows per-field errors as-you-type.
- `Produto` ViewModel has a partial `.validations.cs` that runs the Flunt validator and maps notifications to `ErrorMessage` bindings per field.

## Integration

- DI: Every ViewModel, Page, UseCase, Repository, DataSource, and sync handler is registered in `../Extensions/ServiceCollectionExtension.cs`. Pages and ViewModels are `Transient`; services, DB, navigation are `Singleton`.
- HTTP: All Refit clients run through `AuthenticationHandler` (token injection + 401 refresh) and `LoggingHttpClientHandler`, with a Polly retry policy.
- Sync: `Catalogo` and `Produto` plug into `ISyncEngine` via their `Sync/*Handler.cs` files. Handlers are registered multiple times as `ISyncHandler` and dispatched by `CanHandle(entityType, operation)`.
- Messaging: `WeakReferenceMessenger.Default` for cross-ViewModel events (e.g., `ProdutoUpsertedMessage` notifies the list after upsert).

## Gotchas

- **Two "get by id" flavors**: For products there is both a remote `GetProdutoByIdUseCase` and a local `GetProdutoForEditOfflineFirstUseCase`. Pick based on context.
- **BottomSheet parameters are `object`** — always `TryGetValue` + type check.
- **`CatalogoFavorito` must be set** before product save — enforced in `UpsertProdutoOfflineFirstUseCase`.
- **Offline vs online features differ** — `Categoria` has no local cache; `Catalogo` and `Produto` do. Do not assume all features share behavior.
- **ID remapping after sync** — locally-created entities get a new server GUID on first successful sync. Any stored references to the client UUID are stale.
- **Currency parsing is locale-aware** (`TentarConverterPreco`) — don't `decimal.Parse` directly.

## When Adding Code

- New feature → mirror the layout of `Produto/` (Presentation + Data/Local + Data/Remote + Domain + UseCases + optionally Validators and Sync).
- Keep repositories thin; persistence is the local repository's job. The "main" repository often just delegates to remote.
- Register everything in `ServiceCollectionExtension.cs`.
- For offline-first writes, use the `UpsertProdutoOfflineFirstUseCase` as the reference pattern.
