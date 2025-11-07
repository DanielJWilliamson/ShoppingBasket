using FluentAssertions;
using ShoppingBasket.Application.Services;

namespace ShoppingBasket.UnitTests;

public class PricingServiceTests
{
    public static IEnumerable<object[]> Cases => new List<object[]>
    {
        new object[] { new decimal[] { 10m }, new int[] { 1 }, true, 0.2m, 10.00m, 2.00m, 12.00m },
        new object[] { new decimal[] { 12.99m, 7.49m }, new int[] { 1, 2 }, true, 0.2m, 27.97m, 5.59m, 33.56m }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void CalculateTotals_MatchesExpected(decimal[] prices, int[] qtys, bool includeVat, decimal vatRate,
        decimal expectedSubtotal, decimal expectedVat, decimal expectedTotal)
    {
        var svc = new PricingService();
        var lines = prices.Select((p, i) => (p, qtys[i]));
        var (subtotal, vat, total) = svc.CalculateTotals(lines, includeVat, vatRate);

        subtotal.Should().Be(expectedSubtotal);
        vat.Should().Be(expectedVat);
        total.Should().Be(expectedTotal);
    }
}
