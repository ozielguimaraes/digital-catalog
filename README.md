# MeuCatalogo

[![Deploy API](https://github.com/ozielguimaraes/digital-catalog/actions/workflows/deploy-api.yml/badge.svg?branch=main)](https://github.com/ozielguimaraes/digital-catalog/actions/workflows/deploy-api.yml)
[![Deploy Frontend](https://github.com/ozielguimaraes/digital-catalog/actions/workflows/deploy-frontend.yml/badge.svg?branch=main)](https://github.com/ozielguimaraes/digital-catalog/actions/workflows/deploy-frontend.yml)

Plataforma de catálogo digital multi-tenant. Um usuário cria catálogos com categorias e produtos, gerencia imagens/estoque/preços, e expõe publicamente para clientes navegarem.

## Estrutura

- **`src/backend/`** — ASP.NET Core 6 API. Veja [`src/backend/README.md`](src/backend/README.md).
- **`src/frontend/`** — Angular 20 (storefront + admin). Veja [`src/frontend/README.md`](src/frontend/README.md).
- **`src/mobile/`** — .NET MAUI app cross-plataforma.
- **`tests/`** — testes de carga k6.
