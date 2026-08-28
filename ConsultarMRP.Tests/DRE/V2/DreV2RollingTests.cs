using _2___Application._1_Services.DRE.V2;
using _2___Application._2_Dto_s.TotalizerClassification;
using Xunit;

namespace ConsultarMRP.Tests.DRE.V2;

public sealed class DreV2RollingTests
{
    [Fact]
    public void Uses_realized_through_last_available_month_and_future_budget()
    {
        var fixture = Configure(Enumerable.Range(1, 5), Enumerable.Range(1, 12));

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var row = Row(response, "GROSS_REVENUE");

        var expectedRolling = Enumerable.Range(1, 5)
            .Sum(month => Monthly(row, "realizado", month, DreV2Fixture.Year))
            + Enumerable.Range(6, 7)
                .Sum(month => Monthly(row, "orcado", month, DreV2Fixture.Year));
        var expectedBudget = Enumerable.Range(1, 12)
            .Sum(month => Monthly(row, "orcado", month, DreV2Fixture.Year));

        Assert.Equal(expectedRolling, row.Values["rolling"]["annual-rolling"]);
        Assert.Equal(expectedBudget, row.Values["orcado"]["annual-rolling"]);
        Assert.Equal(expectedRolling - expectedBudget,
            row.Values["variacao"]["annual-rolling"]);
    }

    [Fact]
    public void Uses_all_realized_months_when_the_year_is_fully_realized()
    {
        var fixture = Configure(Enumerable.Range(1, 12), Enumerable.Range(1, 12));
        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var row = Row(response, "GROSS_REVENUE");

        Assert.Equal(
            Enumerable.Range(1, 12).Sum(month => Monthly(row, "realizado", month, DreV2Fixture.Year)),
            row.Values["rolling"]["annual-rolling"]);
    }

    [Fact]
    public void Falls_back_to_budget_when_no_realized_period_exists()
    {
        var fixture = Configure(Array.Empty<int>(), Enumerable.Range(1, 12));
        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var row = Row(response, "GROSS_REVENUE");

        Assert.Equal(row.Values["orcado"]["annual-rolling"],
            row.Values["rolling"]["annual-rolling"]);
        Assert.Equal(0m, row.Values["variacao"]["annual-rolling"]);
    }

    [Fact]
    public void Treats_zero_as_realized_and_preserves_percentage_formulas()
    {
        var fixture = Configure(new[] { 1, 2, 3 }, Enumerable.Range(1, 12));
        var februaryBudget = fixture.Legacy.Orcado.Months.Single(month => month.DateMonth == 2);
        var taxesBudget = februaryBudget.Totalizer
            .SelectMany(totalizer => totalizer.Classifications)
            .Single(classification => classification.Name == "(-) Impostos e Contribuições");
        taxesBudget.Value = 999m;
        var response = DreV2Mapper.Map(fixture.Legacy, 2025);
        var taxes = Row(response, "TAXES_AND_CONTRIBUTIONS");

        Assert.Equal(0m, Monthly(taxes, "realizado", 2, 2025));
        Assert.NotEqual(Monthly(taxes, "orcado", 2, 2025),
            Monthly(taxes, "realizado", 2, 2025));
        Assert.Equal(
            Enumerable.Range(1, 3).Sum(month => Monthly(taxes, "realizado", month, 2025)) +
            Enumerable.Range(4, 9).Sum(month => Monthly(taxes, "orcado", month, 2025)),
            taxes.Values["rolling"]["annual-rolling"]);

        var annual = response.Data.Periods.Single(period => period.Type == "rolling");
        Assert.Equal(2025, annual.Year);
        Assert.Equal("2025", annual.Label);

        var grossProfit = Row(response, "GROSS_PROFIT");
        var netRevenue = Row(response, "NET_REVENUE");
        var grossMargin = Row(response, "GROSS_MARGIN_PERCENT");
        var expectedMargin = Math.Round(
            grossProfit.Values["rolling"]["annual-rolling"]!.Value /
            netRevenue.Values["rolling"]["annual-rolling"]!.Value * 100m,
            2);
        Assert.Equal(expectedMargin, grossMargin.Values["rolling"]["annual-rolling"]);
    }

    [Fact]
    public void Leaves_annual_budget_and_variation_null_when_budget_is_absent()
    {
        var fixture = Configure(Enumerable.Range(1, 5), Array.Empty<int>());
        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var row = Row(response, "GROSS_REVENUE");

        Assert.NotNull(row.Values["rolling"]["annual-rolling"]);
        Assert.Null(row.Values["orcado"]["annual-rolling"]);
        Assert.Null(row.Values["variacao"]["annual-rolling"]);
    }

    private static DreV2Fixture Configure(IEnumerable<int> realized, IEnumerable<int> budget)
    {
        var fixture = new DreV2Fixture();
        fixture.Legacy.Realizado = fixture.CreatePanel("realizado", 0, realized);
        fixture.Legacy.Orcado = fixture.CreatePanel("orcado", 1_000, budget);
        fixture.Legacy.Variacao = new PainelBalancoContabilRespone { Months = new() };
        return fixture;
    }

    private static decimal Monthly(
        _2___Application._2_Dto_s.DRE.V2.DreRowDto row,
        string scenario,
        int month,
        int year) =>
        row.Values[scenario][$"{year:D4}-{month:D2}"]!.Value;

    private static _2___Application._2_Dto_s.DRE.V2.DreRowDto Row(
        _2___Application._2_Dto_s.DRE.V2.DreV2Response response,
        string code) =>
        Assert.Single(response.Data.Rows, row => row.Code == code);
}
