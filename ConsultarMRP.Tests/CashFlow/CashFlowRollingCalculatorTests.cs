using _2___Application._1_Services.CashFlow;
using _2___Application._2_Dto_s.CashFlow;
using Xunit;

namespace ConsultarMRP.Tests.CashFlow;

public sealed class CashFlowRollingCalculatorTests
{
    [Theory]
    [InlineData(5, 12)]
    [InlineData(5, 8)]
    [InlineData(12, 12)]
    [InlineData(5, 0)]
    [InlineData(0, 12)]
    public void Calculates_official_temporal_scenarios(int realizedThrough, int budgetThrough)
    {
        var realized = Enumerable.Range(1, realizedThrough)
            .Select(month => Month(month, month * 10m))
            .ToArray();
        var budget = Enumerable.Range(1, budgetThrough)
            .Select(month => Month(month, 100m + month))
            .ToArray();

        var annual = CashFlowRollingCalculator.Calculate(realized, budget, 2026);
        var rolling = Value(annual, "rolling");
        var annualBudget = Value(annual, "orcado");
        var variation = Value(annual, "variacao");
        var expectedRolling = Enumerable.Range(1, realizedThrough).Sum(month => month * 10m)
            + Enumerable.Range(realizedThrough + 1, Math.Max(0, budgetThrough - realizedThrough))
                .Sum(month => 100m + month);
        var expectedBudget = Enumerable.Range(1, budgetThrough).Sum(month => 100m + month);

        Assert.Equal(expectedRolling, rolling.LucroOperacionalLiquido);
        Assert.Equal(expectedBudget, annualBudget.LucroOperacionalLiquido);
        Assert.Equal(expectedRolling - expectedBudget, variation.LucroOperacionalLiquido);
    }

    [Fact]
    public void Recognizes_zero_and_negative_realized_values_as_existing_periods()
    {
        var realized = new[]
        {
            Month(1, 100m),
            Month(2, 0m),
            Month(3, -200m)
        };
        var budget = Enumerable.Range(1, 5).Select(month => Month(month, 10m)).ToArray();

        var rolling = Value(CashFlowRollingCalculator.Calculate(realized, budget, 2026), "rolling");

        Assert.Equal(-80m, rolling.LucroOperacionalLiquido);
    }

    [Fact]
    public void Keeps_opening_and_closing_balances_non_additive()
    {
        var realized = new[]
        {
            Month(1, 10m, opening: 1_000m, closing: 1_010m),
            Month(2, 20m, opening: 1_010m, closing: 1_030m)
        };
        var budget = new[]
        {
            Month(1, 100m, opening: 900m, closing: 1_000m),
            Month(2, 100m, opening: 1_000m, closing: 1_100m),
            Month(3, 100m, opening: 1_100m, closing: 1_200m)
        };

        var annual = CashFlowRollingCalculator.Calculate(realized, budget, 2025);
        var rolling = Value(annual, "rolling");
        var annualBudget = Value(annual, "orcado");

        Assert.Equal(1_000m, rolling.DisponibilidadeInicioDoPeriodo);
        Assert.Equal(1_200m, rolling.DisponibilidadeFinalDoPeriodo);
        Assert.Equal(900m, annualBudget.DisponibilidadeInicioDoPeriodo);
        Assert.Equal(1_200m, annualBudget.DisponibilidadeFinalDoPeriodo);
        Assert.Equal(2025, annual.Year);
        Assert.Equal("rolling", annual.Type);
        Assert.Equal(new[] { "orcado", "rolling", "variacao" },
            annual.Columns.Select(column => column.Key));
        Assert.All(annual.Columns, column =>
        {
            Assert.Equal(2025, column.Value.Year);
            Assert.Equal("2025", column.Value.Name);
            Assert.Equal("rolling", column.Value.PeriodType);
            Assert.Equal(CashFlowRollingCalculator.AnnualPeriodNumber, column.Value.DateMonth);
        });
    }

    private static CashFlowResponseDto Month(
        int month,
        decimal value,
        decimal opening = 0m,
        decimal closing = 0m) =>
        new()
        {
            Name = month.ToString(),
            DateMonth = month,
            LucroOperacionalLiquido = value,
            FluxoDeCaixaOperacional = value,
            FluxoDeCaixaLivre = value,
            FluxoDeCaixaDaEmpresa = value,
            DisponibilidadeInicioDoPeriodo = opening,
            DisponibilidadeFinalDoPeriodo = closing
        };

    private static CashFlowResponseDto Value(CashFlowAnnualGroupDto annual, string key) =>
        Assert.Single(annual.Columns, column => column.Key == key).Value;
}
