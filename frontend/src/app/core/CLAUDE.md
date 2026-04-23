# Core

Cross-cutting singletons: auth, HTTP interceptors, guards, shared models, global services. Everything here uses `providedIn: 'root'`.

## Files

- **`guards/auth.guard.ts`**: `CanActivate` — blocks unauthenticated navigation via `AuthService.authState$`.
- **`guards/guest.guard.ts`**: `CanActivate` — redirects authenticated users to `/dashboard`. Used on `/signin`, `/signup`.
- **`interceptors/auth.interceptor.ts`**: Functional interceptor (`HttpInterceptorFn`). Injects `Authorization: Bearer <token>`. On 401, refreshes via `AuthService.refreshToken()` and retries the original request. Uses an `isRefreshing` flag + `refreshTokenSubject` to queue concurrent requests during refresh.
- **`interceptors/date-converter.interceptor.ts`**: Class-based (`HttpInterceptor`). Registered via `HTTP_INTERCEPTORS` multi-provider. Walks response bodies and converts ISO date strings to `Date` objects.
- **`models/user.model.ts`**: `User` interface — id, email, nome, etc.
- **`models/product.model.ts`**: `Product` and `ProdutoImagem` interfaces. **Images can be either a string (legacy) or `{ url, isPrincipal, ordem }` (current)** — code must handle both.
- **`models/api-response.model.ts`**: Mirrors the backend `ApiResponse<T>` envelope (`isSuccess`, `data`, `message`, `errors`, `type`).
- **`pipes/date-format.pipe.ts`**: Custom date formatting.
- **`services/auth.service.ts`**: Login/register/logout, token management. Exposes `authState$` (BehaviorSubject) and `getCurrentUserSync()`. Stores tokens in `localStorage` under `TOKEN_KEY`, `REFRESH_TOKEN_KEY`, `USER_KEY`.
- **`services/product.service.ts`**: Product CRUD + stock update.
- **`services/catalog.service.ts`**: Catalog CRUD.
- **`services/category.service.ts`**: Category operations scoped to a catalog.
- **`services/image-upload.service.ts`**: Multipart image upload.
- **`services/image-url.service.ts`**: Rewrites relative storage paths to absolute URLs and picks thumbnail/medium/full variants.
- **`services/theme.service.ts`**: Dark-mode toggle; writes to DOM + localStorage.
- **`services/seo.service.ts`**: Page-level meta tags.
- **`services/sitemap.service.ts`**: Sitemap generation helpers.
- **`services/sentry.service.ts`**: Sentry init + breadcrumb helpers.
- **`utils/error.utils.ts`**: `extractErrorMessage(err)` — pulls a display string from an `ApiResponse` error or raw `HttpErrorResponse`.
- **`utils/date.utils.ts`**: Date helpers.

## Patterns

- **State = BehaviorSubjects in services** — no NgRx, no Akita. `AuthService.authState$`, `CartService.cartItems$` are the canonical pattern.
- **Mixed interceptor styles**: `authInterceptor` is functional (registered via `withInterceptors([authInterceptor])` in `app.config.ts`); `DateConverterInterceptor` is class-based (registered via `HTTP_INTERCEPTORS` multi-provider). Both run — functional interceptors run first.
- **Token refresh serialization**: `authInterceptor` uses a module-level `isRefreshing` flag plus a `ReplaySubject` to ensure only one refresh flight is in progress; queued requests are released with the new token.
- **Error extraction**: Always use `extractErrorMessage(err)` — don't assume `err.error.message` exists. Backend sends `ApiResponse` envelopes with `errors: string[]`.

## Integration

- `auth.interceptor` depends on `AuthService`. Circular-dependency risk handled by injecting via `inject()` inside the interceptor function (not constructor).
- Services call the backend's `MeuCatalogo.API`; base URL lives in `environments/`.
- `image-url.service` is the only place that should transform image paths — other code should not re-concatenate URLs.

## Gotchas

- **Product image shape**: `product.imagens` can be `string[]` or `ProdutoImagem[]`. New code must type-check: `typeof img === 'string' ? img : img.url`.
- **Token storage is `localStorage`** — cleared on logout. Do not move to cookies without updating the interceptor and `AuthService.logout()`.
- **401 handling is inside the interceptor**, not at component level. Don't add your own 401 retry in feature services.
- **Sync user access** via `AuthService.getCurrentUserSync()` may return `null` before `authState$` resolves — prefer the observable when rendering.
- **Date interceptor walks the entire response body** — deeply nested objects pay the cost. If performance becomes an issue, consider scoping by URL.

## When Adding Code

- New service → `@Injectable({ providedIn: 'root' })`.
- New HTTP behavior → prefer a functional interceptor in `withInterceptors()` over the class-based pattern.
- New guard → functional (`CanActivateFn`) preferred over class-based.
- New model → place in `core/models/`, keep consistent with backend DTO shape (camelCase on the wire).
