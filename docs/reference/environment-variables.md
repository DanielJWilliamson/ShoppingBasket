# Environment Variables (Reference)

## Backend
- `ASPNETCORE_ENVIRONMENT` — Development/Production
- `ConnectionStrings__Default` — Connection string for EF Core (default: SQLite at /app/data/app.db)

## Docker
- Ports, volumes, and environment are configured in `docker-compose.yml`.

## Frontend
- The SPA is built with Vite; custom env vars can be injected at build time if needed.
