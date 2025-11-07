using System.Collections.ObjectModel;

namespace ShoppingBasket.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty; // unique, human-friendly
    public string Currency { get; set; } = "GBP";
    public string CountryCode { get; set; } = "GB";
    public decimal Subtotal { get; set; }
    public decimal DiscountedSubtotal { get; set; }
    public decimal Vat { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "Confirmed";

    public ICollection<OrderItem> Items { get; set; } = new Collection<OrderItem>();
}
