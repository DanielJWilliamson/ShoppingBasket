namespace ShoppingBasket.Application.DTOs;

public record ProductDto(int Id, string SKU, string Name, string Description, decimal Price, string? ImageUrl, string? Category);
public record CreateProductRequest(string SKU, string Name, string Description, decimal Price, string? ImageUrl, string? Category);
