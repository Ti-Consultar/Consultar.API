using _2___Application._1_Services.BreakEven.V2;
using _2___Application._2_Dto_s.BreakEven.V2;
using System.Text.Json;
using Xunit;

namespace ConsultarMRP.Tests.BreakEven.V2;

public sealed class BreakEvenCalculatorTests
{
    private readonly BreakEvenCalculator _calculator = new();

    [Fact]
    public void Reproduces_excel_fixture_and_rebuilds_zero_result_with_factor_zero()
    {
        var result = _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("SALES_EXPENSES", -0.05m) });

        Assert.Equal(18_000m, result.Summary.ProjectedGrossOperatingRevenue);
        Assert.Equal(1_600m, result.Summary.ContributionMargin);
        Assert.Equal(0.088889m, result.Summary.ContributionMarginPercentage);
        Assert.Equal(-1_794m, result.Summary.FixedResult);
        Assert.Equal(1_794m, result.Summary.FixedAmountToCover);
        AssertClose(20_182.50m, result.Summary.BaseBreakEven);
        AssertClose(20_182.50m, result.Summary.FinalBreakEven);

        var productSales = Row(result, "PRODUCT_SALES");
        AssertClose(11_212.50m, productSales.BreakEvenValue);
        Assert.Equal(BreakEvenRowCatalog.Variable, productSales.Behavior);

        var salesExpenses = Row(result, "SALES_EXPENSES");
        Assert.Equal(-80m, salesExpenses.BaseValue);
        Assert.Equal(-76m, salesExpenses.ProjectedValue);
        Assert.Equal(-76m, salesExpenses.BreakEvenValue);
        Assert.Equal(BreakEvenRowCatalog.Fixed, salesExpenses.Behavior);

        Assert.Equal(-1_896m, Row(result, "OPERATING_EXPENSES").ProjectedValue);
        AssertClose(0m, Row(result, "EBT").BreakEvenValue);
    }

    [Fact]
    public void Applies_factor_once_to_base_break_even_and_produces_profit_above_equilibrium()
    {
        var result = _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0.10m,
            new[] { Simulation("SALES_EXPENSES", -0.05m) });

        AssertClose(20_182.50m, result.Summary.BaseBreakEven);
        AssertClose(22_200.75m, result.Summary.FinalBreakEven);
        AssertClose(22_200.75m, Row(result, "GROSS_REVENUE").BreakEvenValue);
        AssertClose(179.4m, Row(result, "EBT").BreakEvenValue);
        Assert.Equal(-76m, Row(result, "SALES_EXPENSES").BreakEvenValue);
    }

    [Theory]
    [InlineData("0", "-15000")]
    [InlineData("0.10", "-16500")]
    [InlineData("-0.10", "-13500")]
    [InlineData("-1", "0")]
    public void Applies_positive_zero_negative_and_minus_one_simulations(
        string percentageText,
        string expectedText)
    {
        var percentage = decimal.Parse(percentageText, System.Globalization.CultureInfo.InvariantCulture);
        var expected = decimal.Parse(expectedText, System.Globalization.CultureInfo.InvariantCulture);

        var result = _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("COST_OF_GOODS", percentage) });

        Assert.Equal(expected, Row(result, "COST_OF_GOODS").ProjectedValue);
        Assert.Equal(-15_000m, Row(result, "COST_OF_GOODS").BaseValue);
    }

    [Fact]
    public void Recalculates_dependent_totalizers_after_simulation()
    {
        var result = _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("SALES_RETURNS", -1m) });

        Assert.Equal(0m, Row(result, "SALES_RETURNS").ProjectedValue);
        Assert.Equal(-300m, Row(result, "GROSS_REVENUE_DEDUCTIONS").ProjectedValue);
        Assert.Equal(17_700m, Row(result, "NET_REVENUE").ProjectedValue);
    }

    [Fact]
    public void Includes_capital_gains_even_when_the_optional_other_results_totalizer_is_absent()
    {
        var rows = BreakEvenFixture.CreateRows(new Dictionary<string, decimal>
            {
                ["CAPITAL_GAINS_AND_LOSSES"] = 10m
            })
            .Where(row => row.Code is not "OTHER_RESULTS" and not "OTHER_INCOME" and not "OTHER_EXPENSES")
            .ToArray();

        var result = _calculator.Calculate(rows, 0m);

        Assert.Equal(-1_790m, result.Summary.FixedResult);
    }

    [Fact]
    public void Formats_monetary_response_values_with_two_decimal_places_and_removes_tiny_residues()
    {
        var rows = BreakEvenFixture.CreateRows(new Dictionary<string, decimal>
        {
            ["PRODUCT_SALES"] = 635684.0159465708m,
            ["MERCHANDISE_SALES"] = 32738.51624748491m,
            ["CAPITAL_GAINS_AND_LOSSES"] = 0.00000000000000000000003m,
            ["OTHER_NON_OPERATING_INCOME"] = 0.00000000000000000000000001m
        });

        var result = _calculator.Calculate(rows, 0m);

        Assert.Equal(635684.02m, Row(result, "PRODUCT_SALES").BaseValue);
        Assert.Equal(635684.02m, Row(result, "PRODUCT_SALES").ProjectedValue);
        Assert.Equal(32738.52m, Row(result, "MERCHANDISE_SALES").BaseValue);
        Assert.Equal(0.00m, Row(result, "CAPITAL_GAINS_AND_LOSSES").BaseValue);
        Assert.Equal(0.00m, Row(result, "OTHER_NON_OPERATING_INCOME").ProjectedValue);

        var json = JsonSerializer.Serialize(new
        {
            amount = Row(result, "PRODUCT_SALES").BaseValue,
            secondAmount = Row(result, "MERCHANDISE_SALES").BaseValue,
            tinyAmount = Row(result, "CAPITAL_GAINS_AND_LOSSES").BaseValue
        });

        Assert.Contains("\"amount\":635684.02", json);
        Assert.Contains("\"secondAmount\":32738.52", json);
        Assert.Contains("\"tinyAmount\":0.00", json);
    }

    [Fact]
    public void Returns_all_percentage_rows_in_decimal_scale_with_at_most_six_decimal_places()
    {
        var rows = BreakEvenFixture.CreateRows(new Dictionary<string, decimal>
        {
            ["VARIABLE_EXPENSES"] = 5_491.82548m
        });

        var result = _calculator.Calculate(rows, 0m);
        var contributionMarginPercentage = Row(result, "CONTRIBUTION_MARGIN_PERCENT");

        Assert.Equal(0.399546m, result.Summary.ContributionMarginPercentage);
        Assert.Equal(0.093m, contributionMarginPercentage.BaseValue);
        Assert.Equal(0.399546m, contributionMarginPercentage.ProjectedValue);
        Assert.Equal(0.399546m, contributionMarginPercentage.BreakEvenValue);
    }

    [Fact]
    public void Returns_simulation_sign_rule_for_simulatable_and_read_only_rows()
    {
        var result = _calculator.Calculate(BreakEvenFixture.CreateRows(), 0m);
        var freeRows = new[]
        {
            "OTHER_OPERATING_RESULTS",
            "OTHER_INCOME",
            "CAPITAL_GAINS_AND_LOSSES",
            "FINANCIAL_INCOME"
        };

        Assert.All(
            result.Rows.Where(row => freeRows.Contains(row.Code)),
            row => Assert.Equal(BreakEvenRowCatalog.FreeSimulationSign, row.SimulationSignRule));
        Assert.All(
            result.Rows.Where(row => row.CanSimulate && !freeRows.Contains(row.Code)),
            row => Assert.Equal(BreakEvenRowCatalog.NegativeSimulationSign, row.SimulationSignRule));
        Assert.All(
            result.Rows.Where(row => !row.CanSimulate),
            row => Assert.Null(row.SimulationSignRule));

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Assert.Contains(
            "\"simulationSignRule\":\"negative\"",
            JsonSerializer.Serialize(Row(result, "ADMINISTRATIVE_GENERAL_EXPENSES"), jsonOptions));
        Assert.Contains(
            "\"simulationSignRule\":\"free\"",
            JsonSerializer.Serialize(Row(result, "FINANCIAL_INCOME"), jsonOptions));
        Assert.Contains(
            "\"simulationSignRule\":null",
            JsonSerializer.Serialize(Row(result, "GROSS_REVENUE"), jsonOptions));
    }

    [Fact]
    public void Rejects_simulation_below_minus_one() =>
        Assert.Throws<BreakEvenValidationException>(() => _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("COST_OF_GOODS", -1.01m) }));

    [Fact]
    public void Rejects_negative_factor() =>
        Assert.Throws<BreakEvenValidationException>(() => _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            -0.01m));

    [Fact]
    public void Rejects_duplicate_simulations() =>
        Assert.Throws<BreakEvenValidationException>(() => _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[]
            {
                Simulation("SALES_EXPENSES", -0.05m),
                Simulation("SALES_EXPENSES", 0.10m)
            }));

    [Fact]
    public void Rejects_non_simulatable_and_unknown_rows()
    {
        Assert.Throws<BreakEvenValidationException>(() => _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("PRODUCT_SALES", 0.10m) }));
        Assert.Throws<BreakEvenValidationException>(() => _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("UNKNOWN", 0.10m) }));
    }

    [Fact]
    public void Rejects_zero_gross_revenue()
    {
        var rows = BreakEvenFixture.CreateRows(new Dictionary<string, decimal>
        {
            ["PRODUCT_SALES"] = 0m,
            ["MERCHANDISE_SALES"] = 0m,
            ["SERVICE_REVENUE"] = 0m,
            ["RENTAL_REVENUE"] = 0m
        });

        Assert.Throws<BreakEvenCalculationException>(() => _calculator.Calculate(rows, 0m));
    }

    [Theory]
    [InlineData("16")]
    [InlineData("17")]
    public void Rejects_zero_or_negative_contribution_margin(string percentageText)
    {
        var percentage = decimal.Parse(percentageText, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Throws<BreakEvenCalculationException>(() => _calculator.Calculate(
            BreakEvenFixture.CreateRows(),
            0m,
            new[] { Simulation("VARIABLE_EXPENSES", percentage) }));
    }

    private static BreakEvenLineSimulationDto Simulation(string code, decimal percentage) =>
        new() { RowCode = code, Percentage = percentage };

    private static BreakEvenRowDto Row(BreakEvenCalculationResult result, string code) =>
        result.Rows.Single(row => row.Code == code);

    private static void AssertClose(decimal expected, decimal actual, decimal tolerance = 0.0000001m) =>
        Assert.InRange(actual, expected - tolerance, expected + tolerance);
}
