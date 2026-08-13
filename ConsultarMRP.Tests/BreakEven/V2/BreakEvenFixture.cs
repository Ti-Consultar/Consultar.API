using _2___Application._1_Services.BreakEven.V2;
using _2___Application._2_Dto_s.DRE.V2;

namespace ConsultarMRP.Tests.BreakEven.V2;

internal static class BreakEvenFixture
{
    private static readonly IReadOnlyDictionary<string, decimal> Values =
        new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["GROSS_REVENUE"] = 18_000m,
            ["PRODUCT_SALES"] = 10_000m,
            ["MERCHANDISE_SALES"] = 5_000m,
            ["SERVICE_REVENUE"] = 2_000m,
            ["RENTAL_REVENUE"] = 1_000m,
            ["GROSS_REVENUE_DEDUCTIONS"] = -800m,
            ["SALES_RETURNS"] = -500m,
            ["SALES_ALLOWANCES"] = -200m,
            ["TAXES_AND_CONTRIBUTIONS"] = -100m,
            ["NET_REVENUE"] = 17_200m,
            ["COST_OF_GOODS"] = -15_000m,
            ["COST_OF_SERVICES"] = -500m,
            ["VARIABLE_COSTS"] = 0m,
            ["GROSS_PROFIT"] = 1_700m,
            ["GROSS_MARGIN_PERCENT"] = 9.88m,
            ["VARIABLE_EXPENSES"] = -100m,
            ["CONTRIBUTION_MARGIN"] = 1_600m,
            ["CONTRIBUTION_MARGIN_PERCENT"] = 9.30m,
            ["OPERATING_EXPENSES"] = -1_900m,
            ["DEPRECIATION_EXPENSE"] = -500m,
            ["SALES_EXPENSES"] = -80m,
            ["PERSONNEL_EXPENSES"] = -120m,
            ["ADMINISTRATIVE_GENERAL_EXPENSES"] = -1_200m,
            ["OTHER_OPERATING_RESULTS"] = 50m,
            ["OPERATING_PROFIT"] = -250m,
            ["OPERATING_MARGIN_PERCENT"] = -1.45m,
            ["OTHER_RESULTS"] = 2m,
            ["OTHER_INCOME"] = 12m,
            ["OTHER_EXPENSES"] = -10m,
            ["CAPITAL_GAINS_AND_LOSSES"] = 0m,
            ["OTHER_NON_OPERATING_INCOME"] = 0m,
            ["EBIT"] = -248m,
            ["EBIT_MARGIN_PERCENT"] = -1.44m,
            ["FINANCIAL_RESULT"] = 50m,
            ["FINANCIAL_INCOME"] = 100m,
            ["FINANCIAL_EXPENSES"] = -50m,
            ["EBT"] = -198m,
            ["EBT_MARGIN_PERCENT"] = -1.15m
        };

    public static IReadOnlyList<BreakEvenSourceRow> CreateRows(
        IReadOnlyDictionary<string, decimal>? overrides = null)
    {
        var values = new Dictionary<string, decimal>(Values, StringComparer.Ordinal);
        if (overrides is not null)
        {
            foreach (var item in overrides)
                values[item.Key] = item.Value;
        }

        return BreakEvenRowCatalog.All.Select(rule =>
        {
            var isPercentage = rule.Code.EndsWith("_PERCENT", StringComparison.Ordinal);
            var isCalculated = rule.Behavior == BreakEvenRowCatalog.Calculated;
            return new BreakEvenSourceRow(
                rule.Code,
                rule.Code == "SALES_EXPENSES" ? "Despesas com Vendas" : rule.Code,
                isCalculated ? "subtotal" : "classification",
                isPercentage ? "percentage" : "currency",
                rule.DisplayOrder,
                rule.Level ?? 0,
                rule.OverrideHierarchy ? rule.ParentCode : null,
                isCalculated,
                new DreRowSourceDto
                {
                    SourceType = isCalculated ? "totalizer" : "classification",
                    ClassificationId = isCalculated ? null : rule.DisplayOrder
                },
                values[rule.Code]);
        }).ToArray();
    }
}
