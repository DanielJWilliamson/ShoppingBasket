using ShoppingBasket.Application.DTOs;

namespace ShoppingBasket.Application.Interfaces;

public interface IBasketService
{
    Task<BasketDto> GetOrCreateAsync(int? basketId, string? customerId);
    Task<BasketDto?> GetByIdAsync(int basketId);
    Task<BasketDto> AddItemAsync(int basketId, AddToBasketRequest request);
    Task<BasketDto> UpdateItemAsync(int basketId, UpdateBasketItemRequest request);
    Task<BasketDto> RemoveItemAsync(int basketId, int productId);
    Task<BasketDto> ApplyVoucherAsync(int basketId, string code);
    Task<BasketDto> RemoveVoucherAsync(int basketId, string code);
    Task ClearAsync(int basketId);
}
