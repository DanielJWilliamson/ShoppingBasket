# Security Model

This document explains core security considerations for ShoppingBasket.

## Goals
- Safe-by-default configuration
- No secrets in source control
- Minimal attack surface for a demo app

## Authentication & Authorization
- This demo app does not include user auth; orders are captured for a demo customer context
- If you add auth, prefer OpenID Connect with secure cookie/session or token-based flows

## Data protection
- SQLite DB is stored on a Docker volume; not committed to Git
- For production, prefer a managed database and at-rest encryption

## Input validation
- Validate and sanitize user inputs in controllers and services
- Avoid overposting; use DTOs with explicit bindings

## Headers & HTTPS
- Behind a reverse proxy, terminate TLS and forward headers appropriately
- Enforce HTTPS and security headers in production

## Secrets management
- Use environment variables or a secrets store (Azure Key Vault, AWS Secrets Manager)
- Do not commit secrets; `.gitignore` excludes .env files
