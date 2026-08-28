namespace _2___Application._1_Services.FinancialReports;

public static class RollingContract
{
    public const string RealizedKey = "realizado";
    public const string BudgetKey = "orcado";
    public const string RollingKey = "rolling";
    public const string VariationKey = "variacao";
    public const string PeriodType = "rolling";
    public const int AnnualDisplayOrder = 14;
}

public enum RollingPeriodSource
{
    Realized,
    Budget
}

public sealed record RollingPeriodSelection(int Month, RollingPeriodSource Source);

/// <summary>
/// Selects the monthly sources used by an annual rolling projection.
/// Period existence is supplied by the report dataset; financial values are never
/// inspected, so a zero value remains a valid realized or budget value.
/// </summary>
public static class RollingPeriodSelector
{
    public static IReadOnlyList<RollingPeriodSelection> Select(
        IEnumerable<int> realizedMonths,
        IEnumerable<int> budgetMonths)
    {
        ArgumentNullException.ThrowIfNull(realizedMonths);
        ArgumentNullException.ThrowIfNull(budgetMonths);

        var realized = Normalize(realizedMonths);
        var budget = Normalize(budgetMonths);
        var lastRealizedMonth = realized.Count == 0 ? (int?)null : realized[^1];

        if (!lastRealizedMonth.HasValue)
        {
            return budget
                .Select(month => new RollingPeriodSelection(month, RollingPeriodSource.Budget))
                .ToArray();
        }

        return realized
            .Where(month => month <= lastRealizedMonth.Value)
            .Select(month => new RollingPeriodSelection(month, RollingPeriodSource.Realized))
            .Concat(budget
                .Where(month => month > lastRealizedMonth.Value)
                .Select(month => new RollingPeriodSelection(month, RollingPeriodSource.Budget)))
            .OrderBy(selection => selection.Month)
            .ToArray();
    }

    public static IReadOnlyList<int> SelectBudgetMonths(IEnumerable<int> budgetMonths)
    {
        ArgumentNullException.ThrowIfNull(budgetMonths);
        return Normalize(budgetMonths);
    }

    private static List<int> Normalize(IEnumerable<int> months)
    {
        var normalized = months.Distinct().OrderBy(month => month).ToList();
        var invalid = normalized
            .Cast<int?>()
            .FirstOrDefault(month => month is < 1 or > 12);

        if (invalid.HasValue)
            throw new ArgumentOutOfRangeException(nameof(months), invalid.Value, "O mês deve estar entre 1 e 12.");

        return normalized;
    }
}
