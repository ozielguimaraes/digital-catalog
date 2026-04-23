# MeuCatalogo.API

ASP.NET Core 8 host project. Owns the HTTP surface, middleware pipeline, JWT auth, Swagger, Serilog + Sentry, and EF Core migrations. Business logic lives in `../MeuCatalogo.Application`.

## Entry Point

- **`Program.cs`** is the full composition root:
  - Serilog + Sentry (DSN hardcoded; trace sampling `1.0` dev / `0.1` prod; rolling file sinks under `logs/`).
  - `AddDbContextPool<ApplicationDbContext>` with retry (3×, 2s) + split-query; migrations assembly is **this project** (`MeuCatalogo.API`).
  - `AddIdentity<ApplicationUser, IdentityRole>` with password policy (6+, upper, lower, digit).
  - JWT Bearer — 15-minute access token, `ClockSkew = Zero`.
  - Scoped service registrations for every `I*Service` in `../MeuCatalogo.Application/Interfaces`.
  - `IRefreshTokenService` is overridden to the API-layer implementation (uses Identity types).
  - `IStorageService` singleton — `AzureBlobStorageService` when `AzureStorage:ConnectionString` is present, else `LocalFileStorageService`.
  - `EmailSender` transient; falls back to a dummy sender if `EmailSettings` is missing.
  - CORS policy `AllowAngularApp`; dev allows any origin.
  - Swagger mounted at `/swagger`, redirect from `/`.
  - Startup: `context.Database.MigrateAsync()`; in Development also runs `DbInitializer.InitializeAsync()` for seed data.

## Submodules

- **`Controllers/`** — HTTP endpoints. See `Controllers/CLAUDE.md`.
- **`Services/`** — API-layer services (things needing Identity/JWT). See `Services/CLAUDE.md`.
- **`Middlewares/`** — `CorrelationIdMiddleware`, `ExceptionHandlingMiddleware`, `ProblemDetailsStatusCodeMiddleware`. See `Middlewares/CLAUDE.md`.
- **`Filters/`** — Swagger `LowercaseDocumentFilter`. See `Filters/CLAUDE.md`.
- **`Infrastructure/`** — SMTP email sender. See `Infrastructure/CLAUDE.md`.
- **`Converters/UtcDateTimeConverter.cs`** — JSON `DateTime` converter that always reads/writes UTC ISO 8601.
- **`Migrations/`** — EF Core migrations (auto-generated). This is the migrations assembly, not Application.
- **`Uploads/`** — runtime upload target when using `LocalFileStorageService`. Served at `/uploads/*` via `UseStaticFiles`.
- **`logs/`** — Serilog rolling file output.

## Pipeline Order

```
Sentry tracing
→ ForwardedHeaders (X-Forwarded-For/Proto/Host)
→ StaticFiles (maps Uploads/ → /uploads)
→ HTTPS redirect
→ CORS (AllowAngularApp)
→ CorrelationIdMiddleware
→ ExceptionHandlingMiddleware
→ ProblemDetailsStatusCodeMiddleware
→ Authentication / Authorization
→ Health check endpoint /health
→ Controllers
```

`CorrelationIdMiddleware` must precede `ExceptionHandlingMiddleware` so error logs carry the request ID.

## Gotchas

- **Migrations live HERE**, not in `MeuCatalogo.Application`. Run `dotnet ef migrations add <Name> --project MeuCatalogo.API --startup-project MeuCatalogo.API` with the DbContext pointed at the Application assembly.
- **CORS in dev is permissive** (`SetIsOriginAllowed(_ => true)`). Tighten for production deployments.
- **Refresh token service has two registrations**; the API-layer one wins. Check `Program.cs` if refresh flows misbehave.
- **Sentry DSN is hardcoded** in `Program.cs` — rotating it requires a code change (not a config edit).
- **Serilog writes to `logs/`** relative to the app root. Ensure the directory is writable by the process owner.
- **JWT `ClockSkew = Zero`** means strict expiry enforcement — clock drift between client and server will cause immediate token rejections.
- **Static files at `/uploads`** is tied to `LocalFileStorageService`. On Azure storage, paths are absolute blob URLs and this static file serving is unused.

## When Adding Code

- New service → register in `Program.cs` Scoped, interface in `../MeuCatalogo.Application/Interfaces`, implementation in `../MeuCatalogo.Application/Services` (unless it needs ASP.NET Core types, in which case `./Services`).
- New middleware → register between `CorrelationIdMiddleware` and `Authentication`.
- New migration → add from this project as noted above.
- New config section → add a `*Settings.cs` binder and `builder.Services.Configure<XSettings>(builder.Configuration.GetSection("X"))`.
