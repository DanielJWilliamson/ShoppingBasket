# Repository structure

Current (single-container, two areas):

- Backend: `src/backend/ShoppingBasket` (serves SPA from `wwwroot`; consolidated Api/Application/Domain/Infrastructure)
- Frontend: `src/frontend/ClientApp`
- Tests
  - Backend unit/integration: `tests/`
  - Frontend unit/E2E: under `ClientApp`
- Docker
  - `Dockerfile` multi-stage (build SPA, copy to API `wwwroot`, publish API)
  - `docker-compose.yml` (service `shoppingbasket` → 8080:8080)

## Cleanup done
- Excluded placeholder `Class1.cs` from compilation in all C# projects.
- Compose standardized to `shoppingbasket` with `shoppingbasket:latest` image.

## Two-area layout applied
The SPA has been moved to `src/frontend/ClientApp` and the backend consolidated under `src/backend/ShoppingBasket`. The Dockerfile has been updated to:

1. `COPY src/frontend/ClientApp/package*.json` + `RUN npm ci` (UI stage)
2. `COPY src/frontend/ClientApp .` then `npm run build` (UI stage)
3. `COPY --from=ui-build /ui/dist/ ./src/backend/ShoppingBasket/wwwroot/` (build stage)

This preserves a single container while making the source tree clearer.
