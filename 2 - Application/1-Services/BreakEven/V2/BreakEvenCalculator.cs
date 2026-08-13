using _2___Application._2_Dto_s.BreakEven.V2;

namespace _2___Application._1_Services.BreakEven.V2;

public sealed record BreakEvenCalculationResult(
    BreakEvenSummaryDto Summary,
    IReadOnlyList<BreakEvenRowDto> Rows);

public sealed class BreakEvenCalculator
{
    private static readonly string[] RequiredLeafCodes =
    {
        "PRODUCT_SALES", "MERCHANDISE_SALES", "SERVICE_REVENUE", "RENTAL_REVENUE",
        "SALES_RETURNS", "SALES_ALLOWANCES", "TAXES_AND_CONTRIBUTIONS",
        "COST_OF_GOODS", "COST_OF_SERVICES", "VARIABLE_EXPENSES",
        "DEPRECIATION_EXPENSE", "SALES_EXPENSES", "PERSONNEL_EXPENSES",
        "ADMINISTRATIVE_GENERAL_EXPENSES", "OTHER_OPERATING_RESULTS",
        "CAPITAL_GAINS_AND_LOSSES", "OTHER_NON_OPERATING_INCOME",
        "FINANCIAL_INCOME", "FINANCIAL_EXPENSES"
    };

    private static readonly string[] OperatingExpenseCodes =
    {
        "DEPRECIATION_EXPENSE", "SALES_EXPENSES", "PERSONNEL_EXPENSES",
        "ADMINISTRATIVE_GENERAL_EXPENSES"
    };

    public BreakEvenCalculationResult Calculate(
        IReadOnlyList<BreakEvenSourceRow> sourceRows,
        decimal factor,
        IReadOnlyList<BreakEvenLineSimulationDto>? simulations = null)
    {
        ArgumentNullException.ThrowIfNull(sourceRows);
        if (factor < 0m)
            throw new BreakEvenValidationException("factor deve ser maior ou igual a zero.");

        var sourceByCode = sourceRows
            .Where(row => BreakEvenRowCatalog.ByCode.ContainsKey(row.Code))
            .GroupBy(row => row.Code, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count() == 1
                    ? group.Single()
                    : throw new BreakEvenCalculationException(
                        $"A DRE contém mais de uma linha com o código {group.Key}."),
                StringComparer.Ordinal);

        var missing = RequiredLeafCodes.Where(code => !sourceByCode.ContainsKey(code)).ToArray();
        if (missing.Length > 0)
        {
            throw new BreakEvenCalculationException(
                $"A DRE não possui todas as classificações necessárias ao PE: {string.Join(", ", missing)}.");
        }

        var percentages = ValidateSimulations(simulations, sourceByCode);
        var projected = sourceByCode.ToDictionary(
            item => item.Key,
            item => item.Value.Value,
            StringComparer.Ordinal);

        foreach (var rule in BreakEvenRowCatalog.All.Where(rule => rule.Behavior == BreakEvenRowCatalog.Calculated))
            projected.TryAdd(rule.Code, 0m);

        foreach (var (rowCode, percentage) in percentages)
            projected[rowCode] = sourceByCode[rowCode].Value * (1m + percentage);

        Recalculate(projected, contributionMarginOnGrossRevenue: true);

        var grossRevenue = Get(projected, "GROSS_REVENUE");
        if (grossRevenue == 0m)
        {
            throw new BreakEvenCalculationException(
                "A Receita Operacional Bruta projetada é zero; não é possível calcular o PE.");
        }

        var contributionMargin = Get(projected, "CONTRIBUTION_MARGIN");
        var contributionMarginPercentage = contributionMargin / grossRevenue;
        if (contributionMarginPercentage <= 0m)
        {
            throw new BreakEvenCalculationException(
                "A Margem de Contribuição percentual deve ser positiva para calcular o PE.");
        }

        var fixedResult =
            Get(projected, "OPERATING_EXPENSES") +
            Get(projected, "OTHER_OPERATING_RESULTS") +
            Get(projected, "OTHER_RESULTS") +
            Get(projected, "OTHER_NON_OPERATING_INCOME") +
            Get(projected, "FINANCIAL_RESULT");
        var fixedAmountToCover = Math.Abs(fixedResult);
        // Equivalente a fixedAmountToCover / contributionMarginPercentage, mas evita
        // propagar a representação decimal periódica da razão contribution/grossRevenue.
        var baseBreakEven = fixedAmountToCover * grossRevenue / contributionMargin;
        var finalBreakEven = baseBreakEven * (1m + factor);

        var breakEven = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var rule in BreakEvenRowCatalog.All.Where(rule => rule.Behavior != BreakEvenRowCatalog.Calculated))
        {
            if (!projected.TryGetValue(rule.Code, out var projectedValue))
                continue;

            breakEven[rule.Code] = rule.Behavior == BreakEvenRowCatalog.Variable
                ? finalBreakEven * (projectedValue / grossRevenue)
                : projectedValue;
        }

        foreach (var rule in BreakEvenRowCatalog.All.Where(rule => rule.Behavior == BreakEvenRowCatalog.Calculated))
            breakEven[rule.Code] = 0m;

        Recalculate(breakEven, contributionMarginOnGrossRevenue: true);

        var rows = BuildRows(sourceByCode, projected, breakEven, percentages);
        return new BreakEvenCalculationResult(
            new BreakEvenSummaryDto
            {
                ProjectedGrossOperatingRevenue = BreakEvenResponsePrecision.Money(grossRevenue),
                ContributionMargin = BreakEvenResponsePrecision.Money(contributionMargin),
                ContributionMarginPercentage = BreakEvenResponsePrecision.Percentage(contributionMarginPercentage),
                FixedResult = BreakEvenResponsePrecision.Money(fixedResult),
                FixedAmountToCover = BreakEvenResponsePrecision.Money(fixedAmountToCover),
                BaseBreakEven = BreakEvenResponsePrecision.Money(baseBreakEven),
                FinalBreakEven = BreakEvenResponsePrecision.Money(finalBreakEven)
            },
            rows);
    }

    private static Dictionary<string, decimal> ValidateSimulations(
        IReadOnlyList<BreakEvenLineSimulationDto>? simulations,
        IReadOnlyDictionary<string, BreakEvenSourceRow> sourceByCode)
    {
        var result = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var simulation in simulations ?? Array.Empty<BreakEvenLineSimulationDto>())
        {
            if (string.IsNullOrWhiteSpace(simulation.RowCode))
                throw new BreakEvenValidationException("rowCode é obrigatório em cada simulação.");
            if (!result.TryAdd(simulation.RowCode, simulation.Percentage))
                throw new BreakEvenValidationException(
                    $"A linha {simulation.RowCode} foi informada mais de uma vez.");
            if (!sourceByCode.ContainsKey(simulation.RowCode) ||
                !BreakEvenRowCatalog.ByCode.TryGetValue(simulation.RowCode, out var rule))
            {
                throw new BreakEvenValidationException(
                    $"Linha de simulação inexistente: {simulation.RowCode}.");
            }
            if (!rule.CanSimulate)
                throw new BreakEvenValidationException(
                    $"A linha {simulation.RowCode} não aceita simulação.");
            if (simulation.Percentage < -1m)
                throw new BreakEvenValidationException(
                    $"A simulação da linha {simulation.RowCode} não pode ser inferior a -100%.");
        }

        return result;
    }

    private static void Recalculate(
        Dictionary<string, decimal> values,
        bool contributionMarginOnGrossRevenue)
    {
        Set(values, "GROSS_REVENUE", Sum(values,
            "PRODUCT_SALES", "MERCHANDISE_SALES", "SERVICE_REVENUE", "RENTAL_REVENUE"));
        Set(values, "GROSS_REVENUE_DEDUCTIONS", Sum(values,
            "SALES_RETURNS", "SALES_ALLOWANCES", "TAXES_AND_CONTRIBUTIONS"));
        Set(values, "NET_REVENUE", Get(values, "GROSS_REVENUE") + Get(values, "GROSS_REVENUE_DEDUCTIONS"));
        Set(values, "GROSS_PROFIT",
            Get(values, "NET_REVENUE") + Sum(values, "COST_OF_GOODS", "COST_OF_SERVICES", "VARIABLE_COSTS"));
        SetPercentage(values, "GROSS_MARGIN_PERCENT", Get(values, "GROSS_PROFIT"), Get(values, "NET_REVENUE"));
        Set(values, "CONTRIBUTION_MARGIN", Get(values, "GROSS_PROFIT") + Get(values, "VARIABLE_EXPENSES"));
        SetPercentage(
            values,
            "CONTRIBUTION_MARGIN_PERCENT",
            Get(values, "CONTRIBUTION_MARGIN"),
            contributionMarginOnGrossRevenue ? Get(values, "GROSS_REVENUE") : Get(values, "NET_REVENUE"));

        Set(values, "OPERATING_EXPENSES", Sum(values, OperatingExpenseCodes));
        Set(values, "OPERATING_PROFIT",
            Get(values, "CONTRIBUTION_MARGIN") +
            Get(values, "OPERATING_EXPENSES") +
            Get(values, "OTHER_OPERATING_RESULTS"));
        SetPercentage(values, "OPERATING_MARGIN_PERCENT", Get(values, "OPERATING_PROFIT"), Get(values, "NET_REVENUE"));

        Set(values, "OTHER_RESULTS", Sum(values,
            "OTHER_INCOME", "OTHER_EXPENSES", "CAPITAL_GAINS_AND_LOSSES"));
        Set(values, "EBIT",
            Get(values, "OPERATING_PROFIT") +
            Get(values, "OTHER_RESULTS") +
            Get(values, "OTHER_NON_OPERATING_INCOME"));
        SetPercentage(values, "EBIT_MARGIN_PERCENT", Get(values, "EBIT"), Get(values, "NET_REVENUE"));

        Set(values, "FINANCIAL_RESULT", Sum(values, "FINANCIAL_INCOME", "FINANCIAL_EXPENSES"));
        Set(values, "EBT", Get(values, "EBIT") + Get(values, "FINANCIAL_RESULT"));
        SetPercentage(values, "EBT_MARGIN_PERCENT", Get(values, "EBT"), Get(values, "NET_REVENUE"));
    }

    private static IReadOnlyList<BreakEvenRowDto> BuildRows(
        IReadOnlyDictionary<string, BreakEvenSourceRow> sourceByCode,
        IReadOnlyDictionary<string, decimal> projected,
        IReadOnlyDictionary<string, decimal> breakEven,
        IReadOnlyDictionary<string, decimal> simulations)
    {
        var availableRules = BreakEvenRowCatalog.All
            .Where(rule => sourceByCode.ContainsKey(rule.Code))
            .ToArray();
        var parentCodes = availableRules
            .Select(rule => rule.OverrideHierarchy
                ? rule.ParentCode
                : sourceByCode[rule.Code].ParentCode)
            .Where(parentCode => parentCode is not null)
            .ToHashSet(StringComparer.Ordinal);

        return availableRules.Select(rule =>
        {
            var source = sourceByCode[rule.Code];
            var parentCode = rule.OverrideHierarchy ? rule.ParentCode : source.ParentCode;
            var level = rule.OverrideHierarchy && rule.Level.HasValue ? rule.Level.Value : source.Level;
            var isPercentage = string.Equals(source.ValueType, "percentage", StringComparison.OrdinalIgnoreCase);
            Func<decimal, decimal> presentValue = isPercentage
                ? BreakEvenResponsePrecision.PercentageFromHundredScale
                : BreakEvenResponsePrecision.Money;
            return new BreakEvenRowDto
            {
                Code = source.Code,
                Name = source.Name,
                RowType = source.RowType,
                ValueType = source.ValueType,
                DisplayOrder = rule.DisplayOrder,
                Level = level,
                ParentCode = parentCode,
                HasChildren = parentCodes.Contains(source.Code),
                IsTotalizer = rule.Behavior == BreakEvenRowCatalog.Calculated,
                CanSimulate = rule.CanSimulate,
                SimulationPercentage = rule.CanSimulate
                    ? simulations.GetValueOrDefault(source.Code)
                    : null,
                SimulationSignRule = rule.SimulationSignRule,
                Behavior = rule.Behavior,
                Source = source.Source,
                BaseValue = presentValue(source.Value),
                ProjectedValue = presentValue(Get(projected, source.Code)),
                BreakEvenValue = presentValue(Get(breakEven, source.Code))
            };
        }).ToArray();
    }

    private static decimal Sum(IReadOnlyDictionary<string, decimal> values, params string[] codes) =>
        codes.Sum(code => Get(values, code));

    private static decimal Get(IReadOnlyDictionary<string, decimal> values, string code) =>
        values.TryGetValue(code, out var value) ? value : 0m;

    private static void Set(IDictionary<string, decimal> values, string code, decimal value)
    {
        if (values.ContainsKey(code))
            values[code] = value;
    }

    private static void SetPercentage(
        IDictionary<string, decimal> values,
        string code,
        decimal numerator,
        decimal denominator) =>
        Set(values, code, denominator == 0m ? 0m : numerator / denominator * 100m);
}
