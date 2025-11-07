using System.Collections.ObjectModel;

namespace ShoppingBasket.Domain.Entities;

public class Basket
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Currency { get; set; } = "GBP";
    public string CountryCode { get; set; } = "GB";

    public ICollection<BasketItem> Items { get; set; } = new Collection<BasketItem>();

    public string AppliedDiscountCodesCsv { get; set; } = string.Empty;
}
