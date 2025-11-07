namespace ShoppingBasket.Domain.Entities;

public class Discount
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal? Percentage { get; set; }
    public decimal? FixedAmount { get; set; }
    public bool IsStackable { get; set; }

    public string? AllowedSkusCsv { get; set; }
    public string? ExcludedSkusCsv { get; set; }
}
