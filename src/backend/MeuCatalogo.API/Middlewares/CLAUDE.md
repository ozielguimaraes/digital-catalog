# Middlewares

Custom ASP.NET Core middlewares. Order is defined in `Program.cs` and matters.

## Files

- **`CorrelationIdMiddleware.cs`**: Reads `X-Correlation-ID` from the incoming request (creates a new GUID if absent), echoes it back on the response, and pushes it into `Serilog.Context.LogContext` so every log line inside the request carries it.
- **`ExceptionHandlingMiddleware.cs`**: Top-level try/catch. Maps exceptions to `ProblemDetails` (RFC 7807): `UnauthorizedAccessException` → 403, `ValidationException` → 400, anything else → 500. Logs to Serilog and captures in Sentry.
- **`ProblemDetailsStatusCodeMiddleware.cs`**: Normalizes non-success responses written without a body into `ProblemDetails` shape so clients always get consistent error payloads.

## Pipeline Order (from Program.cs)

```
Sentry → ForwardedHeaders → StaticFiles → HTTPS → CORS
→ CorrelationIdMiddleware
→ ExceptionHandlingMiddleware
→ ProblemDetailsStatusCodeMiddleware
→ Auth/AuthZ → Controllers
```

`CorrelationIdMiddleware` **must** run before `ExceptionHandlingMiddleware` so exception logs carry the correlation ID.

## Gotchas

- **Don't throw `HttpResponseException`-style types from controllers** — this middleware expects framework/business exceptions and maps them; unknown exception types all collapse to 500.
- **Sentry is wired via Serilog sink** — middleware doesn't call Sentry directly, it just logs. If Sentry stops receiving errors, check the Serilog → Sentry configuration in `Program.cs`, not this file.
- **ProblemDetails payload shape** is consumed by the Angular `extractErrorMessage()` util and mobile `BaseApiService` — changing the shape breaks both clients.

## When Adding Code

- Register new middlewares in `Program.cs`; keep them between `CorrelationIdMiddleware` and `Auth` unless you have a specific reason.
- Prefer extending `ExceptionHandlingMiddleware`'s switch over introducing new catch-all middlewares.
