using System;
using System.Collections.Generic;

namespace ShoppingBasket.Application.DTOs;

public record OrderItemDto(int ProductId, string SKU, string Name, decimal UnitPrice, int Quantity, decimal LineTotal);
public record OrderDto(int Id, string OrderNumber, DateTimeOffset CreatedAtUtc, decimal Subtotal, decimal DiscountedSubtotal, decimal Shipping, decimal Vat, decimal Total, IReadOnlyList<OrderItemDto> Items);

// Optional per-line VAT override sent at checkout
public record VatOverride(int ProductId, bool IncludeVat);

// IncludeVat acts as a default; VatOverrides can override per product
public record CheckoutRequest(string? BuyerId, bool IncludeVat, string? ShippingCountry, IReadOnlyList<VatOverride>? VatOverrides = null);
