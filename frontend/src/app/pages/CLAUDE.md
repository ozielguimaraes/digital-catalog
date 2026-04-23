# Pages

Route-level components. Mix of standalone and NgModule-based components; split roughly into **business pages** (home, auth, catalogs, products, checkout) and **UI showcase pages** (charts, tables, form-elements) inherited from the template this project was forked from.

## Files — Business-Critical

- **`home/home.component.ts`** (standalone): Public storefront. Loads catalogs → categories → products on init; filters by catalog/category/search; shows product gallery with modal details; integrates `CartModalComponent`. Uses `ChangeDetectorRef` for cart count updates.
- **`auth-pages/`**: Signin/signup/forgot-password wrappers. Inner forms live in `shared/auth/`. Guarded by `GuestGuard`.
- **`products/products.component.ts`** (standalone): Admin product management (distinct from the NgModule product feature in `../features/products/`).
- **`products/product-create.component.ts` / `product-edit.component.ts`**: Admin product forms.
- **`catalogs/catalogs.component.ts`**: Admin catalog list + CRUD.
- **`checkout/checkout.component.ts`**: Public checkout flow. Uses `CartService`.
- **`profile/profile.component.ts`**: User profile editing.
- **`invoices/invoices.component.ts`**: Invoice management.

## Files — UI Showcase (template-inherited)

- **`dashboard/`**: Stats dashboard landing page.
- **`calender/calender.component.ts`**: FullCalendar demo. (Note the folder is spelled "calender".)
- **`charts/`**: ApexCharts line/bar demos.
- **`tables/`**: Basic table variants.
- **`forms/`**: Form element showcase.
- **`ui-elements/`**: Alerts, avatars, badges, buttons, images, videos showcases.
- **`other-page/`, `blank/`**: Placeholder pages.
- **`not-found.component.ts`**: Catch-all 404.

## Patterns

- **Standalone preferred** for new pages: `@Component({ standalone: true, imports: [...] })`.
- **Route registration** in `app.routes.ts` (not `app-routing.module.ts`) for standalone pages.
- **Protected routes** wrap the page in `AppLayoutComponent` via a parent route with `AuthGuard`.
- **Public routes** (home, auth, checkout) mount at the root and use `GuestGuard` when applicable.
- **Page → core service → backend**: Pages inject services from `core/services/*`; they don't talk to HTTP directly.

## Integration

- Layout: protected pages render inside `shared/layout/app-layout/app-layout.component` (sidebar + header + content).
- Auth-related pages render inside `shared/layout/auth-page-layout/`.
- Data: all pages consume `core/services/*`.
- Shared UI: `shared/ui/*`, `shared/components/*`, `shared/form/*` provide reusable pieces.

## Gotchas

- **Two product admin components exist** — `pages/products/products.component.ts` (standalone) and `features/products/product-list/product-list.component.ts` (NgModule + Material). Confirm which one the current route points to before editing.
- **`calender`** (not `calendar`) — folder name is misspelled; preserve the spelling or you will break route imports.
- **Home page uses ChangeDetectorRef manually** for cart count updates — don't remove `markForCheck`/`detectChanges` calls without verifying signals-based reactivity.
- **Mixed standalone/NgModule**: Check the component's `standalone: true` before importing — the pattern is not uniform across pages.
- **Public pages hit authed endpoints too** — home calls catalog/product APIs that have `[AllowAnonymous]` variants on the backend. Don't assume a page behind no guard means no token is sent (it is, when available, via the interceptor).

## When Adding Code

- Prefer standalone components with direct `RouterModule` / `CommonModule` imports.
- Register the route in `app.routes.ts`.
- Reuse `shared/` components (Button, InputField, Modal, etc.) — don't re-import Material unless you're extending an existing Material-based page.
- Wrap authenticated pages in the `AppLayoutComponent` via a parent route, not by importing the layout inline.
