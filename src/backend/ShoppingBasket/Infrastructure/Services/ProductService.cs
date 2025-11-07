using Microsoft.EntityFrameworkCore;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Application.Interfaces;
using ShoppingBasket.Domain.Entities;

namespace ShoppingBasket.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(string? search = null, string? category = null)
    {
        IQueryable<Product> query = _db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Use case-insensitive LIKE queries so that partial inputs like "Cer" match "Ceramic Mug"
            var s = search.Trim();
            var pattern = $"%{s}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, pattern) ||
                EF.Functions.Like(p.SKU, pattern) ||
                (p.Description != null && EF.Functions.Like(p.Description!, pattern))
            );
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            var c = category.Trim();
            query = query.Where(p => p.Category == c);
        }

        var items = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.SKU, p.Name, p.Description ?? string.Empty, p.Price, p.ImageUrl, p.Category))
            .ToListAsync();
        return items;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? null : new ProductDto(p.Id, p.SKU, p.Name, p.Description ?? string.Empty, p.Price, p.ImageUrl, p.Category);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        // Ensure SKU uniqueness
        if (await _db.Products.AnyAsync(p => p.SKU == request.SKU))
            throw new InvalidOperationException($"Product with SKU {request.SKU} already exists.");

        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            Category = request.Category
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return new ProductDto(product.Id, product.SKU, product.Name, product.Description ?? string.Empty, product.Price, product.ImageUrl, product.Category);
    }
}
