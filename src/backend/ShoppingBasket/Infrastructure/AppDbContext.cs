using Microsoft.EntityFrameworkCore;
using ShoppingBasket.Domain.Entities;

namespace ShoppingBasket.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

        modelBuilder.Entity<BasketItem>()
            .HasOne(i => i.Basket!)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order!)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Catalog seed (includes promotional product exempt from vouchers)
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1,  SKU = "BOOK-001",   Name = "Intro to C#",         Description = "Beginner guide", Price = 12.99m, ImageUrl = "/img/book1.jpg",  Category = "Books" },
            new Product { Id = 2,  SKU = "BOOK-002",   Name = "Advanced .NET",       Description = "Deep dive",      Price = 29.50m, ImageUrl = "/img/book2.jpg",  Category = "Books" },
            new Product { Id = 3,  SKU = "ELC-USB-C1", Name = "USB‑C Cable 1m",     Description = "Fast charge",     Price = 7.49m,  ImageUrl = "/img/cable.jpg",  Category = "Electronics" },
            new Product { Id = 4,  SKU = "ELC-ADPT-1", Name = "Power Adapter 65W",  Description = "GaN charger",      Price = 39.99m, ImageUrl = "/img/charger.jpg",Category = "Electronics" },
            new Product { Id = 5,  SKU = "HM-KET-01",  Name = "Electric Kettle",     Description = "1.7L, auto shutoff", Price = 24.95m, ImageUrl = "/img/kettle.jpg", Category = "Home" },
            new Product { Id = 6,  SKU = "HM-MUG-01",  Name = "Ceramic Mug",         Description = "350ml",           Price = 4.50m,  ImageUrl = "/img/mug.jpg",    Category = "Home" },
            new Product { Id = 7,  SKU = "TOY-CAR-01", Name = "RC Car",             Description = "2.4GHz",          Price = 49.90m, ImageUrl = "/img/rccar.jpg",  Category = "Toys" },
            new Product { Id = 8,  SKU = "SPRT-MAT1",  Name = "Yoga Mat",           Description = "Non-slip",        Price = 18.00m, ImageUrl = "/img/yogamat.jpg",Category = "Sports" },
            new Product { Id = 9,  SKU = "PROMO-001",  Name = "Wireless Earbuds (Promo)", Description = "Limited Offer - 30% Off", Price = 35.00m, ImageUrl = "/img/mug.jpg", Category = "Electronics", IsDiscountExempt = true }
        );

        // Voucher codes seed (percent discounts, non-stackable beyond sequence order; we treat them sequentially)
        modelBuilder.Entity<Discount>().HasData(
            new Discount { Id = 1, Code = "10percent", Percentage = 0.10m, IsStackable = true },
            new Discount { Id = 2, Code = "20percent", Percentage = 0.20m, IsStackable = true },
            new Discount { Id = 3, Code = "30percent", Percentage = 0.30m, IsStackable = true }
        );
    }
}
