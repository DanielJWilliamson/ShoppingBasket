# ShoppingBasket

This repository is organized as a single-container app: the ASP.NET Core API serves the compiled SPA from `wwwroot`.

- Backend (C#): `src/backend/ShoppingBasket` (single consolidated project)
- Frontend (React/Vite/TS): `src/frontend/ClientApp`
- Tests:
  - Backend: `tests/ShoppingBasket.UnitTests`, `tests/ShoppingBasket.IntegrationTests`
  - Frontend: under `ClientApp` (Vitest unit, Playwright E2E)
- Docker: `Dockerfile` (multi-stage), `docker-compose.yml` (single service `shoppingbasket`)

## Cleanup performed
- Removed placeholder `Class1.cs` from project compilation (Application/Domain/Infrastructure) to keep the codebase clean.
- Standardized Docker Compose to a single container named `shoppingbasket`.

## Future structure (optional)
The repo is now organized into two clear areas under `src/`:

- `src/frontend/ClientApp` (React SPA)
- `src/backend/ShoppingBasket` (ASP.NET Core app containing former Api/Application/Domain/Infrastructure code)

Proposed:
```
src/
  frontend/
    ClientApp/
  backend/
    ShoppingBasket/
Dockerfile
docker-compose.yml
```

## Build & test
```
# Backend
 dotnet test --nologo

# Frontend
 cd src/frontend/ClientApp
 npm ci --no-audit --no-fund
 npm run typecheck
 npm run test:unit
 npm run test:e2e

# Docker
 docker compose up -d --build
 curl http://localhost:8080/health
```
