# Data Model (Reference)

## Entities

### Product
- Id (int)
- SKU (string, unique)
- Name (string)
- Description (string?)
- Price (decimal)
- ImageUrl (string?)
- Category (string?)
- IsDiscountExempt (bool) — e.g., promo item

### Basket
- Id (int)
- CustomerId (string)
- Currency (string)
- CountryCode (string)
- AppliedDiscountCodesCsv (string)

### BasketItem
- Id (int)
- BasketId (int)
- ProductId (int)
- Quantity (int)
- UnitPrice (decimal)

### Order
- Id (int)
- CustomerId (string)
- OrderNumber (string, unique)
- Currency (string)
- CountryCode (string)
- Subtotal (decimal)
- DiscountedSubtotal (decimal)
- Vat (decimal)
- Shipping (decimal)
- Total (decimal)
- CreatedAt (DateTimeOffset)
- Status (string)

### OrderItem
- Id (int)
- OrderId (int)
- ProductId (int?)
- SKU (string)
- Name (string)
- Quantity (int)
- UnitPrice (decimal)
- LineTotal (decimal)

### Discount
- Id (int)
- Code (string)
- Percentage (decimal?)
- FixedAmount (decimal?)
- IsStackable (bool)
- AllowedSkusCsv (string?)
- ExcludedSkusCsv (string?)
