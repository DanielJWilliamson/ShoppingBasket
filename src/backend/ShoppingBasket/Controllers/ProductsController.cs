using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShoppingBasket.Api.Hubs;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Application.Interfaces;

namespace ShoppingBasket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
    private readonly IHubContext<CatalogHub> _hub;

    public ProductsController(IProductService products, IHubContext<CatalogHub> hub)
    {
        _products = products;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Get([FromQuery] string? q, [FromQuery] string? category)
    {
        var list = await _products.GetProductsAsync(q, category);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var item = await _products.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    // Simple dev/admin endpoint to seed/add products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var created = await _products.CreateAsync(request);
        await _hub.Clients.All.SendAsync(CatalogHub.CatalogChanged, new { entity = "product", action = "created", id = created.Id });
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
