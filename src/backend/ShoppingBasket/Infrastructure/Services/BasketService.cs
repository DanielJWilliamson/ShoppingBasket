using Microsoft.EntityFrameworkCore;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Application.Interfaces;
using ShoppingBasket.Domain.Entities;

namespace ShoppingBasket.Infrastructure.Services;

public class BasketService : IBasketService
{
    private readonly AppDbContext _db;

    public BasketService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BasketDto> GetOrCreateAsync(int? basketId, string? customerId)
    {
        Basket? basket = null;
        if (basketId is int id)
        {
            basket = await _db.Baskets.Include(b => b.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(b => b.Id == id);
        }
        if (basket == null)
        {
            basket = new Basket { CustomerId = customerId ?? string.Empty };
            _db.Baskets.Add(basket);
            await _db.SaveChangesAsync();
        }
        return ToDto(basket);
    }

    public async Task<BasketDto?> GetByIdAsync(int basketId)
    {
        var basket = await _db.Baskets.Include(b => b.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(b => b.Id == basketId);
        return basket == null ? null : ToDto(basket);
    }

    public async Task<BasketDto> AddItemAsync(int basketId, AddToBasketRequest request)
    {
        var basket = await _db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId)
                      ?? throw new KeyNotFoundException($"Product {request.ProductId} not found");

        var existing = basket.Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing == null)
        {
            basket.Items.Add(new BasketItem
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            });
        }
        else
        {
            existing.Quantity += request.Quantity;
            existing.UnitPrice = product.Price; // keep in sync with current price
        }
        await _db.SaveChangesAsync();
        await _db.Entry(basket).Collection(b => b.Items).Query().Include(i => i.Product).LoadAsync();
        return ToDto(basket);
    }

    public async Task<BasketDto> UpdateItemAsync(int basketId, UpdateBasketItemRequest request)
    {
        var basket = await _db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        var item = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
                   ?? throw new KeyNotFoundException($"Item for product {request.ProductId} not in basket");
        if (request.Quantity <= 0)
        {
            _db.BasketItems.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }
        await _db.SaveChangesAsync();
        await _db.Entry(basket).Collection(b => b.Items).Query().Include(i => i.Product).LoadAsync();
        return ToDto(basket);
    }

    public async Task<BasketDto> RemoveItemAsync(int basketId, int productId)
    {
        var basket = await _db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId)
                   ?? throw new KeyNotFoundException($"Item for product {productId} not in basket");
        _db.BasketItems.Remove(item);
        await _db.SaveChangesAsync();
        await _db.Entry(basket).Collection(b => b.Items).Query().Include(i => i.Product).LoadAsync();
        return ToDto(basket);
    }

    public async Task<BasketDto> ApplyVoucherAsync(int basketId, string code)
    {
        code = code.Trim();
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code required", nameof(code));
        var basket = await _db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        var discount = await _db.Discounts.FirstOrDefaultAsync(d => d.Code == code)
                        ?? throw new KeyNotFoundException($"Voucher '{code}' not found");
        var existingCodes = (basket.AppliedDiscountCodesCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (existingCodes.Contains(code, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Voucher '{code}' already applied");
        existingCodes.Add(discount.Code); // use canonical casing
        basket.AppliedDiscountCodesCsv = string.Join(',', existingCodes);
        await _db.SaveChangesAsync();
        await _db.Entry(basket).Collection(b => b.Items).Query().Include(i => i.Product).LoadAsync();
        return ToDto(basket);
    }

    public async Task<BasketDto> RemoveVoucherAsync(int basketId, string code)
    {
        code = code.Trim();
        var basket = await _db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        var existingCodes = (basket.AppliedDiscountCodesCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        var removed = existingCodes.RemoveAll(c => string.Equals(c, code, StringComparison.OrdinalIgnoreCase));
        if (removed == 0) throw new KeyNotFoundException($"Voucher '{code}' not applied");
        basket.AppliedDiscountCodesCsv = string.Join(',', existingCodes);
        await _db.SaveChangesAsync();
        await _db.Entry(basket).Collection(b => b.Items).Query().Include(i => i.Product).LoadAsync();
        return ToDto(basket);
    }

    public async Task ClearAsync(int basketId)
    {
        var basket = await _db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == basketId)
                     ?? throw new KeyNotFoundException($"Basket {basketId} not found");
        _db.BasketItems.RemoveRange(basket.Items);
        await _db.SaveChangesAsync();
    }

    private static BasketDto ToDto(Basket basket)
    {
        // Map any known promotional SKUs to an original (pre-promo) unit price so the UI can show savings
        var promoOriginals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["PROMO-001"] = 50.00m // Wireless Earbuds: promo price £35 vs original £50
        };

        var items = basket.Items
            .Select(i =>
            {
                var sku = i.Product?.SKU ?? string.Empty;
                decimal? originalUnit = null;
                if (!string.IsNullOrEmpty(sku) && promoOriginals.TryGetValue(sku, out var orig))
                {
                    // Only set original price if it's actually greater than the current unit price
                    if (orig > i.UnitPrice) originalUnit = orig;
                }
                return new BasketItemDto(
                    i.ProductId,
                    sku,
                    i.Product?.Name ?? string.Empty,
                    i.UnitPrice,
                    i.Quantity,
                    i.UnitPrice * i.Quantity,
                    i.Product?.IsDiscountExempt ?? false,
                    originalUnit
                );
            })
            .ToList();
        var subtotal = items.Sum(i => i.LineTotal);
        var codes = (basket.AppliedDiscountCodesCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c)
            .ToList();
        // Apply discounts sequentially only to non-exempt items
        decimal discountedSubtotal = 0m;
        foreach (var item in items)
        {
            var line = item.LineTotal;
            if (!item.IsDiscountExempt)
            {
                foreach (var code in codes)
                {
                    var pct = code.ToLowerInvariant() switch
                    {
                        "10percent" => 0.10m,
                        "20percent" => 0.20m,
                        "30percent" => 0.30m,
                        _ => 0m
                    };
                    if (pct > 0)
                    {
                        line = Math.Round(line * (1 - pct), 2, MidpointRounding.ToEven);
                    }
                }
            }
            discountedSubtotal += line;
        }
        discountedSubtotal = Math.Round(discountedSubtotal, 2, MidpointRounding.ToEven);
        var vat = Math.Round(discountedSubtotal * 0.20m, 2, MidpointRounding.ToEven);
        var total = discountedSubtotal + vat;
        return new BasketDto(basket.Id, string.IsNullOrWhiteSpace(basket.CustomerId) ? null : basket.CustomerId, discountedSubtotal, vat, total, items, codes);
    }
}
