# Controllers

ASP.NET Core controllers. Thin HTTP layer over `MeuCatalogo.Application` services. All routes lowercase via `LowercaseDocumentFilter` + `options.LowercaseUrls = true` in `Program.cs`.

## Files

- **`BaseApiController.cs`**: Abstract base. Exposes `OkResponse<T>()`, `CreatedResponse<T>()`, `HandleApiResponse<T>(ApiResponse<T>)`, `ValidationProblemResponse()`, `BadRequestResponse()`, `NotFoundResponse()`, `ProblemResponse()`. All controllers inherit.
- **`AuthController.cs`**: Register, Login, RefreshToken, Logout, ConfirmEmail, GetCurrentUser, ForgotPassword, ResetPassword. Mixed auth — register/login are `[AllowAnonymous]`, rest `[Authorize]`.
- **`CatalogosController.cs`**: Catalog CRUD. `Obter()` is `[AllowAnonymous]` (public storefront); `ObterMeus()` and CRUD require auth.
- **`CategoriasController.cs`**: Category CRUD scoped to catalog. Full `[Authorize]`.
- **`ProdutosController.cs`**: Product CRUD + image upload + stock update. `ObterPorCatalogo()` is public; rest auth-gated. Private `EnriquecerImagens()` / `EnriquecerProduto()` rewrite relative storage paths into absolute URLs with thumbnail/medium/full variants before serializing.
- **`PlanoAssinaturaController.cs`**: Subscription plan listing. Public.

## Patterns

- **Response wrapping**: Service calls return `ApiResponse<T>`. Controllers always finish with `return HandleApiResponse(result)` — do not hand-craft `Ok()` / `BadRequest()` from service results.
- **Validation**: Check `ModelState.IsValid` first → `ValidationProblemResponse()` on fail. Business validation happens inside services, not here.
- **Auth**: JWT Bearer; `[Authorize]` at class level, `[AllowAnonymous]` overrides per-endpoint. UserId is extracted from claims (`User.FindFirstValue(ClaimTypes.NameIdentifier)`) and passed to services explicitly — services do not peek at `HttpContext`.
- **Swagger**: `[SwaggerTag]`, `[SwaggerOperation]`, `[SwaggerResponse]` on each public action.
- **Logging**: `ILogger<T>` injected. DEBUG for success hot paths, INFO for user actions, WARN for failed auth, ERROR only via middleware.
- **Routing**: `[Route("api/v{version:apiVersion}/[controller]")]` — route token lowercased globally + URL-segment versioning via `Asp.Versioning`. `CatalogosController` (default `[ApiVersion("1.0")]` from `BaseApiController`) → `/api/v1/catalogos`.

## Integration

- Imports from: `MeuCatalogo.Application.Interfaces`, `MeuCatalogo.Application.DTOs`
- Exceptions bubble up to `ExceptionHandlingMiddleware` (never try/catch here)
- Image uploads go `ProdutosController.UploadImage` → `IProdutoService.UploadImagemAsync` → `IStorageService`; the controller re-enriches response URLs before returning

## Gotchas

- **Never return service `ApiResponse` raw** — always go through `HandleApiResponse()` so status codes map correctly (`ResponseType.Forbidden` → 403, `Validation` → 400, `NotFound` → 404, `Created` → 201).
- **Image URLs in responses are rewritten by the controller**, not the service. If you add a new endpoint that returns `ProdutoDto`/`ProdutoResponse`, call `EnriquecerProduto()` before responding or images will be relative paths.
- **Route casing**: Do not use `[Route("api/Produtos")]` with mixed case — filter will lowercase the Swagger doc but not route matching consistency; stick to `[controller]` token.
- **API versioning**: Default `[ApiVersion("1.0")]` is declared on `BaseApiController` and inherited. To introduce a v2 endpoint, add `[ApiVersion("2.0")]` to the controller (or a sibling) and the route segment `v{version:apiVersion}` will resolve it.
- **UserId flows as a parameter**, never read from `HttpContext` inside services. If adding a new authed endpoint, extract the claim at the controller.
- `CorrelationIdMiddleware` puts `X-Correlation-ID` into Serilog `LogContext`; you don't need to log it manually.

## When Adding Code

- Inherit `BaseApiController`.
- Match existing naming: `Obter*` (GET), `Adicionar*` / `Criar*` (POST), `Atualizar*` (PUT), `Remover*` (DELETE).
- Add `[Authorize]` at class level; annotate exceptions with `[AllowAnonymous]`.
- Register the backing service as `Scoped` in `Program.cs`.
- Don't introduce AutoMapper — project uses manual mappers in `Application/Infrastructure/Mappers`.
