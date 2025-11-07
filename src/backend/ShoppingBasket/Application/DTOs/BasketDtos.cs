using System.Collections.Generic;

namespace ShoppingBasket.Application.DTOs;

public record BasketItemDto(int ProductId, string SKU, string Name, decimal UnitPrice, int Quantity, decimal LineTotal, bool IsDiscountExempt, decimal? OriginalUnitPrice);
public record BasketDto(int Id, string? CustomerId, decimal Subtotal, decimal Vat, decimal Total, IReadOnlyList<BasketItemDto> Items, IReadOnlyList<string> AppliedDiscountCodes);

public record AddToBasketRequest(int ProductId, int Quantity);
public record UpdateBasketItemRequest(int ProductId, int Quantity);
