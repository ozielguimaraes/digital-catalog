# Application Services

Core business logic. All services are `sealed`, async, and return `ApiResponse<T>`. They access `ApplicationDbContext` directly — there is no repository interface layer; instead helpers live as extension methods in `Infrastructure/Data/Repository/` and `Infrastructure/Data/DbContextExtensions.cs`.

## Files

- **`CatalogoService.cs`** (`ICatalogoService`): `ObterTodosPublicosAsync`, `ObterPorUsuarioIdAsync` (cached 20s), `ObterPorIdAsync`, `ObterCatalogoIdAsync`, full CRUD.
- **`ProdutoService.cs`** (`IProdutoService`): `ObterPorCatalogoIdPublicoAsync` (cached 20s), `ObterPorIdAsync`, `AdicionarAsync`, `AtualizarAsync`, `RemoverAsync`, `AtualizarEstoqueAsync`, `UploadImagemAsync`. Enforces subscription limits via `user.PodeAdicionarProduto()`. Image upload resizes with SixLabors.ImageSharp into three WebP variants (thumb, medium, full) and stores via `IStorageService`.
- **`CategoriaService.cs`** (`ICategoriaService`): CRUD + `ObterPorCatalogoAsync` (cached 20s).
- **`PlanoAssinaturaService.cs`** (`IPlanoAssinaturaService`): `ListarAsync`, `ObterPorIdAsync`, `AtribuirPlanoGratuitoAsync` (invoked on user register).
- **`ClienteService.cs`** (`IClienteService`): CRUD for the orders-domain customers.
- **`PedidoService.cs`** (`IPedidoService`): CRUD for orders.
- **`RefreshTokenService.cs`** (`IRefreshTokenService`): `Generate`, `Refresh`, `Revoke`, `RevokeAll`. See the API-layer override in `../../MeuCatalogo.API/Services` — the API-level registration wins.

## Patterns

- **ApiResponse discriminator**: Return `ApiResponse<T>.Success(...)`, `.Error(...)`, `.NotFound(...)`, `.Forbidden(...)`, `.Validation(...)`, `.Created(...)`, `.Deleted(...)`. The `Type` enum drives HTTP status mapping at the controller.
- **Authorization inside the service**: Every mutating method takes `userId` explicitly and returns `Forbidden` when the owning entity doesn't belong to that user. Services never touch `HttpContext`.
- **Caching**: `IMemoryCache` with string keys like `produtos:catalogo:{id}`, `categorias:catalogo:{id}`. TTL is 20s. Mutations invalidate by calling `cache.Remove(...)` — don't forget this on new write paths.
- **Async all the way**: Every DB call is async; no `.Result` / `.Wait()`.
- **Manual mapping**: Use the classes in `../Infrastructure/Mappers`. No AutoMapper.

## Integration

- Imports from: `Entities`, `DTOs`, `Infrastructure.Data` (DbContext + extensions), `Infrastructure.Mappers`, `Infrastructure.Storage`.
- Consumed by: `MeuCatalogo.API/Controllers/*`.
- `ProdutoService.UploadImagemAsync` flows: `IFormFile` → ImageSharp resize → `IStorageService.UploadAsync` → new `ProdutoImagem` row → `SaveChangesAsync`.

## Gotchas

- **No repository layer** — services call `_context.Produtos.Where(...)` directly. Don't introduce `IProdutoRepository`; extend `DbContextExtensions` or the per-entity `*RepositoryExtensions` classes instead.
- **Subscription limits** live on `ApplicationUser.PodeAdicionarProduto()` / `PodeAdicionarCatalogo()`. Any new write path that adds products/catalogs must gate on these.
- **Cache invalidation is manual**: if you add a new mutation, evict the matching cache keys or reads will serve stale data for 20s.
- **Double validation**: Controllers validate `ModelState`, but services also re-check business rules. Don't assume input is sane because the controller let it through.
- **`RefreshTokenService` duplication**: the real registration is in `MeuCatalogo.API/Services/RefreshTokenService.cs` — the copy here exists for reference but is shadowed by the API-layer Scoped registration.

## When Adding Code

- Inherit nothing — services are `sealed` classes with ctor DI.
- Register in `Program.cs` as `AddScoped<IX, XService>()`.
- Return `ApiResponse<T>`, never raw entities or plain tuples.
- Accept `userId` as an explicit parameter for any authenticated path.
- Evict relevant cache keys on writes.
