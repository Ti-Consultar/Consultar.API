using _2___Application._2_Dto_s.DRE.V2;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application._1_Services.FinancialReports;

namespace _2___Application._1_Services.DRE.V2;

public sealed class DreV2MappingException : InvalidOperationException
{
    public DreV2MappingException(string message) : base(message)
    {
    }
}

public sealed record DreBoundRowDefinition(
    DreRowDefinition Definition,
    int? TotalizerId,
    int? ClassificationId,
    int? ParentTotalizerId);

public static class DreV2Mapper
{
    private const string RollingPeriodKey = "annual-rolling";

    private static readonly (string Key, string Label, int Order)[] ScenarioDefinitions =
    {
        ("orcado", "Orçado", 1),
        ("realizado", "Realizado", 2),
        ("variacao", "Variação", 3)
    };

    private static readonly string[] MonthLabels =
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    public static DreV2Response Map(PainelBalancoComparativoResponse legacy, int year)
    {
        ArgumentNullException.ThrowIfNull(legacy);

        var sourceScenarios = GetScenarios(legacy);
        var periods = GetPeriods(legacy, year);
        var boundRows = BindCatalog(legacy);
        var parentCodes = boundRows
            .Select(bound => bound.Definition.ParentCode)
            .Where(parentCode => parentCode is not null)
            .ToHashSet(StringComparer.Ordinal);
        var rows = boundRows
            .Select(bound => MapRow(
                bound,
                legacy,
                periods,
                sourceScenarios,
                boundRows,
                parentCodes.Contains(bound.Definition.Code)))
            .OrderBy(row => row.DisplayOrder)
            .ToArray();

        var hasRolling = periods.Any(period => period.Type == RollingContract.PeriodType);
        if (hasRolling)
            ApplyRollingValues(rows, legacy, year);

        var duplicateCode = rows.GroupBy(row => row.Code).FirstOrDefault(group => group.Count() > 1);
        if (duplicateCode is not null)
            throw new DreV2MappingException($"Código de linha DRE V2 duplicado: {duplicateCode.Key}.");

        return new DreV2Response
        {
            Data = new DreV2DataDto
            {
                Periods = periods,
                Scenarios = sourceScenarios,
                Rows = rows
            }
        };
    }

    public static DreRowDetailsV2Response MapDetails(
        PainelBalancoComparativoResponse legacy,
        int year,
        string rowCode,
        string scenario,
        string period)
    {
        ArgumentNullException.ThrowIfNull(legacy);

        var bound = BindCatalog(legacy)
            .SingleOrDefault(item => string.Equals(item.Definition.Code, rowCode, StringComparison.Ordinal));

        if (bound is null)
            throw new KeyNotFoundException($"Linha DRE V2 não encontrada: {rowCode}.");

        var panel = GetScenarioPanel(legacy, scenario)
            ?? throw new KeyNotFoundException($"Cenário DRE V2 não encontrado: {scenario}.");
        var month = FindMonth(panel, period, year)
            ?? throw new KeyNotFoundException($"Período DRE V2 não encontrado: {period}.");

        var entries = GetEntries(month, bound)
            .Select(MapEntry)
            .ToArray();

        return new DreRowDetailsV2Response
        {
            Data = new DreRowDetailsV2DataDto
            {
                RowCode = rowCode,
                Scenario = scenario,
                Period = period,
                Entries = entries
            }
        };
    }

    public static IReadOnlyList<DreBoundRowDefinition> BindCatalog(PainelBalancoComparativoResponse legacy)
    {
        var referenceMonth = EnumerateScenarioPanels(legacy)
            .SelectMany(item => item.Panel.Months ?? new List<MonthPainelContabilRespone>())
            .FirstOrDefault(month => month.Totalizer is { Count: > 0 });

        if (referenceMonth is null)
            return Array.Empty<DreBoundRowDefinition>();

        var totalizersByName = referenceMonth.Totalizer
            .GroupBy(totalizer => totalizer.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var result = new List<DreBoundRowDefinition>(DreRowCatalog.All.Count);

        foreach (var definition in DreRowCatalog.All)
        {
            if (definition.SourceType == DreRowCatalog.Calculated)
            {
                result.Add(new DreBoundRowDefinition(definition, null, null, null));
                continue;
            }

            if (definition.SourceType == DreRowCatalog.Totalizer)
            {
                if (!totalizersByName.TryGetValue(definition.LegacySourceName, out var totalizers))
                {
                    if (!definition.Required)
                        continue;

                    throw MissingSource(definition, "totalizer", totalizersByName.Keys);
                }

                var totalizer = totalizers[0];
                result.Add(new DreBoundRowDefinition(definition, totalizer.Id, null, null));
                continue;
            }

            if (definition.LegacyParentTotalizerName is null)
                throw MissingSource(definition, "parent totalizer", totalizersByName.Keys);

            if (!totalizersByName.TryGetValue(definition.LegacyParentTotalizerName, out var parents))
            {
                if (!definition.Required)
                    continue;

                throw MissingSource(definition, "parent totalizer", totalizersByName.Keys);
            }

            var match = parents
                .SelectMany(parent =>
                    (parent.Classifications ?? new List<ClassificationRespone>())
                    .Select(classification => new { Parent = parent, Classification = classification }))
                .FirstOrDefault(item =>
                    string.Equals(
                        item.Classification.Name,
                        definition.LegacySourceName,
                        StringComparison.Ordinal));

            if (match is null && !definition.Required)
                continue;

            if (match is null)
                throw MissingSource(definition, "classification", totalizersByName.Keys);

            result.Add(new DreBoundRowDefinition(
                definition,
                null,
                match.Classification.Id,
                match.Parent.Id));
        }

        return result;
    }

    private static DreV2MappingException MissingSource(
        DreRowDefinition definition,
        string sourcePart,
        IEnumerable<string>? availableTotalizers = null) =>
        new($"Mapeamento DRE V2 ausente ou ambíguo. Code: {definition.Code}; " +
            $"sourceType: {definition.SourceType}; sourcePart: {sourcePart}; " +
            $"legacySource: {definition.LegacySourceName}; " +
            $"legacyParent: {definition.LegacyParentTotalizerName ?? "(none)"}; " +
            $"availableTotalizers: [{string.Join(", ", availableTotalizers ?? Array.Empty<string>())}].");

    private static IReadOnlyList<DreScenarioDto> GetScenarios(PainelBalancoComparativoResponse legacy) =>
        ScenarioDefinitions
            .Where(definition => HasScenarioData(GetScenarioPanel(legacy, definition.Key)))
            .Select(definition => new DreScenarioDto
            {
                Key = definition.Key,
                Label = definition.Label,
                DisplayOrder = definition.Order
            })
            .ToArray();

    private static bool HasScenarioData(PainelBalancoContabilRespone? panel) =>
        panel?.Months?.Any(month => month.Totalizer is { Count: > 0 }) == true;

    private static IReadOnlyList<DrePeriodDto> GetPeriods(PainelBalancoComparativoResponse legacy, int year)
    {
        var monthNumbers = EnumerateScenarioPanels(legacy)
            .SelectMany(item => item.Panel.Months ?? new List<MonthPainelContabilRespone>())
            .Select(month => month.DateMonth)
            .Distinct()
            .OrderBy(month => month == 13 ? int.MaxValue : month)
            .ToArray();

        var sourceColumns = GetSourcePeriodColumns(legacy);
        var periods = monthNumbers.Select(month =>
        {
            if (month == 13)
            {
                return new DrePeriodDto
                {
                    Key = "accumulated",
                    Label = "Acumulado",
                    Year = year,
                    Month = null,
                    Type = "accumulated",
                    DisplayOrder = 13,
                    Columns = sourceColumns
                };
            }

            if (month is < 1 or > 12)
                throw new DreV2MappingException($"Período mensal legado inválido: {month}.");

            return new DrePeriodDto
            {
                Key = $"{year:D4}-{month:D2}",
                Label = MonthLabels[month - 1],
                Year = year,
                Month = month,
                Type = "month",
                DisplayOrder = month,
                Columns = sourceColumns
            };
        }).ToList();

        if (EnumerateScenarioPanels(legacy)
            .SelectMany(item => item.Panel.Months ?? new List<MonthPainelContabilRespone>())
            .Any(month => month.DateMonth is >= 1 and <= 12))
        {
            periods.Add(new DrePeriodDto
            {
                Key = RollingPeriodKey,
                Label = year.ToString(),
                Year = year,
                Month = null,
                Type = RollingContract.PeriodType,
                DisplayOrder = RollingContract.AnnualDisplayOrder,
                Columns = new[]
                {
                    new DrePeriodColumnDto { Key = RollingContract.BudgetKey, Label = "Orçado", Type = "budget", DisplayOrder = 1 },
                    new DrePeriodColumnDto { Key = RollingContract.RollingKey, Label = "Rolling", Type = RollingContract.PeriodType, DisplayOrder = 2 },
                    new DrePeriodColumnDto { Key = RollingContract.VariationKey, Label = "Variação", Type = "variation", DisplayOrder = 3 }
                }
            });
        }

        return periods;
    }

    private static IReadOnlyList<DrePeriodColumnDto> GetSourcePeriodColumns(
        PainelBalancoComparativoResponse legacy) =>
        ScenarioDefinitions
            .Where(definition => HasScenarioData(GetScenarioPanel(legacy, definition.Key)))
            .Select(definition => new DrePeriodColumnDto
            {
                Key = definition.Key,
                Label = definition.Label,
                Type = definition.Key switch
                {
                    "realizado" => "actual",
                    "orcado" => "budget",
                    _ => "variation"
                },
                DisplayOrder = definition.Order
            })
            .ToArray();

    private static DreRowDto MapRow(
        DreBoundRowDefinition bound,
        PainelBalancoComparativoResponse legacy,
        IReadOnlyList<DrePeriodDto> periods,
        IReadOnlyList<DreScenarioDto> scenarios,
        IReadOnlyList<DreBoundRowDefinition> boundRows,
        bool hasChildren)
    {
        var values = new Dictionary<string, Dictionary<string, decimal?>>(StringComparer.Ordinal);
        var counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        var classificationData = bound.Definition.SourceType == DreRowCatalog.Classification
            ? new Dictionary<string, Dictionary<string, IReadOnlyList<DreDetailEntryDto>>>(StringComparer.Ordinal)
            : null;
        var expandable = bound.Definition.SourceType == DreRowCatalog.Classification ||
                         bound.Definition.RowType == "section" ||
                         hasChildren;

        foreach (var scenario in scenarios)
        {
            var panel = GetScenarioPanel(legacy, scenario.Key)!;
            var scenarioValues = new Dictionary<string, decimal?>(StringComparer.Ordinal);
            var scenarioCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var scenarioData = classificationData is not null
                ? new Dictionary<string, IReadOnlyList<DreDetailEntryDto>>(StringComparer.Ordinal)
                : null;

            foreach (var period in periods)
            {
                if (period.Type == RollingContract.PeriodType)
                    continue;

                var month = FindMonth(panel, period.Key, period.Year ?? 0);
                if (month is null)
                    continue;

                if (!TryGetValue(month, bound, boundRows, out var value))
                    throw new DreV2MappingException(
                        $"Origem DRE V2 não encontrada no resultado legado. " +
                        $"Scenario: {scenario.Key}; Period: {period.Key}; Code: {bound.Definition.Code}; " +
                        $"sourceType: {bound.Definition.SourceType}; totalizerId: {bound.TotalizerId}; " +
                        $"classificationId: {bound.ClassificationId}; " +
                        $"sourceParentTotalizerId: {bound.ParentTotalizerId}.");

                scenarioValues[period.Key] = value;
                if (expandable)
                {
                    var entries = GetEntries(month, bound);
                    scenarioCounts[period.Key] = entries.Count;

                    if (scenarioData is not null)
                        scenarioData[period.Key] = entries.Select(MapEntry).ToArray();
                }
            }

            values[scenario.Key] = scenarioValues;
            if (expandable)
                counts[scenario.Key] = scenarioCounts;
            if (classificationData is not null && scenarioData is not null)
                classificationData[scenario.Key] = scenarioData;
        }

        return new DreRowDto
        {
            Code = bound.Definition.Code,
            Name = bound.Definition.Name,
            RowType = bound.Definition.RowType,
            ValueType = bound.Definition.ValueType,
            DisplayOrder = bound.Definition.DisplayOrder,
            Level = bound.Definition.Level,
            ParentCode = bound.Definition.ParentCode,
            Expandable = expandable,
            Source = new DreRowSourceDto
            {
                SourceType = bound.Definition.SourceType,
                TotalizerId = bound.TotalizerId,
                ClassificationId = bound.ClassificationId,
                SourceParentTotalizerId = bound.ParentTotalizerId
            },
            Details = new DreRowDetailsMetadataDto
            {
                Available = expandable,
                Counts = counts,
                Data = classificationData
            },
            Values = values
        };
    }

    private static void ApplyRollingValues(
        IReadOnlyList<DreRowDto> rows,
        PainelBalancoComparativoResponse legacy,
        int year)
    {
        var realizedMonths = GetMonthlyNumbers(legacy.Realizado);
        var budgetMonths = GetMonthlyNumbers(legacy.Orcado);
        var rollingSelection = RollingPeriodSelector.Select(realizedMonths, budgetMonths);
        var annualBudgetMonths = RollingPeriodSelector.SelectBudgetMonths(budgetMonths);
        var hasBudget = annualBudgetMonths.Count > 0;

        foreach (var row in rows.Where(row => row.ValueType != "percentage"))
        {
            var budget = hasBudget
                ? SumMonths(row, RollingContract.BudgetKey, annualBudgetMonths, year)
                : (decimal?)null;
            var rolling = rollingSelection.Count > 0
                ? SumSelection(row, rollingSelection, year)
                : (decimal?)null;

            SetAnnualValue(row, RollingContract.BudgetKey, budget);
            SetAnnualValue(row, RollingContract.RollingKey, rolling);
            SetAnnualValue(row, RollingContract.VariationKey,
                rolling.HasValue && budget.HasValue ? rolling.Value - budget.Value : null);
        }

        var rowsByCode = rows.ToDictionary(row => row.Code, StringComparer.Ordinal);
        foreach (var row in rows.Where(row => row.ValueType == "percentage"))
        {
            foreach (var scenario in new[] { RollingContract.BudgetKey, RollingContract.RollingKey, RollingContract.VariationKey })
            {
                var value = CalculateAnnualPercentage(row.Code, scenario, rowsByCode);
                SetAnnualValue(row, scenario, value);
            }
        }

        foreach (var row in rows.Where(row => row.Expandable))
        {
            foreach (var scenario in new[] { RollingContract.BudgetKey, RollingContract.RollingKey, RollingContract.VariationKey })
            {
                if (!row.Details.Counts.TryGetValue(scenario, out var counts))
                {
                    counts = new Dictionary<string, int>(StringComparer.Ordinal);
                    row.Details.Counts[scenario] = counts;
                }

                counts[RollingPeriodKey] = 0;

                if (row.Details.Data is not null)
                {
                    if (!row.Details.Data.TryGetValue(scenario, out var data))
                    {
                        data = new Dictionary<string, IReadOnlyList<DreDetailEntryDto>>(StringComparer.Ordinal);
                        row.Details.Data[scenario] = data;
                    }

                    data[RollingPeriodKey] = Array.Empty<DreDetailEntryDto>();
                }
            }
        }
    }

    private static IReadOnlyList<int> GetMonthlyNumbers(PainelBalancoContabilRespone? panel) =>
        panel?.Months?
            .Where(month => month.DateMonth is >= 1 and <= 12)
            .Select(month => month.DateMonth)
            .Distinct()
            .OrderBy(month => month)
            .ToArray()
        ?? Array.Empty<int>();

    private static decimal SumMonths(
        DreRowDto row,
        string scenario,
        IEnumerable<int> months,
        int year) =>
        months.Sum(month => GetMonthlyValue(row, scenario, month, year));

    private static decimal SumSelection(
        DreRowDto row,
        IEnumerable<RollingPeriodSelection> selection,
        int year) =>
        selection.Sum(item => GetMonthlyValue(
            row,
            item.Source == RollingPeriodSource.Realized ? RollingContract.RealizedKey : RollingContract.BudgetKey,
            item.Month,
            year));

    private static decimal GetMonthlyValue(DreRowDto row, string scenario, int month, int year)
    {
        var period = $"{year:D4}-{month:D2}";
        if (row.Values.TryGetValue(scenario, out var values) &&
            values.TryGetValue(period, out var value) &&
            value.HasValue)
        {
            return value.Value;
        }

        throw new DreV2MappingException(
            $"Valor mensal necessário ao Rolling não encontrado. " +
            $"Scenario: {scenario}; Period: {period}; Code: {row.Code}.");
    }

    private static void SetAnnualValue(DreRowDto row, string scenario, decimal? value)
    {
        if (!row.Values.TryGetValue(scenario, out var values))
        {
            values = new Dictionary<string, decimal?>(StringComparer.Ordinal);
            row.Values[scenario] = values;
        }

        values[RollingPeriodKey] = value;
    }

    private static decimal? CalculateAnnualPercentage(
        string percentageCode,
        string scenario,
        IReadOnlyDictionary<string, DreRowDto> rows)
    {
        var numeratorCode = percentageCode switch
        {
            "GROSS_MARGIN_PERCENT" => "GROSS_PROFIT",
            "CONTRIBUTION_MARGIN_PERCENT" => "CONTRIBUTION_MARGIN",
            "OPERATING_MARGIN_PERCENT" => "OPERATING_PROFIT",
            "EBIT_MARGIN_PERCENT" => "EBIT",
            "EBT_MARGIN_PERCENT" => "EBT",
            "NET_MARGIN_PERCENT" => "NET_INCOME",
            "EBITDA_MARGIN_PERCENT" => "EBITDA",
            "NOPAT_MARGIN_PERCENT" => "NOPAT",
            _ => null
        };

        if (numeratorCode is null ||
            !rows.TryGetValue(numeratorCode, out var numeratorRow) ||
            !rows.TryGetValue("NET_REVENUE", out var denominatorRow))
        {
            return null;
        }

        var numerator = GetAnnualValue(numeratorRow, scenario);
        var denominator = GetAnnualValue(denominatorRow, scenario);
        if (!numerator.HasValue || !denominator.HasValue)
            return null;

        return denominator.Value == 0m
            ? 0m
            : Math.Round(numerator.Value / denominator.Value * 100m, 2);
    }

    private static decimal? GetAnnualValue(DreRowDto row, string scenario) =>
        row.Values.TryGetValue(scenario, out var values) &&
        values.TryGetValue(RollingPeriodKey, out var value)
            ? value
            : null;

    private static bool TryGetValue(
        MonthPainelContabilRespone month,
        DreBoundRowDefinition bound,
        IReadOnlyList<DreBoundRowDefinition> boundRows,
        out decimal value)
    {
        value = default;
        if (bound.Definition.SourceType == DreRowCatalog.Calculated)
        {
            var children = boundRows
                .Where(candidate => string.Equals(
                    candidate.Definition.ParentCode,
                    bound.Definition.Code,
                    StringComparison.Ordinal))
                .ToArray();

            if (children.Length == 0)
                return false;

            foreach (var child in children)
            {
                if (!TryGetValue(month, child, boundRows, out var childValue))
                    return false;

                value += childValue;
            }

            return true;
        }

        if (bound.TotalizerId.HasValue)
        {
            var totalizer = month.Totalizer?.SingleOrDefault(item => item.Id == bound.TotalizerId.Value);
            if (totalizer is null)
                return false;

            value = totalizer.TotalValue;
            return true;
        }

        var parent = month.Totalizer?.SingleOrDefault(item => item.Id == bound.ParentTotalizerId);
        var classification = parent?.Classifications?.SingleOrDefault(item => item.Id == bound.ClassificationId);
        if (classification is null)
            return false;

        value = classification.Value;
        return true;
    }

    private static IReadOnlyList<BalanceteDataResponse> GetEntries(
        MonthPainelContabilRespone month,
        DreBoundRowDefinition bound)
    {
        if (bound.Definition.SourceType == DreRowCatalog.Calculated)
            return Array.Empty<BalanceteDataResponse>();

        if (bound.TotalizerId.HasValue)
        {
            return month.Totalizer?
                       .SingleOrDefault(item => item.Id == bound.TotalizerId.Value)?
                       .Classifications?
                       .SelectMany(item => item.Datas ?? new List<BalanceteDataResponse>())
                       .ToArray()
                   ?? Array.Empty<BalanceteDataResponse>();
        }

        return month.Totalizer?
                   .SingleOrDefault(item => item.Id == bound.ParentTotalizerId)?
                   .Classifications?
                   .SingleOrDefault(item => item.Id == bound.ClassificationId)?
                   .Datas?
                   .ToArray()
               ?? Array.Empty<BalanceteDataResponse>();
    }

    private static DreDetailEntryDto MapEntry(BalanceteDataResponse entry) => new()
    {
        Id = entry.Id,
        Name = entry.Name ?? string.Empty,
        CostCenter = entry.CostCenter ?? string.Empty,
        InitialValue = entry.InitialValue,
        CreditValue = entry.CreditValue,
        DebitValue = entry.DebitValue,
        Value = entry.Value
    };

    private static MonthPainelContabilRespone? FindMonth(
        PainelBalancoContabilRespone panel,
        string period,
        int year)
    {
        if (period == "accumulated")
            return panel.Months?.SingleOrDefault(month => month.DateMonth == 13);

        if (!DateOnly.TryParseExact(
                $"{period}-01",
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var date) ||
            date.Year != year)
        {
            throw new ArgumentException($"Período inválido: {period}.", nameof(period));
        }

        return panel.Months?.SingleOrDefault(month => month.DateMonth == date.Month);
    }

    private static IEnumerable<(string Key, PainelBalancoContabilRespone Panel)> EnumerateScenarioPanels(
        PainelBalancoComparativoResponse legacy)
    {
        if (legacy.Realizado is not null)
            yield return ("realizado", legacy.Realizado);
        if (legacy.Orcado is not null)
            yield return ("orcado", legacy.Orcado);
        if (legacy.Variacao is not null)
            yield return ("variacao", legacy.Variacao);
    }

    private static PainelBalancoContabilRespone? GetScenarioPanel(
        PainelBalancoComparativoResponse legacy,
        string scenario) =>
        scenario switch
        {
            "realizado" => legacy.Realizado,
            "orcado" => legacy.Orcado,
            "variacao" => legacy.Variacao,
            _ => null
        };
}
