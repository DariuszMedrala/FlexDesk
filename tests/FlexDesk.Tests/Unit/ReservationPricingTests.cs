using FlexDesk.Application.Reservations;

namespace FlexDesk.Tests.Unit;

public class ReservationPricingTests
{
    private static readonly DateTime Start = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CalculateTotal_BelowDiscountThreshold_ChargesFullPrice()
    {
        var total = ReservationPricing.CalculateTotal(15m, Start, Start.AddHours(4));

        Assert.Equal(60m, total);
    }

    [Fact]
    public void CalculateTotal_JustBelowThreshold_DoesNotDiscount()
    {
        var total = ReservationPricing.CalculateTotal(60m, Start, Start.AddHours(7));

        Assert.Equal(420m, total);
    }

    [Fact]
    public void CalculateTotal_AtThreshold_AppliesTenPercentDiscount()
    {
        var total = ReservationPricing.CalculateTotal(15m, Start, Start.AddHours(8));

        Assert.Equal(108m, total);
    }

    [Fact]
    public void CalculateTotal_AboveThreshold_AppliesTenPercentDiscount()
    {
        var total = ReservationPricing.CalculateTotal(60m, Start, Start.AddHours(10));

        Assert.Equal(540m, total);
    }

    [Fact]
    public void CalculateTotal_RoundsToTwoDecimals()
    {
        var total = ReservationPricing.CalculateTotal(33.33m, Start, Start.AddHours(3));

        Assert.Equal(99.99m, total);
    }
}
