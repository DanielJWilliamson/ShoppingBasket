<div align="center">

# ShoppingBasket

Single-container shopping app: ASP.NET Core backend serves a React/Vite SPA from `wwwroot`.

![build](https://img.shields.io/badge/build-passing-brightgreen) ![license](https://img.shields.io/badge/license-MIT-blue)

</div>

## Overview
ShoppingBasket demonstrates a clean, practical e-commerce flow with vouchers, VAT, shipping, and orders. It’s ideal for demos, interviews, or as a starting point.

## Key features
- React/Vite SPA served by ASP.NET Core
- Voucher codes (10/20/30), VAT toggle, shipping by region, promo SKU handling
- Seeded catalog and discounts on first run
- Unit, integration, and Playwright E2E tests
- One-command Docker startup

## Getting started (5 minutes)
Prerequisites: Docker Desktop

```powershell
# from a terminal
git clone <your-repo-url>
cd ShoppingBasket

docker compose up -d --build
curl http://localhost:8080/health
```
Open http://localhost:8080.

For full tutorial, see docs/getting-started/01-installation.md.

## Project layout
- Backend (C#): `src/backend/ShoppingBasket` (single consolidated project)
- Frontend (React/Vite/TS): `src/frontend/ClientApp`
- Tests: `tests/ShoppingBasket.UnitTests`, `tests/ShoppingBasket.IntegrationTests`, SPA unit/E2E under ClientApp
- Docker: `Dockerfile` (multi-stage), `docker-compose.yml` (single service `shoppingbasket`)

## Documentation
See the docs site (Diátaxis):
- Getting started: `docs/getting-started/01-installation.md`
- Concepts: `docs/concepts/architectural-overview.md`
- Reference: `docs/reference/api-endpoints.md`

## How to contribute
We welcome issues, ideas, and PRs. See [CONTRIBUTING.md](CONTRIBUTING.md) and [CONDUCT.md](CONDUCT.md).

## License
This project is licensed under the MIT License. See [LICENSE](LICENSE).
