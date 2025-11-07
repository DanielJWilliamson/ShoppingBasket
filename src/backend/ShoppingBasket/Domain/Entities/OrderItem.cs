namespace ShoppingBasket.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? ProductId { get; set; } // optional if deleted later
    public string SKU { get; set; } = string.Empty; // snapshot at purchase time
    public string Name { get; set; } = string.Empty; // snapshot at purchase time
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public Order? Order { get; set; }
}
