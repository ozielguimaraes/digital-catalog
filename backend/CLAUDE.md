# Backend

ASP.NET Core 6 backend. Two projects:

- **`MeuCatalogo.API/`** — host, HTTP surface, middlewares, JWT auth, Swagger, Serilog + Sentry, and EF Core migrations. See `MeuCatalogo.API/CLAUDE.md`.
- **`MeuCatalogo.Application/`** — entities, business services, DTOs, mappers, DbContext, storage adapters. No ASP.NET Core dependency. See `MeuCatalogo.Application/CLAUDE.md`.

## Solution

- `MeuCatalogo.sln` wires both projects.
- `.pipelines/` holds CI/CD configuration.

## Overall Architecture

```
HTTP request
  → Middlewares (correlation id, exception handling, problem details)
  → Controllers (BaseApiController → HandleApiResponse)
  → Application Services (return ApiResponse<T>)
  → ApplicationDbContext (direct access + extension-method query helpers)
  → EF Core + SQL Server / Azure SQL
```

## Key Decisions

- **Universal envelope `ApiResponse<T>`** — every service returns it; controllers map `Type` discriminator to HTTP status.
- **No repository pattern** — services call DbContext directly; grouped query helpers are extension methods.
- **Manual mapping** — no AutoMapper. Mappers in `MeuCatalogo.Application/Infrastructure/Mappers/`.
- **Migrations live in the API project** (not Application). Run `dotnet ef migrations add ... --project MeuCatalogo.API`.
- **Storage is pluggable**: `AzureBlobStorageService` in production (when connection string present), `LocalFileStorageService` in dev.
- **JWT 15-minute access tokens** + separate refresh-token table (30-day).
- **Caching** via `IMemoryCache` with 20s TTL on list reads; mutations must evict keys manually.
- **Logging** via Serilog → rolling files + Sentry.
- **UTC-only** `DateTime` on the wire (enforced by `UtcDateTimeConverter`).

## Integration

- Consumed by `../frontend/` (Angular admin + storefront) and `../mobile/` (MAUI).
- Contract shape mirrored on clients — the `ApiResponse` envelope and `ProblemDetails` errors are parsed by Angular (`extractErrorMessage`) and mobile (`BaseApiService`).

## Gotchas

- **Do not introduce repository interfaces** — use extension methods in `MeuCatalogo.Application/Infrastructure/Data/`.
- **Do not introduce AutoMapper** — the codebase is manual-mapper-first.
- **Refresh token service is registered in the API project** (uses Identity); the copy in Application is shadowed.
- **Sentry DSN is hardcoded** in `MeuCatalogo.API/Program.cs`.
- **Migrations command must target the API project** even though entities live in Application.

## When Adding Code

- New endpoint → controller in API, service + interface in Application, DTO pair in `Application/DTOs/Requests` and `Application/DTOs/Responses`, mapper in `Application/Infrastructure/Mappers`, register Scoped in `Program.cs`.
- New entity → class in `Application/Entities`, configuration in `Application/Infrastructure/Data/Configurations`, `DbSet<T>` in `ApplicationDbContext`, migration from the API project.
