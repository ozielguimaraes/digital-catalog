# Database

SQLite via `sqlite-net-pcl` (NOT EF Core). Async wrapper with lazy initialization. All local entities inherit `Core/Base/BaseEntity`.

## Files

- **`AppDbContext.cs`**: Lazy singleton around `SQLiteAsyncConnection`. `InitializeAsync()` runs once, gated by `SemaphoreSlim(1,1)` (double-check). Configures PRAGMAs (WAL journal mode, ForeignKeys ON, SharedCache), creates tables (`SyncQueue`, `UserEntity`, `CatalogoEntity`, `ProdutoEntity`, `ProdutoImagemEntity`), creates indexes on `ProdutoEntity.CatalogoId` and `ProdutoEntity.CategoriaId`, and runs in-line migrations via `EnsureColumnExistsAsync(...)` (idempotent `ALTER TABLE`).
- **`BaseRepository.cs`**: Abstract `IRepository<T>` implementation. Every method calls `_context.InitializeAsync()` first — cheap after the first call, safe under concurrency.
- **`DatabaseConstants.cs`**: DB path (`AppDataDirectory/meucatalogo.db3`) and SQLite open flags.

## Patterns

- **Lazy init on every call**: Repositories call `await _context.InitializeAsync()` at the top of each method. The semaphore makes this a no-op after the first run; do not try to skip it for "performance".
- **Migrations as column adds**: `EnsureColumnExistsAsync(table, column, sqlType)` issues `ALTER TABLE ADD COLUMN` only when the column is missing. Adequate for additive schema changes; drops or renames are not supported — plan a data migration or DB version bump.
- **Transactions are rare**: Most writes are single-statement. The one place that uses `RunInTransactionAsync` is `Features/Produto/Data/Sync/ProdutoUpsertSyncHandler` (ID remapping).
- **No EF Core**: Don't `using Microsoft.EntityFrameworkCore`. Queries are `Table<T>().Where(...)` or raw SQL via `QueryAsync<T>(sql, args)`.

## Integration

- `AppDbContext` is registered as **Singleton** in `ServiceCollectionExtension`.
- Consumed by every `*LocalRepository` and `*LocalDataSource` in features.
- Lifecycle trigger: `App.xaml.cs.OnHandlerChanged()` calls `InitializeAsync()` early and registers the connectivity listener.

## Gotchas

- **Singleton + async init race**: The semaphore is what makes this safe. Do not bypass it by calling private methods directly or removing the double-check.
- **Lazy init adds overhead to cold start** but not to subsequent calls — the first call does real work, the rest are semaphore-fast-path returns.
- **Schema migrations via `ALTER TABLE ADD COLUMN` only**: You cannot drop or rename columns here. For destructive changes, add a new column, migrate data, and stop reading the old column (or bump the DB filename).
- **Foreign keys are ON** — deletes may cascade or block depending on the table definition; check the `CREATE TABLE` in `InitializeAsync` before assuming a `DELETE` succeeds.
- **WAL journal mode** improves write concurrency but leaves `-wal` and `-shm` sidecar files next to the DB. Backups must include all three.
- **SyncQueue has no TTL**: Failed items sit forever unless cleared manually. Consider this when debugging "sync seems stuck".

## When Adding Code

- New entity → inherit `BaseEntity`, add the `CREATE TABLE` statement in `InitializeAsync`, add any indexes, add migrations (`EnsureColumnExistsAsync`) if the entity evolved post-release.
- New column on an existing entity → add it to the entity class AND add an `EnsureColumnExistsAsync` call so existing installs get the column.
- New repository → inherit `BaseRepository<T>`; don't reimplement `InitializeAsync` calling.
