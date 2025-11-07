# Setting up Monitoring

This guide outlines simple monitoring options.

## Health checks
- Built-in endpoint: `GET /health` (returns 200 OK when healthy)

## Container metrics
- Use Docker Desktop or `docker stats` for basic CPU/memory
- Consider Prometheus/Grafana for metrics scraping and dashboards

## Application logs
- Stream logs: `docker logs -f shoppingbasket`
- In .NET, configure Serilog or built-in logging to target a centralized sink

## Alerts
- Add simple uptime checks (Pingdom, UptimeRobot) for `/health`
- Alert on container restarts and error rates
