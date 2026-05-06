# Infrastructure

Concrete adapters: SQLite database, offline sync engine, HTTP auth pipeline, navigation service, image processor.

## Submodules

- **`Database/`** — SQLite (`sqlite-net-pcl`, NOT EF Core). See `Database/CLAUDE.md`.
- **`SyncEngine/`** — Offline queue + handler dispatch + exponential backoff. See `SyncEngine/CLAUDE.md`.
- **`Auth/`**:
  - `ApiConstants.cs` — base URL (`http://catalogo-api.sanyz.com.br/api`).
  - `AuthenticationHandler.cs` — `DelegatingHandler` that injects Bearer tokens, handles 401 by refreshing and retrying (clones the request), and navigates to `LoginPage` on refresh failure. Excludes `/auth/login`, `/auth/refresh-token`, `/auth/register`.
  - `LoggingHttpClientHandler.cs` — logs every request/response with a correlation UUID and elapsed time.
  - `INavigationService.cs` — Shell-navigation facade.
- **`Navigation/NavigationService.cs`** — `INavigationService` implementation. `InitializeAsync()` routes to `LoginPage`, `CatalogoListaPage`, or `ProdutoAdicionarPage` based on auth + `CatalogoFavorito`.
- **`Imaging/MauiImageProcessor.cs`** — `IImageProcessor` using `PlatformImage` (MAUI Graphics) to resize and re-encode as JPEG at configurable quality (default 0.75).
- **`Validation/ValidatableObject.cs`** — fully commented out; placeholder, not used.

## Patterns

- **DelegatingHandler chain**: Refit clients are configured with `.AddHttpMessageHandler<AuthenticationHandler>().AddHttpMessageHandler<LoggingHttpClientHandler>()` plus a Polly retry policy (exponential backoff 3×).
- **Lazy DB init on every repo call**: Safe due to `SemaphoreSlim` inside `AppDbContext.InitializeAsync`.
- **Navigation on background threads**: The 401-refresh redirect to `LoginPage` uses `MainThread.BeginInvokeOnMainThread` to marshal to the UI thread.

## Integration

- DI (`Extensions/ServiceCollectionExtension.cs`):
  - `AppDbContext` — Singleton.
  - `ISyncEngine` → `SyncEngineService` — Singleton.
  - `INavigationService` → `NavigationService` — Singleton.
  - `IImageProcessor` → `MauiImageProcessor` — Singleton.
  - `AuthenticationHandler`, `LoggingHttpClientHandler` — Transient (handler chain).
  - `ISyncHandler` — Transient, multi-register (one per feature).
- Lifecycle (`App.xaml.cs`):
  - `OnHandlerChanged` initializes the DB, registers `Connectivity.ConnectivityChanged`.
  - `OnConnectivityChanged` calls `ProcessPendingSyncAsync` when internet is restored.

## Gotchas

- **Refresh-token exclusion list** in `AuthenticationHandler` must be kept in sync with the actual auth routes. Adding an auth endpoint that should skip token injection requires editing the exclusion list.
- **401-refresh is synchronous inside the handler** — wrapped in `Task.Run` but logged as a warning. A slow refresh can cascade into visible UI delay.
- **Connectivity listener is registered but never unregistered** — if the app recreates its main window, duplicate handlers can fire. Guard against double-registration if you refactor.
- **`ValidatableObject` is commented out** — validation uses Flunt at the UseCase level instead. Don't revive this file without a plan.
- **`MauiImageProcessor` returns a `MemoryStream`** — callers own disposal. Leaking these under heavy upload loads causes GC pressure.
- **Navigation initialization is manual** — `NavigationService.InitializeAsync()` is called from AppShell/App startup code; it is not auto-invoked by DI.

## When Adding Code

- New HTTP handler → transient, chain via `.AddHttpMessageHandler<>()` after the existing handlers.
- New infrastructure singleton → register once, inject where needed; don't create feature-local copies.
- New database entity → extend `AppDbContext.InitializeAsync` CREATE list + add `EnsureColumnExistsAsync` migration hook for existing installs.
- New sync handler → implement `ISyncHandler`, register `AddTransient<ISyncHandler, YourHandler>()`.
