# Application Infrastructure

Persistence, mapping, and external-resource adapters for the Application layer.

## Submodules

- **`Data/`**:
  - `ApplicationDbContext.cs` — inherits `IdentityDbContext<ApplicationUser>`. DbSets for every entity in `../Entities`. `OnModelCreating` applies configurations by assembly scan and caps unconstrained string columns at 100 chars.
  - `DbContextExtensions.cs` — generic helpers (`AdicionarAsync<T>`, `AtualizarAsync<T>`) plus entity-specific query helpers (`ObterCatalogoComProdutosAsync`, `GetProdutoWithDetailsAsync`, etc.).
  - `Configurations/` — one `IEntityTypeConfiguration<T>` per entity. Relationships, indexes, `OnDelete` behavior, explicit string lengths.
  - `Repository/` — per-entity extension classes (`ProdutoRepositoryExtensions`, `CatalogoRepositoryExtensions`, ...). These are **not** repository classes — just grouped query helpers.
  - `DbInitializer.cs` — dev-only seeding; invoked from `Program.cs` when `env.IsDevelopment()`.
- **`Mappers/`**:
  - `CatalogoMapper.cs`, `PedidoMapper.cs` — hand-written entity → DTO mappers. Static methods.
- **`Storage/`**:
  - `IStorageService` — `UploadAsync(blobPath, stream, contentType)`, `DeletePrefixAsync(prefix)`, `GetBlobUrl(blobPath)`, `GetPresignedUrlFromPublicUrl(publicUrl, expiration)`.
  - `AzureBlobStorageService.cs` — Azure SDK; reads connection string from config; presigned URLs use SAS tokens.
  - `LocalFileStorageService.cs` — writes to `Uploads/` under the API app root; returns relative `/uploads/...` URLs. Presigned URLs no-op.

## Patterns

- **No repository abstraction**: Services call `_context.Produtos.Where(...).ToListAsync()` directly. The `*RepositoryExtensions` classes provide named query helpers, not an `IRepository<T>` seam.
- **Storage selection at DI time**: `Program.cs` registers `IStorageService` as `Singleton` — `AzureBlobStorageService` when a connection string is present, otherwise `LocalFileStorageService`. Same consumer contract.
- **Manual mapping only** — no AutoMapper, no Mapster.
- **Split query behavior** is on by default (configured in `AddDbContextPool`) for navigation-heavy queries.

## Integration

- Consumed by: `../Services/*` — services own `ApplicationDbContext`, `IStorageService`, `IMemoryCache`.
- Migrations assembly is `MeuCatalogo.API` (set in `AddDbContextPool` options). Migrations live there, not here.
- Connection string is `DefaultConnection` from `appsettings.*.json`.

## Gotchas

- **Migrations live in the API project**, not here. Run `dotnet ef migrations add <Name> --project MeuCatalogo.API` against the API project, not this one.
- **Local storage returns relative URLs** — they become absolute only after `ProdutosController.EnriquecerImagens()`. Don't call `GetBlobUrl` expecting absolute when running against local storage.
- **`DeletePrefixAsync` in local storage** walks the filesystem; in Azure it scans blobs with the prefix. Behavior is consistent but performance differs.
- **`DbInitializer` runs only in Development** — production databases rely on `context.Database.MigrateAsync()` at startup.
- **Retry is built into the connection** (3x, 2s delay via `EnableRetryOnFailure`) — transient failures are already handled; don't wrap service calls in additional retry logic.
- **Presigned URLs**: Local storage returns the public URL unchanged. Code must not assume the URL is different from the input.

## When Adding Code

- New entity → add DbSet + Configuration + migration (from the API project).
- New query helper → extension method in the per-entity `*RepositoryExtensions` or on `DbContextExtensions`.
- New mapper → static class in `Mappers/`, follow the `*Mapper.ToDto(entity)` naming.
- New storage backend → implement `IStorageService`, switch registration in `Program.cs`.
