# App (Angular)

Root Angular application. Hybrid architecture: **app root is standalone**, but a subset of feature modules still use NgModules + lazy-loading. New work should favor standalone components.

## Files

- **`app.component.ts`**: Standalone root. Imports `RouterModule`. Shell is just `<router-outlet>`.
- **`app.config.ts`**: `ApplicationConfig` — `provideRouter(routes)`, `provideHttpClient(withInterceptors([authInterceptor]))`, plus the class-based `DateConverterInterceptor` via `HTTP_INTERCEPTORS` multi-provider. Also wires Sentry.
- **`app.routes.ts`**: Active route table (standalone style). Public: `/`, `/signin`, `/signup`, `/checkout`. Protected (wrapped in `AppLayoutComponent` via a parent route with `AuthGuard`): `/dashboard/*`.
- **`app-routing.module.ts`**: Legacy NgModule-based routing. Still present and used by the `features/*` lazy-loaded modules. Don't add new routes here.

## Submodules

- **`core/`** — Guards, interceptors, models, pipes, services, utils. All `providedIn: 'root'`. See `core/CLAUDE.md`.
- **`shared/`** — Layout shells, large reusable component library, shared services. See `shared/CLAUDE.md`.
- **`pages/`** — Route-level components (mix of business-critical and UI showcase). See `pages/CLAUDE.md`.
- **`features/`** — Legacy NgModule + lazy-loaded features (`dashboard/`, `products/`). Uses Angular Material. See `features/CLAUDE.md`.
- **`layouts/`** — Legacy `main-layout/` NgModule shell; superseded by `shared/layout/`. Present but not on the active path.
- **`models/cart-item.model.ts`** — Shopping cart item type.
- **`services/cart.service.ts`** — Cart state via BehaviorSubject, persisted to `localStorage`. `cartItems$`, `cartTotal$`, `cartCount$`.

## Architecture

- **State management**: No store. Each service exposes `BehaviorSubject` / `ReplaySubject` streams (e.g., `AuthService.authState$`, `CartService.cartItems$`).
- **HTTP**: Two interceptors — functional `authInterceptor` (token injection + 401 refresh) runs first; class-based `DateConverterInterceptor` walks responses converting ISO strings to `Date`.
- **Styling**: Tailwind CSS v4 + custom CSS variables in `styles.css`. Dark mode via `@custom-variant dark`.
- **Component libraries in mix**: Tailwind + shared custom components everywhere; Angular Material only inside `features/products/` and `features/dashboard/`.
- **Error handling**: `extractErrorMessage(err)` util (in `core/utils/error.utils.ts`) handles both `ApiResponse`-shaped backend errors and raw `HttpErrorResponse`.
- **Observability**: Sentry initialized in `core/services/sentry.service.ts`, invoked from `app.config.ts`.

## Gotchas

- **Two routing systems coexist** — `app.routes.ts` (standalone, active) and `app-routing.module.ts` (NgModule, legacy). A route can exist in either. Don't duplicate; new routes go in `app.routes.ts`.
- **Mixed standalone / NgModule** — the pattern is not uniform across pages. Check the component's `@Component({ standalone: ... })` before importing.
- **Interceptor order**: Functional interceptors from `withInterceptors()` run before class-based ones from `HTTP_INTERCEPTORS`. Changing order requires consolidating them.
- **Product image shape**: `Product.imagens` can be `string[]` (legacy) or `ProdutoImagem[]` (`{ url, isPrincipal, ordem }`). New code must handle both.
- **Cart persists only in localStorage** — there is no backend cart sync. Sessions don't roam across devices.
- **`calender/`** (misspelled) is a real folder for the calendar page — preserve the spelling in imports.

## When Adding Code

- New page → standalone component, register in `app.routes.ts`.
- New HTTP interceptor → functional, added to `withInterceptors([...])`.
- New service → `@Injectable({ providedIn: 'root' })` under `core/services/`.
- New shared component → standalone, under `shared/components/<category>/`.
- Don't introduce NgRx, PrimeNG, or a second component library.
