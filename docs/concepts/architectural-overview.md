# Architectural Overview

ShoppingBasket is a single-container web application:
- Backend: ASP.NET Core 9 at `src/backend/ShoppingBasket`
- Frontend: React + Vite SPA at `src/frontend/ClientApp` (built during Docker image creation)
- Container: The backend serves the compiled SPA from `wwwroot`

## High-level diagram
![Architecture](../assets/architecture-diagram.png)

## Components
- API: Controllers expose endpoints for products, basket, orders
- Domain: Entities (Product, Basket, Order) and business rules (vouchers, VAT, shipping)
- Infrastructure: EF Core (SQLite), data access, seeding
- Frontend: SPA consuming API, offering basket/checkout flows and E2E tests

## Data flow
1. SPA requests catalog and basket data from the API
2. API reads/writes via EF Core to SQLite
3. SPA displays computed totals; server persists orders

## Build and deploy
- Dockerfile uses multi-stage: build SPA, copy to backend wwwroot, publish .NET app
- `docker-compose.yml` runs a single service `shoppingbasket` on port 8080
