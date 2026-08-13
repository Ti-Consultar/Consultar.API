namespace _2___Application._1_Services.BreakEven.V2;

internal static class BreakEvenResponsePrecision
{
    public static decimal Money(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero) + 0.00m;

    public static decimal Percentage(decimal value) =>
        decimal.Round(value, 6, MidpointRounding.AwayFromZero);

    public static decimal PercentageFromHundredScale(decimal value) =>
        Percentage(value / 100m);
}
