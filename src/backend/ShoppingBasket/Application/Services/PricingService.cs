using ShoppingBasket.Application.Interfaces;

namespace ShoppingBasket.Application.Services;

public interface IPricingService
{
    (decimal Subtotal, decimal Vat, decimal Total) CalculateTotals(IEnumerable<(decimal unitPrice, int quantity)> lines, bool includeVat, decimal vatRate = 0.2m);
}

public class PricingService : IPricingService
{
    public (decimal Subtotal, decimal Vat, decimal Total) CalculateTotals(IEnumerable<(decimal unitPrice, int quantity)> lines, bool includeVat, decimal vatRate = 0.2m)
    {
        // Round monetary amounts to 2 decimals using MidpointRounding.ToEven for financial accuracy
        decimal subtotal = lines.Sum(l => Math.Round(l.unitPrice, 2, MidpointRounding.ToEven) * l.quantity);
        subtotal = Math.Round(subtotal, 2, MidpointRounding.ToEven);
        decimal vat = includeVat ? Math.Round(subtotal * vatRate, 2, MidpointRounding.ToEven) : 0m;
        decimal total = subtotal + vat;
        return (subtotal, vat, total);
    }
}
