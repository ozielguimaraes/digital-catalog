# MeuCatalogo.Application

Business/domain project. Holds entities, services, DTOs, mappers, persistence configuration, and storage abstractions. Has no dependency on ASP.NET Core — the host (`../MeuCatalogo.API`) composes everything.

## Submodules

- **`Entities/`** — EF Core domain entities, all inheriting `BaseEntity`. See `Entities/CLAUDE.md`.
- **`Services/`** — Sealed business services returning `ApiResponse<T>`. Direct DbContext access; no repository abstraction. See `Services/CLAUDE.md`.
- **`Infrastructure/`** — DbContext, entity configurations, query-helper extensions, manual mappers, storage implementations. See `Infrastructure/CLAUDE.md`.
- **`DTOs/`** — Input/output contracts.
  - `Requests/` — API inputs (`*Request.cs`, `*CreateDto`, `*UpdateDto`).
  - `Responses/` — API outputs (`*Response.cs`).
  - Top-level — generic DTOs (`ProdutoDto`, `ImageDto`, `ImageLinksDto`, `UserDto`, `PlanoAssinaturaDto`, etc.).
  - `ApiResponse<T>` — the universal envelope (fields: `IsSuccess`, `Data`, `Message`, `Type` enum, `Errors`). Returned by every service method.
- **`Interfaces/`** — Service contracts (`ICatalogoService`, `IProdutoService`, `ICategoriaService`, `IStorageService`, etc.). Implementations live in `Services/` and `Infrastructure/Storage/`.

## Patterns

- **`ApiResponse<T>` everywhere**: Every service method returns `ApiResponse<T>`; controllers unwrap via `HandleApiResponse()`. The `Type` enum (`Success` / `Created` / `Deleted` / `Validation` / `NotFound` / `Forbidden`) drives HTTP status mapping.
- **No repository interface**: Services talk to `ApplicationDbContext` directly. Query helpers are extension methods (`Infrastructure/Data/DbContextExtensions.cs` and per-entity `*RepositoryExtensions.cs`).
- **Manual mapping**: Static mappers in `Infrastructure/Mappers/`. No AutoMapper.
- **Caching via `IMemoryCache`**: 20-second TTL on list reads (catalogs, products, categories). Mutations must evict matching keys manually.
- **UTC-only `DateTime`**: Entities use `DateTime.UtcNow`; serialization is forced UTC by the API-layer `UtcDateTimeConverter`.
- **Subscription gating**: `ApplicationUser.PodeAdicionarProduto()` / `PodeAdicionarCatalogo()` gate write paths.

## Integration

- Consumed by: `../MeuCatalogo.API` — registers every `I*Service` as Scoped in `Program.cs`.
- Migrations live in **`../MeuCatalogo.API/Migrations/`** (the migrations assembly is configured as `MeuCatalogo.API`), not here.
- Shared contracts: the mobile and Angular clients mirror the DTO shapes (request/response envelopes).

## Gotchas

- **`ApiResponse.Type` is the source of truth for HTTP status** — don't return `null` data with `IsSuccess = true` expecting a 204; use the appropriate type discriminator.
- **No repository pattern** — do not introduce `IProdutoRepository` etc. Extend `DbContextExtensions` or the per-entity extension classes.
- **Cache invalidation is manual** — new write paths on catalogs/products/categories must `cache.Remove(...)` matching keys.
- **DTOs are duplicated by role** (`*Request`, `*Response`, `*Dto`). Don't collapse them into one class — request and response shapes differ in validation attributes and nullability.
- **`IRefreshTokenService` is registered in API layer** — the copy here is shadowed at DI time.
- **Image paths in DTOs are relative storage paths**; the API controller enriches them before responding. Don't assume DTOs carry absolute URLs.

## When Adding Code

- New entity → see `Entities/CLAUDE.md`. Remember to add migrations from the API project.
- New service → interface in `Interfaces/`, implementation in `Services/`, register `Scoped` in `../MeuCatalogo.API/Program.cs`.
- New DTO → pick the right subfolder (`Requests/` or `Responses/`), don't reuse entities as DTOs.
- New storage target → implement `IStorageService` under `Infrastructure/Storage/`.
