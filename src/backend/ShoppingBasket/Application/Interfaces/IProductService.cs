using ShoppingBasket.Application.DTOs;

namespace ShoppingBasket.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(string? search = null, string? category = null);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
}
