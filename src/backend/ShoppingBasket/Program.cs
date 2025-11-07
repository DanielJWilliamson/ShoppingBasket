using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ShoppingBasket.Api.Hubs;
using ShoppingBasket.Application.Interfaces;
using ShoppingBasket.Application.Services;
using ShoppingBasket.Application.Validation;
using ShoppingBasket.Infrastructure;
using ShoppingBasket.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

builder.Services.AddHealthChecks();

// Persistence: SQLite file DB for demo
// Resolve connection string robustly: prefer ConnectionStrings:Default, fallback to sensible defaults
var configuredCs = builder.Configuration.GetConnectionString("Default");
var runningInContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
var connectionString = string.IsNullOrWhiteSpace(configuredCs)
    ? (runningInContainer ? "Data Source=/app/data/app.db" : "Data Source=app.db")
    : configuredCs!;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Application services
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Ensure database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
        // Bootstrap essential reference data for existing persisted DBs that predate HasData seeds
        if (!await db.Discounts.AnyAsync())
        {
            db.Discounts.AddRange(
                new ShoppingBasket.Domain.Entities.Discount { Code = "10percent", Percentage = 0.10m, IsStackable = true },
                new ShoppingBasket.Domain.Entities.Discount { Code = "20percent", Percentage = 0.20m, IsStackable = true },
                new ShoppingBasket.Domain.Entities.Discount { Code = "30percent", Percentage = 0.30m, IsStackable = true }
            );
            await db.SaveChangesAsync();
        }
        // Ensure promotional product exists even for older DBs
        if (!await db.Products.AnyAsync(p => p.SKU == "PROMO-001"))
        {
            db.Products.Add(new ShoppingBasket.Domain.Entities.Product
            {
                SKU = "PROMO-001",
                Name = "Wireless Earbuds (Promo)",
                Description = "Limited Offer - 30% Off",
                Price = 35.00m,
                ImageUrl = "/img/mug.jpg",
                Category = "Electronics",
                IsDiscountExempt = true
            });
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var msg = $"Failed to initialize SQLite database with connection string: '{connectionString}'.\n{ex.Message}";
        throw new InvalidOperationException(msg, ex);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In production, redirect HTTP to HTTPS if configured; skip in Development (e.g., local Docker)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Serve static UI (to be produced by the frontend build into wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// Image placeholder fallback: if a requested /img/* asset isn't present in wwwroot,
// return an inline SVG placeholder instead of a 404 to reduce browser console noise.
// This runs after static files: existing assets are served by UseStaticFiles; missing ones hit this endpoint.
// capture env for use in endpoint without adding extra usings
var _env = app.Environment;
app.MapMethods("/img/{**path}", new[] { "GET", "HEAD" }, (string path) =>
{
    // Resolve the physical path under wwwroot/img safely
    var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var imgDir = Path.Combine(webRoot, "img");
        var candidate = Path.GetFullPath(Path.Combine(imgDir, path ?? string.Empty));
        if (candidate.StartsWith(Path.GetFullPath(imgDir), StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(candidate))
        {
                var contentType = "image/jpeg"; // our seeded paths are .jpg
                return Results.File(candidate, contentType);
        }

        const string svg = """
<svg xmlns='http://www.w3.org/2000/svg' width='256' height='160'>
    <defs>
        <linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>
            <stop offset='0%' stop-color='#dbeafe'/>
            <stop offset='100%' stop-color='#bfdbfe'/>
        </linearGradient>
    </defs>
    <rect width='100%' height='100%' fill='url(#g)'/>
    <text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='#1e40af' font-size='20' font-family='system-ui,Segoe UI,Roboto'>No image</text>
    <rect x='0.5' y='0.5' width='255' height='159' fill='none' stroke='#93c5fd'/>
    <text x='6' y='18' fill='#1e3a8a' font-size='10' font-family='system-ui,Segoe UI,Roboto'>placeholder</text>
    Sorry, your browser does not support inline SVG.
</svg>
""";
        return Results.Text(svg, "image/svg+xml");
});

app.MapControllers();
app.MapHub<CatalogHub>("/hubs/catalog");
app.MapHealthChecks("/health");

// SPA fallback
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
