# Deploying to Production

This guide shows how to build and run the ShoppingBasket container for production.

## Build an image
```powershell
# from repo root
docker build -t shoppingbasket:prod .
```

## Run the container
```powershell
docker run -d --name shoppingbasket -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production -v sb-data:/app/data shoppingbasket:prod
```

## Health and logs
- Health: `GET /health`
- Logs: `docker logs -f shoppingbasket`

## Configuration tips
- Consider an external database for persistent, shareable data across instances
- Serve behind a reverse proxy (e.g., Nginx, Traefik) for TLS and path-based routing
- Enable structured logging and forward to a log sink (e.g., ELK, Seq)
