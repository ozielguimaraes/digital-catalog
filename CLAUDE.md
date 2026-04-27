# Digital Catalog (MeuCatalogo)

Multi-tenant digital catalog platform. A user (catalog owner) creates catalogs containing categories and products, manages images/stock/pricing, and exposes them publicly for end customers to browse. Three clients share one backend.

## Monorepo Layout

- **`backend/`** — ASP.NET Core 6 API + Application layers. JWT auth, EF Core, Azure Blob / local storage, Serilog + Sentry. See `backend/CLAUDE.md`.
- **`frontend/`** — Angular 20 storefront + admin dashboard. Tailwind v4, mixed standalone/NgModule, BehaviorSubject state. See `frontend/CLAUDE.md`.
- **`mobile/`** — .NET MAUI cross-platform app. Clean architecture + MVVM, offline-first for catalogs and products, SQLite local store. See `mobile/CLAUDE.md`.
- **`tests/`** — k6 load tests against the deployed API. See `tests/CLAUDE.md`.

Other root files:
- `project_rules.md` — project-level conventions (read alongside this file).
- `offline-approach.txt` — design document for the mobile offline-first architecture.
- `.editorconfig`, `.gitignore` — repo-level config.

## Domain Model (shared vocabulary)

- **`Catalogo`** — a storefront owned by one user (`UserId`). Has whatsapp/email contact fields.
- **`Categoria`** — belongs to a `Catalogo`.
- **`Produto`** — belongs to a `Catalogo` and a `Categoria`. Has `Preco`, optional `PrecoComDesconto`, `InformacoesAdicionais` (HTML), and 0+ `ProdutoImagem`.
- **`ProdutoImagem`** — one row per image. `BasePath` is a path stem; thumbnail/medium/full URLs are derived by convention.
- **`Estoque`** — 1-to-1 with `Produto`; optional (a product can have no stock row).
- **`Variacao` / `TipoVariacao` / `OpcaoVariacao`** — product variations (color × size × …).
- **`Cliente` / `Pedido` / `ItemPedido`** — orders domain.
- **`PlanoAssinatura` / `AssinaturaUsuario`** — subscription plans gate feature limits (`LimiteProdutos`, `LimiteCatalogos`, `PermiteVariacoes`, etc.). Enforced via `ApplicationUser.PodeAdicionarProduto()` / `PodeAdicionarCatalogo()`.
- **`ApplicationUser`** — Identity user with `Catalogos` and `Assinaturas` navigations.

## Universal Contracts

- **`ApiResponse<T>`** is the wire envelope every endpoint returns: `{ isSuccess, data, message, type, errors }`. The `type` enum drives HTTP status (`Success`/`Created`/`Deleted`/`Validation`/`NotFound`/`Forbidden`). All three clients parse this shape.
- **`ProblemDetails`** (RFC 7807) is the error shape for unhandled exceptions — produced by `ExceptionHandlingMiddleware`. Angular's `extractErrorMessage` and mobile's `BaseApiService` both accept it.
- **JWT** access tokens are 15 minutes; refresh tokens are 30 days (stored server-side in `RefreshToken` table). Mobile's `AuthenticationHandler` auto-refreshes on 401; Angular's `auth.interceptor` does the same.
- **Images** come back as relative storage paths in DTOs; the API's `ProdutosController` enriches them into absolute URLs with `-thumb`, `-medium`, `-full` variants at response time.
- **UTC `DateTime`** on the wire (enforced by `UtcDateTimeConverter`); the Angular `DateConverterInterceptor` rehydrates them to JS `Date`.

## Cross-Cutting Decisions

- **No repository interface layer on the backend** — services use `ApplicationDbContext` directly; query helpers are extension methods.
- **Manual mapping everywhere** — no AutoMapper on backend, no store/selector pattern on frontend.
- **Offline-first on mobile only** — `Catalogo` and `Produto` go through `ISyncEngine`; `Categoria` and `Estoque` are online-only.
- **Migrations in `MeuCatalogo.API`**, not `MeuCatalogo.Application` — run EF commands from the API project.
- **Subscription limits live on `ApplicationUser`**, not on the plan entity — any new "can do X" gate belongs there.
- **Caching** on the backend via `IMemoryCache` with 20s TTL and manual invalidation on writes.

## Project Rules (from `project_rules.md` and global `CLAUDE.md`)

- Responda em pt-BR ao usuário; código e comentários podem seguir o padrão do projeto (maior parte em inglês/pt misturado).
- Não invente classes/métodos/endpoints inexistentes.
- Não refatore código que funciona sem pedido explícito.
- Comentários apenas quando estritamente necessários.
- Rode testes antes e depois de qualquer mudança; nada é "pronto" sem testes passando.
- **Commits/PRs: nunca incluir trailer `Co-Authored-By: Claude…` nem `🤖 Generated with Claude Code`.** A mensagem termina na última linha do conteúdo real.

## When Adding Code

- Pick the right tier: backend (entity + service + DTO + controller) vs frontend (page + service) vs mobile (feature folder with Data/Domain/UseCases/Presentation).
- Keep `ApiResponse<T>` shape consistent — every backend endpoint must return it; every client assumes it.
- New entity touching the database → migration in `backend/MeuCatalogo.API` AND SQLite schema update + `EnsureColumnExistsAsync` in `mobile/MeuCatalogo/Infrastructure/Database/AppDbContext.cs`.
- Keep the subdirectory CLAUDE.md files up to date when their area changes substantially.
