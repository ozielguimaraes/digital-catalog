# Mobile

.NET MAUI cross-platform app (Android / iOS / Windows / MacCatalyst / Tizen) using Clean Architecture + MVVM with `CommunityToolkit.Mvvm`. Offline-first for catalogs and products.

## Layout

- **`MeuCatalogo/`** — the MAUI project.
- **`mobile.sln`** — solution file.
- **`offline-rules.txt`** — design notes for the offline-first pattern (read if modifying sync behavior).

## Project Structure (`MeuCatalogo/`)

- **`MauiProgram.cs`** — `MauiAppBuilder` composition: Sentry, Community Toolkit, Plugin.Maui.BottomSheet, Fingerprint, fonts, and the DI extension methods (`AddClientServices`, `AddApplicationServices`, `AddViewModels`).
- **`App.xaml.cs`** — lifecycle. `OnHandlerChanged` initializes the DB and registers the connectivity listener; `OnConnectivityChanged` triggers sync when online. `CreateWindow` routes to `LoginPage` or `AppShell` based on auth state.
- **`AppShell.xaml` / `AppShellViewModel.cs`** — tab navigation shell; registers routes for the feature pages; loads header user info.
- **`BottomSheetKeys.cs` / `BottomSheetParameters.cs`** — named identifiers and parameter keys for bottom-sheet navigation.
- **`Core/`** — abstractions (`IRepository`, `IUseCase`, `IImageProcessor`) and base types. See `MeuCatalogo/Core/CLAUDE.md`.
- **`Features/`** — Auth, Catalogo, Categoria, Estoque, Produto, Settings. See `MeuCatalogo/Features/CLAUDE.md`.
- **`Infrastructure/`** — SQLite DbContext, SyncEngine, HTTP auth handlers, image processor, navigation service. See `MeuCatalogo/Infrastructure/CLAUDE.md`.
- **`Extensions/ServiceCollectionExtension.cs`** — central DI wiring: Refit clients (with `AuthenticationHandler` + `LoggingHttpClientHandler` + Polly retry), repositories, data sources, use cases, pages, ViewModels, sync handlers, bottom sheets.
- **`Domain/`** — cross-cutting domain models (`SyncQueue`, `SyncOperation`, `SyncStatus`).
- **`Behaviors/`** — attached behaviors (e.g., `CurrencyBehavior`).
- **`Components/`** — custom XAML controls (`CustomEntry`, `DropDown`, `NumericEntry`).
- **`Converters/`** — XAML value converters.
- **`Platforms/`** — per-platform hooks (mostly stubs).
- **`Resources/`** — fonts, images, colors, styles.

## Key Decisions

- **SQLite via `sqlite-net-pcl`** (NOT EF Core). Schema created in `AppDbContext.InitializeAsync`; additive migrations via `EnsureColumnExistsAsync`.
- **Offline-first** for `Catalogo` and `Produto`: save locally, queue sync, reconcile when online. `Categoria` and `Estoque` are online-only.
- **Refit + Polly** for HTTP. All clients route through `AuthenticationHandler` (token refresh on 401) and `LoggingHttpClientHandler`.
- **Flunt** for validation inside UseCases (fail-fast).
- **`CommunityToolkit.Mvvm`** source generators for `[ObservableProperty]` and `[RelayCommand]`.
- **`WeakReferenceMessenger`** for cross-ViewModel events (e.g., `ProdutoUpsertedMessage`).
- **`Plugin.Maui.BottomSheet`** for bottom-sheet UX (`CategoriaBottomSheet`, `EstoqueBottomSheet`).

## Integration

- Talks to the `../backend/` API over HTTPS. Base URL in `Infrastructure/Auth/ApiConstants.cs`.
- Contract envelope `ApiResponse<T>` mirrors the backend; errors unwrapped in feature repositories / `BaseApiService`.
- `CatalogoFavorito` is the pivot point: required for product creation, persisted in `Preferences` by `Features/Settings`.

## Gotchas

- **No EF Core on mobile** — don't `using Microsoft.EntityFrameworkCore`. Queries are `Table<T>()` or raw SQL.
- **Sync queue has no TTL** — failed items sit forever; no dead-letter / retry cap.
- **ID remapping** after sync: locally-created entities get a new server GUID on first successful sync. Old client UUIDs are stale everywhere.
- **Currency parsing is locale-aware** — use `TentarConverterPreco`, not `decimal.Parse`.
- **Token refresh is inside `AuthenticationHandler`** — don't add 401 retry logic in ViewModels.
- **BottomSheet parameters are `object`** — always `TryGetValue` + type-check.
- **Platforms/** is mostly empty stubs. Android entry-handler customization (emoji disable) is the only non-stub; iOS/Windows inherit defaults.
- **Connectivity listener is never unregistered** — guard against double-registration if refactoring lifecycle.

## When Adding Code

- New feature → mirror `Features/Produto/` layout (Presentation + Data/Local + Data/Remote + Domain + UseCases + Validators + Sync).
- Register all new types in `Extensions/ServiceCollectionExtension.cs` — pages/ViewModels/UseCases are `Transient`; services/DB/navigation are `Singleton`.
- New SQLite entity → add CREATE TABLE in `AppDbContext.InitializeAsync` AND `EnsureColumnExistsAsync` migrations for existing installs.
- New bottom sheet → `AddBottomSheet<TPage, TViewModel>(BottomSheetKeys.Xxx)`; define the key/parameters at the project root.
