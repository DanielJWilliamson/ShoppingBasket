using Microsoft.EntityFrameworkCore;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Application.Interfaces;
using ShoppingBasket.Domain.Entities;

namespace ShoppingBasket.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ShoppingBasket.Application.Services.IPricingService _pricing;

    public OrderService(AppDbContext db, ShoppingBasket.Application.Services.IPricingService pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    public async Task<OrderDto> CheckoutAsync(int basketId, CheckoutRequest request)
    {
        var basket = await _db.Baskets.Include(b => b.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        if (basket.Items.Count == 0)
            throw new InvalidOperationException("Basket is empty");

        // Resolve per-line VAT inclusion using overrides when provided
        var overrideMap = (request.VatOverrides ?? Array.Empty<VatOverride>()).ToDictionary(v => v.ProductId, v => v.IncludeVat);
        var vatRate = 0.20m;

    decimal subtotal = 0m;
    decimal discountedSubtotal = 0m;
    decimal vat = 0m;
        foreach (var i in basket.Items)
        {
            var lineNet = Math.Round(i.UnitPrice, 2, MidpointRounding.ToEven) * i.Quantity;
            lineNet = Math.Round(lineNet, 2, MidpointRounding.ToEven);
            subtotal += lineNet;
            // Apply basket-level voucher discounts excluding discount-exempt (promotional) items
            var isExempt = i.Product?.IsDiscountExempt ?? false;
            var lineForDiscount = lineNet;
            if (!isExempt)
            {
                lineForDiscount = ApplyBasketDiscounts(lineNet, basket.AppliedDiscountCodesCsv ?? string.Empty);
            }
            discountedSubtotal += lineForDiscount;
            var includeVatForLine = overrideMap.TryGetValue(i.ProductId, out var inc) ? inc : request.IncludeVat;
            if (includeVatForLine)
            {
                vat += Math.Round(lineForDiscount * vatRate, 2, MidpointRounding.ToEven);
            }
        }
        subtotal = Math.Round(subtotal, 2, MidpointRounding.ToEven);
        discountedSubtotal = Math.Round(discountedSubtotal, 2, MidpointRounding.ToEven);
        vat = Math.Round(vat, 2, MidpointRounding.ToEven);

    // Shipping cost: £10 for GB, £20 otherwise
    var baseShipping = (request.ShippingCountry ?? basket.CountryCode) == "GB" ? 10m : 20m;
    // Apply the same voucher sequence to shipping as requested
    var shipping = ApplyBasketDiscounts(baseShipping, basket.AppliedDiscountCodesCsv ?? string.Empty);
    var total = discountedSubtotal + shipping + vat;

        var order = new Order
        {
            CustomerId = string.IsNullOrWhiteSpace(request.BuyerId) ? basket.CustomerId : request.BuyerId!,
            OrderNumber = GenerateOrderNumber(),
            Currency = basket.Currency,
            CountryCode = basket.CountryCode,
            Subtotal = subtotal,
            DiscountedSubtotal = discountedSubtotal,
            Shipping = shipping,
            Vat = vat,
            Total = total,
            Items = basket.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                SKU = i.Product?.SKU ?? string.Empty,
                Name = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };
        _db.Orders.Add(order);

        // Clear basket
        _db.BasketItems.RemoveRange(basket.Items);

        await _db.SaveChangesAsync();

    return new OrderDto(order.Id, order.OrderNumber, order.CreatedAt, order.Subtotal, order.DiscountedSubtotal, order.Shipping, order.Vat, order.Total,
            order.Items.Select(oi => new OrderItemDto(oi.ProductId ?? 0, oi.SKU, oi.Name, oi.UnitPrice, oi.Quantity, oi.LineTotal)).ToList());
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? customerId = null)
    {
        var query = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            query = query.Where(o => o.CustomerId == customerId);
        }
        var orders = await query.OrderByDescending(o => o.Id).ToListAsync();
    return orders.Select(o => new OrderDto(o.Id, o.OrderNumber, o.CreatedAt, o.Subtotal, o.DiscountedSubtotal, o.Shipping, o.Vat, o.Total,
            o.Items.Select(oi => new OrderItemDto(oi.ProductId ?? 0, oi.SKU, oi.Name, oi.UnitPrice, oi.Quantity, oi.LineTotal)).ToList())).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var o = await _db.Orders.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (o == null) return null;
        return new OrderDto(o.Id, o.OrderNumber, o.CreatedAt, o.Subtotal, o.DiscountedSubtotal, o.Shipping, o.Vat, o.Total,
            o.Items.Select(oi => new OrderItemDto(oi.ProductId ?? 0, oi.SKU, oi.Name, oi.UnitPrice, oi.Quantity, oi.LineTotal)).ToList());
    }

    private decimal ApplyBasketDiscounts(decimal lineNet, string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return lineNet;
        var codes = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        decimal result = lineNet;
        foreach (var code in codes)
        {
            var pct = code.ToLowerInvariant() switch
            {
                "10percent" => 0.10m,
                "20percent" => 0.20m,
                "30percent" => 0.30m,
                _ => 0m
            };
            if (pct > 0m)
            {
                result = Math.Round(result * (1 - pct), 2, MidpointRounding.ToEven);
            }
        }
        return result;
    }

    private static string GenerateOrderNumber()
    {
        // Simple human-friendly unique-ish order number: YYYYMMDD-XXXX
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var rand = Random.Shared.Next(1000, 9999);
        return $"{date}-{rand}";
    }
}
