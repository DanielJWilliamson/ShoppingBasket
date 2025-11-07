# Configuration

ShoppingBasket works out of the box. You can customize behavior using environment variables (see Reference → Environment Variables) or by editing appsettings.*.

## Docker compose
The default compose maps port 8080 and sets the SQLite file under /app/data.

```yaml
services:
  shoppingbasket:
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Data Source=/app/data/app.db
    volumes:
      - sb-data:/app/data
```

## Backend overrides
- ASPNETCORE_ENVIRONMENT: Development/Production
- ConnectionStrings__Default: Replace SQLite with your connection if needed

## Frontend build
The SPA is built during Docker image creation and served from backend `wwwroot`.
