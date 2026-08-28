using _2___Application._1_Services.FinancialReports;
using Xunit;

namespace ConsultarMRP.Tests.FinancialReports;

public sealed class RollingPeriodSelectorTests
{
    [Theory]
    [MemberData(nameof(Scenarios))]
    public void Selects_sources_from_period_existence(
        int[] realized,
        int[] budget,
        (int Month, RollingPeriodSource Source)[] expected)
    {
        var result = RollingPeriodSelector.Select(realized, budget);

        Assert.Equal(expected,
            result.Select(item => (item.Month, item.Source)).ToArray());
    }

    public static IEnumerable<object[]> Scenarios()
    {
        yield return new object[]
        {
            Enumerable.Range(1, 5).ToArray(),
            Enumerable.Range(1, 12).ToArray(),
            Enumerable.Range(1, 12)
                .Select(month => (month, month <= 5 ? RollingPeriodSource.Realized : RollingPeriodSource.Budget))
                .ToArray()
        };
        yield return new object[]
        {
            Enumerable.Range(1, 5).ToArray(),
            Enumerable.Range(1, 8).ToArray(),
            Enumerable.Range(1, 8)
                .Select(month => (month, month <= 5 ? RollingPeriodSource.Realized : RollingPeriodSource.Budget))
                .ToArray()
        };
        yield return new object[]
        {
            Enumerable.Range(1, 12).ToArray(),
            Enumerable.Range(1, 12).ToArray(),
            Enumerable.Range(1, 12).Select(month => (month, RollingPeriodSource.Realized)).ToArray()
        };
        yield return new object[]
        {
            Enumerable.Range(1, 5).ToArray(),
            Array.Empty<int>(),
            Enumerable.Range(1, 5).Select(month => (month, RollingPeriodSource.Realized)).ToArray()
        };
        yield return new object[]
        {
            Array.Empty<int>(),
            Enumerable.Range(1, 12).ToArray(),
            Enumerable.Range(1, 12).Select(month => (month, RollingPeriodSource.Budget)).ToArray()
        };
        yield return new object[]
        {
            new[] { 1, 2, 3, 4, 5 },
            new[] { 6, 8 },
            new[]
            {
                (1, RollingPeriodSource.Realized),
                (2, RollingPeriodSource.Realized),
                (3, RollingPeriodSource.Realized),
                (4, RollingPeriodSource.Realized),
                (5, RollingPeriodSource.Realized),
                (6, RollingPeriodSource.Budget),
                (8, RollingPeriodSource.Budget)
            }
        };
    }
}
