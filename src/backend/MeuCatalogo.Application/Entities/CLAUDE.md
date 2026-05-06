# Entities

EF Core domain entities. Every non-identity entity inherits `BaseEntity`. Multi-tenancy is via `UserId` on `Catalogo`; every other aggregate traces back through `Catalogo`.

## Files

- **`BaseEntity.cs`**: `Id` (Guid), `DataCriacao` (UTC), `DataAtualizacao?`, `Ativo` (bool soft-delete flag).
- **`ApplicationUser.cs`**: Extends `IdentityUser`. Nav: `Catalogos`, `Assinaturas`. Methods: `ObterAssinaturaAtiva()`, `TemPlanoAtivo()`, `PodeAdicionarProduto()`, `PodeAdicionarCatalogo()` — subscription gate logic lives here.
- **`Catalogo.cs`**: `Nome`, `Descricao`, `NomeCurto`, `NumeroWhatsapp`, `Email`, `UserId` (FK → `ApplicationUser`). Nav: `Produtos`.
- **`Categoria.cs`**: `Nome`, `Descricao`, `CatalogoId`. Nav: `Produtos`.
- **`Produto.cs`**: `Nome`, `CategoriaId`, `CatalogoId`, `Preco`, `PrecoComDesconto?`, `InformacoesAdicionais?`. Nav: `Estoque` (1-1), `Variacoes`, `ItensPedido`, `Imagens`. `ObterPrecoUnitario()` returns discount price when set, otherwise full.
- **`ProdutoImagem.cs`**: `BasePath`, `IsPrincipal`, `Ordem`. One row per image; thumbnail/medium/full are derived by convention from `BasePath`, not separate columns.
- **`Estoque.cs`**: `ProdutoId` (FK 1-1), `Quantidade`, `QuantidadeMinima`, `QuantidadeMaxima`.
- **`Variacao.cs` / `TipoVariacao.cs` / `OpcaoVariacao.cs`**: Many-to-many product variation (color × size × etc.).
- **`Cliente.cs`**: Order-domain customer. `Nome`, `Email`, `Telefone`, `CPF`, `EnderecoId?`.
- **`Pedido.cs` / `ItemPedido.cs`**: Orders and line items.
- **`PlanoAssinatura.cs`**: Subscription plan definition — `LimiteProdutos`, `LimiteCatalogos`, `PermiteVariacoes`, `PermiteEstoque`, `PermiteRelatorios`, etc.
- **`AssinaturaUsuario.cs`**: Per-user plan assignment with `DataInicio` / `DataFim`. `EstaAtiva()` checks expiry.
- **`RefreshToken.cs`**: `UserId`, `Token` (unique), `ExpiresAt`, `IsRevoked`.

## Patterns

- **UTC-only timestamps**: `DataCriacao = DateTime.UtcNow`. The `UtcDateTimeConverter` in the API forces UTC on serialization; entities must match.
- **Soft delete via `Ativo`**: Many queries filter `Where(x => x.Ativo)`. New queries should match this convention unless you truly want deleted rows.
- **Fluent configuration**: Relationships, indexes, string-length caps live in `../Infrastructure/Data/Configurations/*Configuration.cs`, not in annotations here.
- **String defaults**: `OnModelCreating` caps string columns at 100 chars unless a configuration overrides.

## Integration

- DbContext: `../Infrastructure/Data/ApplicationDbContext.cs` exposes `DbSet<T>` for all of these.
- Migrations assembly: `MeuCatalogo.API` (not this project) — see its `Migrations/` folder.
- Mapped to DTOs by the manual mappers in `../Infrastructure/Mappers`.

## Gotchas

- **`ProdutoImagem.BasePath` is a path stem**, not a URL. The API's `ProdutosController.EnriquecerImagens()` rewrites it into absolute URLs with `-thumb`, `-medium`, `-full` WebP suffixes at response time.
- **Subscription limits live on `ApplicationUser`**, not on `AssinaturaUsuario`. When adding a new "can do X" rule, add a method on `ApplicationUser` that walks `ObterAssinaturaAtiva()`.
- **`Estoque` is 1-to-1 with `Produto`** but is not auto-created — a product can exist without a stock row. Services must check for null.
- **Adding a string property >100 chars?** Override in the Configuration class — `OnModelCreating` caps at 100 globally.

## When Adding Code

- Inherit `BaseEntity`.
- Add a matching `*Configuration` in `../Infrastructure/Data/Configurations/`.
- Add a `DbSet<T>` in `ApplicationDbContext`.
- Add a migration from the `MeuCatalogo.API` project.
- If the new entity is user-scoped, route it through `Catalogo.UserId` rather than inventing a new `UserId` column.
