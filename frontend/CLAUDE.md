# Frontend

Angular 20 single-page app. Public storefront + authenticated admin dashboard. Tailwind v4 for styling; Sentry for error tracking; ApexCharts / FullCalendar / Swiper for UI widgets.

## Layout

- **`src/app/`** — the Angular app itself. See `src/app/CLAUDE.md` for architecture.
- **`src/environments/`** — API base URL per environment (`environment.ts`, `environment.prod.ts`).
- **`src/index.html`, `src/main.ts`, `src/styles.css`** — bootstrap + global Tailwind theme (custom `@theme` + `@custom-variant dark`).
- **`public/`** — static assets.
- **`angular.json`** — build configuration (budgets, `outputPath: dist/`).
- **`package.json`** — dependencies; note the explicit mixed stack: Tailwind + a few Angular Material components inside `features/*`.
- **`tsconfig.*.json`, `.postcssrc.json`** — TS + PostCSS config.

## Deployment Artifacts (not app code)

This project ships with many deploy scripts because the same build targets multiple hosting models. Keep them isolated from app code:

- **Firebase**: `deploy-firebase.sh`, `firebase.json`, `.firebaserc`, `DEPLOY-FIREBASE.md`.
- **FTP**: `deploy-ftp*.sh`, `deploy-ftp*.py`, `DEPLOY-FTP.md`, `diagnose-ftp.py`, `test-ftp*.py`.
- **IIS**: `deploy-to-iis.bat`, `deploy-to-iis.ps1`, `DEPLOY-IIS.md`.
- **macOS**: `deploy-macos.sh`, `DEPLOY-MACOS.md`.
- Utilities: `build-info.js`, `check-zip.sh`, `package-release.sh`, `verify-deployment.ps1`, `quick-deploy.sh`.

Do not modify these for app-logic changes — they are packaging/transport concerns only.

## Key Decisions

- **Hybrid app architecture**: root is standalone; `features/*` still uses NgModules + lazy-loading. New work should favor standalone. See `src/app/CLAUDE.md`.
- **No store**: state in services via `BehaviorSubject`.
- **Two HTTP interceptors**: functional `authInterceptor` first, class-based `DateConverterInterceptor` second.
- **Token storage in `localStorage`**.
- **SEO support**: `core/services/seo.service.ts` + `sitemap.service.ts` suggest SSR-ready/SEO-aware pages (confirm via `SEO-README.md`).
- **Tailwind v4 syntax** — not v3. `@theme` and `@custom-variant` in `styles.css`.

## Integration

- Consumes the `../backend/` API. Base URL from `environments/`.
- Shares the `ApiResponse<T>` contract shape with the backend — errors come through as `{ isSuccess, errors, message }` and are unwrapped by `core/utils/error.utils.ts`.
- Image URLs on `Product.imagens` can be legacy strings or the `{ url, isPrincipal, ordem }` shape — handle both.

## Gotchas

- **Two routing systems coexist** in the app — `app.routes.ts` (standalone, active) and `app-routing.module.ts` (NgModule, legacy). New routes go in `app.routes.ts`.
- **Tailwind v4 syntax** — copying a v3 config will silently break theming.
- **Feature modules use Angular Material**, the rest uses Tailwind + custom components. Don't pull Material into standalone pages.
- **Cart state is localStorage-only** — no backend sync, no cross-device continuity.
- **Sentry init lives in `core/services/sentry.service.ts`** and is wired from `app.config.ts`.

## When Adding Code

- New page → standalone component, registered in `app.routes.ts`.
- New HTTP call → service in `src/app/core/services/`, shaped around backend DTOs.
- New shared UI → standalone component under `src/app/shared/components/<category>/`.
- Keep deploy scripts out of app code commits.
