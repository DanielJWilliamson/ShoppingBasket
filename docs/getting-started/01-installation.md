# Installation

This tutorial gets you running ShoppingBasket locally in minutes.

## Prerequisites
- Docker Desktop (Windows/macOS/Linux)

Optional (for local dev):
- .NET 9 SDK
- Node.js 20+ (for running frontend tests locally)

## Clone and start (Docker)
```powershell
git clone <your-repo-url>
cd ShoppingBasket
# build and run the single-container app
docker compose up -d --build
# verify
curl http://localhost:8080/health
```
Open http://localhost:8080 in a browser.

## What’s included on first run
- Seeded catalog products and discount codes are created automatically if the DB is empty
- The SQLite database is stored in a Docker volume (sb-data) and not committed
- Images are bundled into the container via wwwroot/img

## Troubleshooting
- If port 8080 is in use, change the published port in `docker-compose.yml`
- To reset data, remove the volume: `docker compose down -v` (this deletes order history)
