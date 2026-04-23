# Filters

Swagger document filters. Not MVC action/exception filters.

## Files

- **`LowercaseDocumentFilter.cs`**: `IDocumentFilter` that rewrites every path in the generated OpenAPI document to lowercase. Pairs with `builder.Services.AddRouting(options => options.LowercaseUrls = true)` so Swagger UI always matches routing behavior.

## Gotchas

- If you add uppercase literal segments to a `[Route(...)]` attribute, routing lowercases them but this filter also normalizes the Swagger doc — clients generated from the OpenAPI spec will expect lowercase paths.
- This is a Swagger filter, not an `IActionFilter` — do not add request/response inspection logic here.
