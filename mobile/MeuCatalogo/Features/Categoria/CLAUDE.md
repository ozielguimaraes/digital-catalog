# Categoria Feature

Product categories within a catalog. Lives entirely in a bottom sheet — no full page. Offline-first: SQLite is the SSOT and the API is synchronized via the SyncEngine.

## Files

- **`CategoriaBottomSheet.xaml` / `CategoriaBottomSheetViewModel.cs`**: Selection list with inline create/edit.
- **`Data/CategoriaRepository.cs` (`ICategoriaRepository`)**: Remote create/update, and mirrors successful writes into SQLite to keep SSOT consistent.
- **`Data/Local/CategoriaLocalRepository.cs` (`ICategoriaLocalRepository`)**: SQLite-backed list/replace/upsert of `CategoriaEntity`.
- **`Data/Remote/CategoriaRemoteDataSource.cs` (`ICategoriaRemoteDataSource`)**: Wraps `ICategoriaApi`.
- **`Data/Remote/ICategoriaApi.cs`**: Refit — list by catalog, create, update.
- **`Data/Remote/Contracts/Requests/CategoriaUpsertRequest.cs`**: Create/update payload.
- **`Data/Remote/Contracts/Responses/CategoriaResponse.cs`**: API response.
- **`Domain/Categoria.cs`**: Domain model.
- **`Domain/CategoriaEntity.cs`**: SQLite row.
- **`Models/CategoriaModel.cs`**: UI-facing model with `IsSelected` and edit-in-progress state.
- **`UseCases/GetCategoriasByCatalogoUseCase.cs`**: Local-first read (SSOT). If empty and online, performs a pull sync; otherwise triggers background sync.
- **`UseCases/SyncCategoriasByCatalogoUseCase.cs`**: Enqueues a pull via `ISyncEngine` and processes the queue.
- **`UseCases/CreateCategoriaUseCase.cs`**: Remote create + local upsert.
- **`UseCases/UpdateCategoriaUseCase.cs`**: Remote update + local upsert.
- **`Data/Sync/CategoriaPullSyncHandler.cs` (`ISyncHandler`)**: Pull-only handler — fetches latest from API and replaces SQLite by `CatalogoId`. Matches `SyncEntityTypes.CategoriasByCatalogo`.

## Patterns

- **BottomSheet-as-selector**: The sheet is opened with a pre-built `List<CategoriaModel>`; the caller passes the list in via `BottomSheetNavigationParameters[BottomSheetParameters.Categorias]`. On select, the sheet returns the chosen model via `BottomSheetParameters.CategoriaSelectionada`.
- **`INavigationAware`** on both sides: sheet ViewModel reads input in `OnNavigatedTo(parameters)`; caller ViewModel reads selection in its own `OnNavigatedTo(parameters)` after `GoBackAsync`.
- **Inline create/edit**: No separate dialog for new/rename — the same sheet toggles into edit mode.
- **ViewModel validation**: Length, non-empty, duplicate-by-name checks happen inside the ViewModel, not a Flunt validator. This is intentional because the UX surfaces errors inline.
- **SSOT in SQLite**: The caller should treat `GetCategoriasByCatalogoUseCase` as authoritative (local-first) and not depend on remote calls to render the sheet.

## Integration

- Opened from `Features/Produto/Presentation/ProdutoAdicionarPageViewModel` using `IBottomSheetNavigationService.NavigateToAsync<CategoriaBottomSheetViewModel>(BottomSheetKeys.ListaCategoria, parameters)`.
- `BottomSheetKeys.ListaCategoria` and `BottomSheetParameters.Categorias` / `CategoriaSelectionada` are defined at the mobile project root.
- Registered via `AddBottomSheet<CategoriaBottomSheet, CategoriaBottomSheetViewModel>(BottomSheetKeys.ListaCategoria)` in `ServiceCollectionExtension`.
- DB: `CategoriaEntity` is created in `AppDbContext.InitializeAsync` and indexed by `CatalogoId`.

## Gotchas

- **BottomSheet parameters are typed as `object`** — always `TryGetValue` + type-check, never direct cast. A wrong type silently breaks the sheet.
- **`CategoriaModel` vs `Categoria`**: `CategoriaModel` is the UI selection model (with `IsSelected`); `Categoria` is the domain model. Don't pass domain models into/out of the sheet — convert first.
- **First run offline**: If the app never pulled categories for a catalog and the device is offline, the SQLite list is empty and the sheet will show empty (but still allows creating locally only if the API supports it; today create/update remain online-only).
- **Validation is ViewModel-local** — don't look for a Flunt `CategoriaValidator`; it doesn't exist.
- **Duplicate-name check is client-side only** — the API may still 409 if race conditions happen.

## When Adding Code

- Keep the sheet self-contained — new category fields go on `CategoriaUpsertRequest` and both request-builder paths (create and update).
- New selection result → add a key to `BottomSheetParameters`, wire `GoBackAsync(parameters)` on this side and `OnNavigatedTo(parameters)` on the caller.
