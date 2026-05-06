# Shared

Reusable layout, UI components, form controls, and shared services. Large component library (100+ components) inherited from the Tailwind Angular template the project was forked from, extended with business-specific pieces.

## Submodules

- **`layout/`**: Chrome shells.
  - `app-layout/` — protected dashboard wrapper (sidebar + header + content).
  - `app-header/` — top bar with user menu.
  - `app-sidebar/` — collapsible left nav; state managed by `SidebarService`.
  - `auth-page-layout/` — signin/signup chrome.
  - `generator-layout/` — builder/generator chrome.
  - `backdrop/` — mobile sidebar overlay.
- **`components/`**: Business and UI components.
  - `auth/` — `SigninFormComponent`, `SignupFormComponent` (the forms themselves; `pages/auth-pages/` just hosts them).
  - `cards/` — Card templates (with image, links, icons, horizontal, etc.).
  - `charts/` — ApexCharts wrappers (BarChart, LineChart).
  - `ecommerce/` — `ProductListTable`, `RecentOrders`, `BillingInfo`, `DemographicCard`, `MonthlySalesChart`.
  - `form/` — `InputField`, `Label`, `Select`, `MultiSelect`, `Checkbox`, `DatePicker`, `TimePicker`, etc.
  - `tables/` — `BasicTable` variants.
  - `common/` — `PageBreadcrumb`, `TableDropdown`, `ComponentCard`, `CountdownTimer`, `ChartTab`, `ThemeToggle`.
  - `ui/` — `Alert`, `Avatar`, `Badge`, `Button`, `Dropdown`, `Modal`, `Table`, responsive `Images`/`Videos`.
  - `image-upload/` — image picker with crop (uses `cropperjs`).
  - `image-editor/` — image editor component.
  - `cart-modal/` — shopping cart modal.
  - `transactions/` — `TransactionHistory`, `OrderDetails`, `CustomerDetails`.
  - `invoice/` — `InvoiceList`, `InvoiceMain`, `InvoiceTable`, `PaymentMethod`.
- **`services/sidebar.service.ts`**: Sidebar state — `isExpanded$`, `isHovered$`, `isMobileOpen$` (BehaviorSubjects).
- **`pipe/safe-html.pipe.ts`**: `DomSanitizer.bypassSecurityTrustHtml` wrapper for trusted innerHTML binding.
- **`core.module.ts`**: Legacy aggregator NgModule; not used by standalone consumers.

## Patterns

- **Component style is mixed** — some components are standalone, others are NgModule-declared. Check the decorator before importing; standalone components are imported directly, NgModule components require their feature module.
- **Tailwind-first styling** — components use Tailwind utility classes; no component-scoped styles except where Tailwind cannot express the rule.
- **Theme via CSS custom properties** — global theme defined in `styles.css` under `@theme` blocks; dark mode via `@custom-variant dark`.
- **Icons**: inline SVG or via Angular component, not an icon-font library.

## Integration

- Consumed by `pages/*` and `features/*`.
- `SidebarService` is the single source of truth for sidebar state — `app-header`, `app-sidebar`, `backdrop` all subscribe.
- `SafeHtmlPipe` is the only sanctioned path for injecting server-provided HTML (e.g., product `informacoesAdicionais`).

## Gotchas

- **Many near-duplicate components** — multiple `Card*`, `Button*` variants. Don't add a new one without checking existing ones first; prefer extending an existing variant.
- **Standalone vs NgModule split is not documented per-component** — inspect the decorator. Mixing them in imports causes cryptic template errors.
- **`SafeHtmlPipe` bypasses sanitization** — only use with server content you trust (e.g., `Produto.InformacoesAdicionais` authored by the catalog owner). Never pass user-provided free-text from an unauthenticated form.
- **Tailwind v4** is in use — syntax differs from v3 (e.g., `@theme` block, `@custom-variant`). Do not copy v3 configs wholesale.
- **Cart state** lives in `../services/cart.service.ts` (app root, not here) with localStorage persistence — `cart-modal` reads from it.

## When Adding Code

- Prefer extending an existing component over adding a new one.
- New components should be standalone unless they must integrate with an NgModule consumer.
- New icons → inline SVG; no icon-font dependencies.
- Service state → BehaviorSubject pattern matching `SidebarService`.
