using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ShoppingBasket.Domain.Entities;
using ShoppingBasket.Infrastructure;
using ShoppingBasket.Infrastructure.Services;

namespace ShoppingBasket.UnitTests;

public class VoucherCalculationTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task Applying_10percent_Reduces_NonExempt_Only()
    {
        using var db = CreateDb();
        // Seed products
        var normal = new Product { Id = 100, SKU = "NORM-1", Name = "Normal", Price = 100m, IsDiscountExempt = false };
        var promo = new Product { Id = 101, SKU = "PROMO-1", Name = "Promo", Price = 50m, IsDiscountExempt = true };
        db.Products.AddRange(normal, promo);
        db.Discounts.Add(new Discount { Id = 10, Code = "10percent", Percentage = 0.10m, IsStackable = true });
        await db.SaveChangesAsync();

        var basket = new Basket { CustomerId = "test" };
        basket.Items.Add(new BasketItem { ProductId = normal.Id, Quantity = 1, UnitPrice = normal.Price });
        basket.Items.Add(new BasketItem { ProductId = promo.Id, Quantity = 1, UnitPrice = promo.Price });
        db.Baskets.Add(basket);
        await db.SaveChangesAsync();

        var svc = new BasketService(db);
    var dto1 = await svc.GetByIdAsync(basket.Id);
    dto1!.Subtotal.Should().Be(150m); // no discount applied yet in preview subtotal before vouchers

    var after = await svc.ApplyVoucherAsync(basket.Id, "10percent");
    // discounted subtotal should be 100*0.9 + 50 (promo exempt) = 140
    after.Subtotal.Should().Be(140m);
    after.AppliedDiscountCodes.Should().Contain("10percent");
    }
}
