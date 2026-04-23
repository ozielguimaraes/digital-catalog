# Produto Feature

Products — the most complex feature. **Offline-first** for create/update/delete: saves locally to SQLite instantly, queues a sync operation, uploads images when online. The save path is the reference pattern for offline-first in this codebase.

## Files

### Presentation/
- **`ProdutoAdicionarPage.xaml` / `ProdutoAdicionarPageViewModel.cs`**: Add/edit product form.
- **`ProdutoAdicionarPageViewModel.validations.cs`**: Partial class containing `ValidateAll()` / `ValidateFields(params string[])`. Splits Flunt validation out of the main ViewModel for readability.
- **`ProdutoListaPage.xaml` / `ProdutoListaPageViewModel.cs`**: Product list.
- **`ProdutosViewModel.cs`**: Shared base/list logic.
- **`ProdutoMessages.cs`**: `WeakReferenceMessenger` message types (e.g., `ProdutoUpsertedMessage`) used to notify the list after an upsert.

### Data/
- **`ProdutoRepository.cs` (`IProdutoRepository`)**: Thin wrapper over the remote data source.
- **`Local/ProdutoLocalRepository.cs` (`IProdutoLocalRepository`)**: SQLite CRUD for `ProdutoEntity`.
- **`Local/ProdutoImagemLocalRepository.cs` (`IProdutoImagemLocalRepository`)**: SQLite CRUD for `ProdutoImagemEntity`. Includes `ReplaceByProdutoIdAsync` for full image-set rewrites.
- **`Remote/ProdutoRemoteDataSource.cs` (`IProdutoRemoteDataSource`)**: Wraps `IProdutoApi`. Multipart image upload lives here.
- **`Remote/IProdutoApi.cs`**: Refit — list, get, create, update, delete, upload image.
- **`Remote/Contracts/Requests/`**: `ProdutoCreateRequest`, `ProdutoUpdateRequest`.
- **`Remote/Contracts/Responses/`**: `ProdutoResponse`, `ProdutoImagemResponse`.
- **`Sync/ProdutoPullSyncHandler.cs` (`ISyncHandler`)**: Pulls product lists from API on demand. Matches `SyncEntityTypes.ProdutosByCatalogo`.
- **`Sync/ProdutoUpsertSyncHandler.cs` (`ISyncHandler`)**: Flushes `Pending` create/update entities. Handles client-UUID → server-GUID remapping inside a transaction (`RunInTransactionAsync`).
- **`Sync/ProdutoDeleteSyncHandler.cs` (`ISyncHandler`)**: Flushes pending deletes.

### Domain/
- **`ProdutoEntity.cs`**: SQLite row. Has `SyncStatus` (Pending/Completed/Failed).
- **`ProdutoImagemEntity.cs`**: SQLite row for product images.

### UseCases/
- **`GetProdutosByCatalogoIdUseCase.cs`**: Remote list by catalog.
- **`GetProdutoByIdUseCase.cs`**: Remote fetch by id.
- **`GetProdutoForEditOfflineFirstUseCase.cs`**: Local SQLite fetch for editing.
- **`SyncProdutosByCatalogoUseCase.cs`**: Enqueue a pull for a catalog.
- **`CreateProdutoUseCase.cs` / `CreateProdutoRemoteUseCase.cs`**: Create paths — "Remote" hits the API, the base wraps it.
- **`UpdateProdutoRemoteUseCase.cs`**: Remote update.
- **`DeleteProdutoRemoteUseCase.cs` / `DeleteProdutoOfflineFirstUseCase.cs`**: Delete paths.
- **`UpsertProdutoOfflineFirstUseCase.cs`**: Main save path. Validates, checks connectivity, routes online vs offline, persists locally, queues sync when offline.
- **`UploadProdutoImageUseCase.cs`**: Multipart image upload (per-image).

### Validators/
- **`UpsertProdutoValidator.cs`**: Flunt `Contract<UpsertProdutoOfflineFirstRequest>` — name, price, category, discounted-price rules.

## Patterns

### Offline-first save (`UpsertProdutoOfflineFirstUseCase`)

```
Validate (Flunt fail-fast)
 → Check Connectivity.NetworkAccess
    → Online:  CreateProdutoRemoteUseCase / UpdateProdutoRemoteUseCase
               → Persist locally with SyncStatus.Completed
               → Return remote response
    → Offline: Local upsert with SyncStatus.Pending
               → Replace image entities for the product
               → Queue SyncQueue row (Create or Update; pending-update stays Create)
               → Return pending response
```

### Image handling

- Local capture → `IImageProcessor.CompressAsync` → save to `FileSystem.AppDataDirectory/{guid}.{ext}` → tracked as `ProdutoImagemResponse` with `SyncStatus.Pending`.
- After a successful online save, the ViewModel calls `SincronizarImagensPendentesAsync` which iterates images, skips already-remote URLs, and invokes `UploadProdutoImageUseCase` for each local file.
- Order: `Ordem` is 1-based; `IsPrincipal = true` on the first image. Reordering and deletion must keep both fields consistent.

### ViewModel structure

- `[ObservableProperty]` for bound fields; `partial void OnXxxChanged(...)` triggers per-field validation.
- `TentarConverterPreco` handles locale-specific decimal parsing (thousands and decimal separators).
- Two validation entry points: `ValidateAll()` before save, `ValidateFields(params string[])` for per-field on-change.
- `ApplyValidationMessages` clears existing error messages before re-running validators.
- `WeakReferenceMessenger.Default.Send(new ProdutoUpsertedMessage(id))` after save so the list refreshes.

## Integration

- DI: Every repository, data source, use case, sync handler, page, and ViewModel is registered in `Extensions/ServiceCollectionExtension.cs`.
- `ISyncEngine` dispatches queued `ProdutoEntity` operations to the three `ISyncHandler` implementations based on `SyncOperation`.
- Consumes: `Features/Categoria/*` (bottom sheet for category pick), `Features/Estoque/*` (bottom sheet for stock), `Features/Settings/ISettingsService` (CatalogoFavorito).
- `CatalogoFavorito` **must** be set — `UpsertProdutoOfflineFirstUseCase` guards on it.

## Gotchas

- **Two different "get by id" use cases**: `GetProdutoByIdUseCase` hits the remote API; `GetProdutoForEditOfflineFirstUseCase` reads from SQLite. Pick the correct one for the context — the edit screen uses the offline one.
- **Pending-update collapses into Create**: If a locally-queued Create is edited before it syncs, the sync queue entry stays `Create` (with the new payload). Don't "upgrade" it to Update.
- **ID remapping is inside the upsert sync handler**: When a `Create` syncs, the server returns a new GUID; the handler updates the local `ProdutoEntity.Id` and all `ProdutoImagemEntity.ProdutoId` foreign keys inside a transaction. Any code holding the old client UUID afterwards is stale.
- **Image SyncStatus stays Pending until the product itself is Completed** — uploads are gated on the parent product being synced.
- **Currency parsing is locale-aware** (`TentarConverterPreco`). Don't `decimal.Parse(text)` directly — it will fail on Portuguese-formatted input.
- **Validation file is a partial class**: `ProdutoAdicionarPageViewModel.validations.cs` shares the class declaration with the main file. Keep them in sync if you change access modifiers or generics.
- **`WeakReferenceMessenger` messages bypass DI** — subscribers must `Register` explicitly; don't rely on implicit discovery.
- **Upload use case runs per-image** — it is not batched. Large image sets make many HTTP calls.
- **The `ProdutoRepository` does not persist to SQLite** — persistence is explicit in use cases and the upsert sync handler.

## When Adding Code

- New field on `Produto` → extend request DTO, response DTO, entity, ViewModel binding, validator, `ProdutoUpsertSyncHandler` payload deserialization, and likely a DB migration (see `AppDbContext.EnsureColumnExistsAsync`).
- New save-side rule → add to `UpsertProdutoValidator`; do not duplicate in the ViewModel.
- New write path must honor offline-first: persist locally, queue sync, return immediately. Never block the UI thread on the network.
- Register new UseCases/Repositories/DataSources in `ServiceCollectionExtension.cs`.
