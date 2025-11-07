using ShoppingBasket.Application.DTOs;

namespace ShoppingBasket.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(int basketId, CheckoutRequest request);
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? customerId = null);
    Task<OrderDto?> GetByIdAsync(int id);
}
