# SyncEngine

Offline-first sync engine. Queues operations into a SQLite `SyncQueue` table and dispatches them to registered `ISyncHandler` implementations when the device is online. Drives offline behavior for `Catalogo` and `Produto`.

## Files

- **`ISyncEngine.cs`**: Contract — `QueueSyncAsync(entityType, entityId, operation, payload)`, `ProcessQueueAsync()`.
- **`ISyncHandler.cs`**: Per-entity handler contract — `bool CanHandle(string entityType, SyncOperation op)` and `Task HandleAsync(SyncQueueItem item)`.
- **`SyncEntityTypes.cs`**: String constants — `"Catalogos"`, `"ProdutosByCatalogo"`. Matches the `CanHandle` predicate on each handler.
- **`SyncEngineService.cs`**: The implementation. Queue + processing loop + retry.

## Architecture

### Queue

- `QueueSyncAsync` writes a new `SyncQueue` row with `Status = Pending`, `RetryCount = 0`, `NextRetryAt = now`.
- If online, it fires `ProcessQueueAsync` in the background (non-blocking). If offline, it just writes the row.

### Processing

- `ProcessQueueAsync` is guarded by an internal lock — only one processing loop runs at a time.
- Short-circuits when `Connectivity.NetworkAccess != Internet`.
- Selects rows where `Status IN (Pending, Failed) AND NextRetryAt <= now`, processed sequentially.
- For each row: finds the first `ISyncHandler` whose `CanHandle(entityType, operation)` returns true, calls `HandleAsync(item)`.
- On success: `Status = Completed` (row kept as history). On exception: `Status = Failed`, `RetryCount++`, `LastError = ex.Message`, `NextRetryAt = now + backoff`.

### Retry

- Exponential backoff: `NextRetryAt = now + min(2^retryCount, 300) seconds` (capped at 5 minutes).
- No max retry count — a failed item retries forever until success or manual cleanup.

### Handler dispatch

Handlers are registered with `AddTransient<ISyncHandler, SomeHandler>()`. Multiple handlers can exist for the same `entityType` as long as their `CanHandle` predicates differentiate by `SyncOperation`:

```
ISyncHandler → CatalogoPullSyncHandler      (Catalogos, Pull)
ISyncHandler → ProdutoPullSyncHandler       (ProdutosByCatalogo, Pull)
ISyncHandler → ProdutoUpsertSyncHandler     (ProdutoEntity, Create|Update)
ISyncHandler → ProdutoDeleteSyncHandler     (ProdutoEntity, Delete)
```

## Integration

- DI: `SyncEngineService` is **Singleton**. Handlers are `Transient` (resolved per queue item).
- Lifecycle: `App.xaml.cs.OnHandlerChanged()` wires `Connectivity.ConnectivityChanged`. When internet is restored, `OnConnectivityChanged` calls `ProcessPendingSyncAsync()`.
- Callers: features like `Produto` and `Catalogo` call `QueueSyncAsync` from their use cases / sync-handler entry points.
- Data: `SyncQueue` table is created in `AppDbContext.InitializeAsync`.

## Gotchas

- **No retry cap**: Failed items accumulate in `SyncQueue` indefinitely. There is no TTL, no dead-letter handling, and no in-app UI to flush stuck rows. Debug "sync is stuck" by querying `SyncQueue` directly.
- **Connectivity listener lifetime**: Registered in `OnHandlerChanged` but never explicitly unregistered. If the app window is recreated, duplicate listeners may fire — check before adding more listener logic.
- **Handler resolution picks the FIRST match**: If two handlers return true from `CanHandle` for the same item, the first wins. Keep predicates disjoint.
- **Payload is a JSON string**: Handlers deserialize it themselves; shape changes are breaking unless handlers defensively read with `JsonElement`/default values.
- **Processing is sequential**: A slow handler blocks the whole queue. Avoid synchronous long-running work inside `HandleAsync`.
- **ID remapping is the handler's problem**: When a local `Create` syncs and the server assigns a new GUID, the handler must update the local entity id and any foreign keys (see `ProdutoUpsertSyncHandler` using `RunInTransactionAsync`).
- **Backoff is in-memory once scheduled** — restarting the app does not reset `NextRetryAt`; the queue resumes from persisted state.
- **Queuing is fire-and-forget for processing**: Callers cannot `await` the actual sync — only the enqueue. UI must reflect `SyncStatus.Pending` explicitly.

## When Adding Code

- New syncable entity type → add a constant to `SyncEntityTypes`, implement `ISyncHandler`, register with `AddTransient<ISyncHandler, YourHandler>()`. Keep `CanHandle` precise.
- New sync payload → version it or read defensively; old queued rows may have the previous shape.
- Never call `ProcessQueueAsync` from inside a handler — you will recurse and deadlock the processing lock.
