using _2___Application._2_Dto_s.DRE.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

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
    private static readonly (string Key, string Label, int Order)[] ScenarioDefinitions =
    {
        ("realizado", "Realizado", 1),
        ("orcado", "Orçado", 2),
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

        var scenarios = GetScenarios(legacy);
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
                scenarios,
                boundRows,
                parentCodes.Contains(bound.Definition.Code)))
            .OrderBy(row => row.DisplayOrder)
            .ToArray();

        var duplicateCode = rows.GroupBy(row => row.Code).FirstOrDefault(group => group.Count() > 1);
        if (duplicateCode is not null)
            throw new DreV2MappingException($"Código de linha DRE V2 duplicado: {duplicateCode.Key}.");

        return new DreV2Response
        {
            Data = new DreV2DataDto
            {
                Periods = periods,
                Scenarios = scenarios,
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

        return monthNumbers.Select(month =>
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
                    DisplayOrder = 13
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
                DisplayOrder = month
            };
        }).ToArray();
    }

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
