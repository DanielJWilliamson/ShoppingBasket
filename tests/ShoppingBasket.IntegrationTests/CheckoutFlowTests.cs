using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Infrastructure;

namespace ShoppingBasket.IntegrationTests;

public class CheckoutFlowTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;

    public CheckoutFlowTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task EndToEnd_Checkout_CreatesOrder_And_EmptiesBasket()
    {
        // create basket
        var basket = await _client.PostAsync("/api/baskets/create?customerId=tester", null);
        basket.EnsureSuccessStatusCode();
        var b = await basket.Content.ReadFromJsonAsync<BasketDto>();
        b.Should().NotBeNull();
        var basketId = b!.Id;

        // get products
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        products!.Count.Should().BeGreaterThan(0);
        var first = products![0];

        // add item
        var add = await _client.PostAsJsonAsync($"/api/baskets/{basketId}/items", new AddToBasketRequest(first.Id, 1));
        add.EnsureSuccessStatusCode();

        // checkout
    var orderResp = await _client.PostAsJsonAsync($"/api/orders/checkout/{basketId}", new CheckoutRequest("tester", true, "GB"));
        orderResp.EnsureSuccessStatusCode();
        var order = await orderResp.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Items.Count.Should().Be(1);

        // basket should be empty now
        var basketAfter = await _client.GetFromJsonAsync<BasketDto>($"/api/baskets/{basketId}");
        basketAfter!.Items.Should().BeEmpty();
      }
}
