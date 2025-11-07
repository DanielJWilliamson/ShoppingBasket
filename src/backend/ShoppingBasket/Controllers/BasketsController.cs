using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShoppingBasket.Api.Hubs;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Application.Interfaces;

namespace ShoppingBasket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketsController : ControllerBase
{
    private readonly IBasketService _baskets;
    private readonly IHubContext<CatalogHub> _hub;

    public BasketsController(IBasketService baskets, IHubContext<CatalogHub> hub)
    {
        _baskets = baskets;
        _hub = hub;
    }

    [HttpGet("{basketId:int}")]
    public async Task<ActionResult<BasketDto>> Get(int basketId)
    {
        var basket = await _baskets.GetByIdAsync(basketId);
        return basket == null ? NotFound() : Ok(basket);
    }

    [HttpPost("create")]
    public async Task<ActionResult<BasketDto>> Create([FromQuery] string? customerId)
    {
        var basket = await _baskets.GetOrCreateAsync(null, customerId);
        return Ok(basket);
    }

    [HttpPost("{basketId:int}/items")]
    public async Task<ActionResult<BasketDto>> AddItem(int basketId, [FromBody] AddToBasketRequest request)
    {
        var basket = await _baskets.AddItemAsync(basketId, request);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "basket", action = "updated", id = basket.Id });
        return Ok(basket);
    }

    [HttpPut("{basketId:int}/items")]
    public async Task<ActionResult<BasketDto>> UpdateItem(int basketId, [FromBody] UpdateBasketItemRequest request)
    {
        var basket = await _baskets.UpdateItemAsync(basketId, request);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "basket", action = "updated", id = basket.Id });
        return Ok(basket);
    }

    [HttpDelete("{basketId:int}/items/{productId:int}")]
    public async Task<ActionResult<BasketDto>> RemoveItem(int basketId, int productId)
    {
        var basket = await _baskets.RemoveItemAsync(basketId, productId);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "basket", action = "updated", id = basket.Id });
        return Ok(basket);
    }

    [HttpPost("{basketId:int}/vouchers/{code}")]
    public async Task<ActionResult<BasketDto>> ApplyVoucher(int basketId, string code)
    {
        var basket = await _baskets.ApplyVoucherAsync(basketId, code);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "basket", action = "updated", id = basket.Id });
        return Ok(basket);
    }

    [HttpDelete("{basketId:int}/vouchers/{code}")]
    public async Task<ActionResult<BasketDto>> RemoveVoucher(int basketId, string code)
    {
        var basket = await _baskets.RemoveVoucherAsync(basketId, code);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "basket", action = "updated", id = basket.Id });
        return Ok(basket);
    }

    [HttpDelete("{basketId:int}")]
    public async Task<IActionResult> Clear(int basketId)
    {
        await _baskets.ClearAsync(basketId);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "basket", action = "cleared", id = basketId });
        return NoContent();
    }
}
