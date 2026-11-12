namespace FlexDesk.Application.Reservations;

public static class ReservationPricing
{
    public const int DiscountThresholdHours = 8;
    public const decimal LongStayDiscountRate = 0.10m;

    public static decimal CalculateTotal(decimal pricePerHour, DateTime startTime, DateTime endTime)
    {
        var hours = (decimal)(endTime - startTime).TotalHours;
        var total = pricePerHour * hours;

        if (hours >= DiscountThresholdHours)
        {
            total *= 1 - LongStayDiscountRate;
        }

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
