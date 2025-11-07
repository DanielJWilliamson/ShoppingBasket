# API Endpoints (Reference)

Base URL: `http://localhost:8080`

Note: This is a summary; for exact request/response shapes, consult the source or add Swagger/OpenAPI (Swashbuckle is already referenced).

## Health
- GET `/health` → 200 OK when healthy

## Products
- GET `/api/products` → list of products

## Basket
- GET `/api/basket` → current basket
- POST `/api/basket/items` → add item (productId, quantity)
- PATCH `/api/basket/items/{id}` → update quantity
- DELETE `/api/basket/items/{id}` → remove line
- POST `/api/basket/discounts` → apply voucher code

## Orders
- POST `/api/orders` → place order from current basket
- GET `/api/orders` → list orders (demo scope)
