# API Services

API-layer services that cannot live in `MeuCatalogo.Application` because they depend on ASP.NET Core types (HttpContext, Identity, JWT). Business services live in `../../MeuCatalogo.Application/Services`.

## Files

- **`RefreshTokenService.cs`**: Implements `IRefreshTokenService`. Generates/validates refresh tokens (30-day expiry) backed by the `RefreshToken` entity. Reuses an existing valid token on request instead of issuing a new one. Registered as `Scoped`.

## Patterns

- Services here depend on `UserManager<ApplicationUser>`, `SignInManager`, `IConfiguration` for JWT secrets — things the Application layer must not know about.
- Async-only, `Task`-returning.
- Return `ApiResponse<T>` so controllers can pipe through `HandleApiResponse()`.

## Integration

- Registered in `Program.cs` under `builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>()` (this override takes precedence over any Application-level registration).
- Consumed by `AuthController` during login/refresh flows.

## Gotchas

- **Dual registration risk**: An `IRefreshTokenService` may also exist in the Application layer. The API-layer registration must win — check `Program.cs` if refresh tokens behave unexpectedly.
- **Reuse-not-recreate** rule: On refresh, existing non-expired tokens are returned rather than generating a new row. Don't refactor this into "always create" without auditing the entity's unique-token constraint.

## When Adding Code

- If the new service can run without ASP.NET Core dependencies, put it in `MeuCatalogo.Application/Services` instead.
- Use `UserManager` / `SignInManager` only here.
