using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShoppingBasket.Api.Hubs;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Application.Interfaces;

namespace ShoppingBasket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly IHubContext<CatalogHub> _hub;

    public OrdersController(IOrderService orders, IHubContext<CatalogHub> hub)
    {
        _orders = orders;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Get([FromQuery] string? customerId)
    {
        var list = await _orders.GetOrdersAsync(customerId);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var item = await _orders.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("checkout/{basketId:int}")]
    public async Task<ActionResult<OrderDto>> Checkout(int basketId, [FromBody] CheckoutRequest request)
    {
        var order = await _orders.CheckoutAsync(basketId, request);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "order", action = "created", id = order.Id });
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}
