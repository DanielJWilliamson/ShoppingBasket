using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingBasket.Application.DTOs;
using ShoppingBasket.Infrastructure;

namespace ShoppingBasket.IntegrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace AppDbContext with in-memory Sqlite for tests
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));

            // Build provider to create database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

public class ProductsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;

    public ProductsTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Products_Returns_Seeded_List()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        items.Should().NotBeNull();
        items!.Count.Should().BeGreaterThan(0);
    }
}
