# Features

Feature modules using the **NgModule + lazy-loading** pattern. Contrast with `../pages/` which mostly uses standalone components. Feature modules are the legacy organizational model in this codebase.

## Files

- **`dashboard/`**:
  - `dashboard.component.ts` — stats and recent-activity panel.
  - `dashboard.module.ts` — `NgModule` declaration.
  - `dashboard.routes.ts` — feature-scoped routes.
- **`products/`**:
  - `products.module.ts` — NgModule, lazy-loaded.
  - `products.routes.ts` — feature routes.
  - `product-list/product-list.component.ts` — `MatTable` + `MatPaginator` + `MatSort`, search/filter, CRUD actions, `MatDialog` for forms, `MatSnackBar` for notifications.
  - `product-form/product-form.component.ts` — create/edit form hosted in a `MatDialog`.
  - `category-form/category-form.component.ts` — inline category creation dialog.

## Patterns

- **NgModule + lazy-load**: Feature modules are loaded via `loadChildren: () => import(...).then(m => m.FeatureModule)` in `app-routing.module.ts`.
- **Material UI locally**: Even though the rest of the app is Tailwind-first, these feature modules lean on `@angular/material` (`MatTable`, `MatDialog`, `MatSnackBar`, `MatPaginator`, `MatSort`).
- **Feature-scoped services** are imported from `core/services/*` — feature modules don't own their own HTTP services.

## Integration

- Imports from: `core/services/*`, `shared/components/*`.
- Routes declared in `products.routes.ts` / `dashboard.routes.ts` are mounted under `/dashboard/*` by `app-routing.module.ts`.
- Products feature uses `image-url.service` for image transforms — don't concatenate URLs yourself.

## Gotchas

- **Two routing systems coexist**: `app.routes.ts` (active, standalone-style) and `app-routing.module.ts` (legacy, NgModule-style). Features here attach via the legacy system. Don't duplicate a route in both.
- **Mixed UI stack**: Material components live only in features. The rest of the app uses Tailwind + custom shared components. Adding Material to a standalone page means importing the Material module — prefer the shared components instead.
- **Lazy-loaded feature route** = the feature module must declare its own routing; do not duplicate those routes in `app.routes.ts`.
- **MatDialog data contract**: product/category forms expect a specific `data` payload shape — check the dialog open calls in `product-list` before refactoring.

## When Adding Code

- New feature of similar complexity → prefer **standalone components** under `pages/` instead of a new NgModule here. This folder is the legacy model.
- Inside existing features: keep NgModule pattern for consistency within that feature; don't mix styles inside one feature.
- UI convention: reuse Material components already in use within features; don't introduce PrimeNG or a third component library.
