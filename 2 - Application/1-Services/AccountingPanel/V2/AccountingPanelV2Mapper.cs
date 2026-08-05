using _2___Application._2_Dto_s.AccountingPanel.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

namespace _2___Application._1_Services.AccountingPanel.V2;

public sealed class AccountingPanelV2MappingException : InvalidOperationException
{
    public AccountingPanelV2MappingException(string message) : base(message)
    {
    }
}

public sealed record AccountingPanelBoundRow(
    AccountingPanelRowDefinition Definition,
    int? TotalizerId,
    int? ClassificationId,
    int? ParentTotalizerId);

public static class AccountingPanelV2Mapper
{
    private const string RealizedScenario = "realizado";

    private static readonly string[] MonthLabels =
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    public static AccountingPanelV2Response Map(
        PainelBalancoContabilRespone assets,
        PainelBalancoContabilRespone liabilities,
        int year)
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(liabilities);

        var sources = new Dictionary<string, PainelBalancoContabilRespone>(StringComparer.Ordinal)
        {
            [AccountingPanelRowCatalog.Asset] = assets,
            [AccountingPanelRowCatalog.Liability] = liabilities
        };
        var periods = GetPeriods(assets, liabilities, year);
        var boundRows = BindCatalog(sources);
        var parentCodes = boundRows
            .Select(bound => bound.Definition.ParentCode)
            .Where(parentCode => parentCode is not null)
            .ToHashSet(StringComparer.Ordinal);
        var legacyRows = boundRows
            .Where(bound => bound.Definition.SourceType != AccountingPanelRowCatalog.Calculated)
            .Select(bound => MapRow(
                bound,
                sources[bound.Definition.StatementKey],
                periods,
                boundRows,
                parentCodes.Contains(bound.Definition.Code)))
            .ToArray();
        var calculatedRows = boundRows
            .Where(bound => bound.Definition.SourceType == AccountingPanelRowCatalog.Calculated)
            .Select(bound => MapCalculatedRow(bound.Definition, legacyRows, periods));
        var rows = legacyRows
            .Concat(calculatedRows)
            .OrderBy(row => row.DisplayOrder)
            .ToArray();

        var duplicateCode = rows
            .GroupBy(row => row.Code, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateCode is not null)
            throw new AccountingPanelV2MappingException(
                $"Código de linha duplicado no Painel Contábil V2: {duplicateCode.Key}.");

        return new AccountingPanelV2Response
        {
            Data = new AccountingPanelV2DataDto
            {
                Periods = periods,
                Scenarios = new[]
                {
                    new AccountingPanelScenarioDto
                    {
                        Key = RealizedScenario,
                        Label = "Realizado",
                        DisplayOrder = 1
                    }
                },
                Statements = new[]
                {
                    new AccountingPanelStatementDto
                    {
                        Key = AccountingPanelRowCatalog.Asset,
                        Label = "Ativo",
                        DisplayOrder = 1,
                        TotalRowCode = "TOTAL_ASSETS"
                    },
                    new AccountingPanelStatementDto
                    {
                        Key = AccountingPanelRowCatalog.Liability,
                        Label = "Passivo",
                        DisplayOrder = 2,
                        TotalRowCode = "TOTAL_LIABILITIES"
                    }
                },
                Rows = rows
            }
        };
    }

    private static IReadOnlyList<AccountingPanelBoundRow> BindCatalog(
        IReadOnlyDictionary<string, PainelBalancoContabilRespone> sources)
    {
        var result = new List<AccountingPanelBoundRow>();

        foreach (var source in sources)
        {
            var referenceMonth = (source.Value.Months ?? new List<MonthPainelContabilRespone>())
                .FirstOrDefault(month => month.Totalizer is { Count: > 0 });
            if (referenceMonth is null)
                continue;

            foreach (var definition in AccountingPanelRowCatalog.All
                         .Where(definition => definition.StatementKey == source.Key))
            {
                if (definition.SourceType == AccountingPanelRowCatalog.Calculated)
                {
                    result.Add(new AccountingPanelBoundRow(definition, null, null, null));
                    continue;
                }

                if (definition.SourceType == AccountingPanelRowCatalog.GeneralTotal)
                {
                    if (!string.Equals(
                            referenceMonth.MonthPainelContabilTotalizer?.Name,
                            definition.LegacySourceName,
                            StringComparison.Ordinal))
                        throw MissingSource(definition, "general total");

                    result.Add(new AccountingPanelBoundRow(definition, null, null, null));
                    continue;
                }

                var matchingTotalizers = (referenceMonth.Totalizer ?? new List<TotalizerParentRespone>())
                    .Where(totalizer => string.Equals(
                        totalizer.Name,
                        definition.SourceType == AccountingPanelRowCatalog.Classification
                            ? definition.LegacyParentTotalizerName
                            : definition.LegacySourceName,
                        StringComparison.Ordinal))
                    .ToArray();
                if (matchingTotalizers.Length == 0)
                    throw MissingSource(definition, "totalizer");

                if (definition.SourceType == AccountingPanelRowCatalog.Totalizer)
                {
                    result.Add(new AccountingPanelBoundRow(
                        definition,
                        UniqueId(matchingTotalizers.Select(totalizer => totalizer.Id)),
                        null,
                        null));
                    continue;
                }

                var matchingClassifications = matchingTotalizers
                    .SelectMany(totalizer => totalizer.Classifications ?? new List<ClassificationRespone>())
                    .Where(classification => string.Equals(
                        classification.Name,
                        definition.LegacySourceName,
                        StringComparison.Ordinal))
                    .ToArray();
                if (matchingClassifications.Length == 0)
                    throw MissingSource(definition, "classification");

                result.Add(new AccountingPanelBoundRow(
                    definition,
                    null,
                    UniqueId(matchingClassifications.Select(classification => classification.Id)),
                    UniqueId(matchingTotalizers.Select(totalizer => totalizer.Id))));
            }
        }

        return result;
    }

    private static int? UniqueId(IEnumerable<int> ids)
    {
        var distinctIds = ids.Distinct().ToArray();
        return distinctIds.Length == 1 ? distinctIds[0] : null;
    }

    private static AccountingPanelV2MappingException MissingSource(
        AccountingPanelRowDefinition definition,
        string sourcePart) =>
        new($"Mapeamento do Painel Contábil V2 ausente. Code: {definition.Code}; " +
            $"statementKey: {definition.StatementKey}; sourceType: {definition.SourceType}; " +
            $"sourcePart: {sourcePart}; legacySource: {definition.LegacySourceName}; " +
            $"legacyParent: {definition.LegacyParentTotalizerName ?? "(none)"}.");

    private static AccountingPanelRowDto MapRow(
        AccountingPanelBoundRow bound,
        PainelBalancoContabilRespone source,
        IReadOnlyList<AccountingPanelPeriodDto> periods,
        IReadOnlyList<AccountingPanelBoundRow> boundRows,
        bool hasChildren)
    {
        var scenarioValues = new Dictionary<string, decimal?>(StringComparer.Ordinal);
        var scenarioCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var scenarioData = bound.Definition.SourceType == AccountingPanelRowCatalog.Classification
            ? new Dictionary<string, IReadOnlyList<AccountingPanelEntryDto>>(StringComparer.Ordinal)
            : null;
        var expandable = hasChildren ||
                         bound.Definition.SourceType == AccountingPanelRowCatalog.Classification;

        foreach (var period in periods)
        {
            var month = FindMonth(source, period);
            if (month is null)
                continue;

            if (!TryGetValue(month, bound.Definition, out var value))
                throw new AccountingPanelV2MappingException(
                    $"Origem do Painel Contábil V2 não encontrada no resultado legado. " +
                    $"Period: {period.Key}; Code: {bound.Definition.Code}; " +
                    $"sourceType: {bound.Definition.SourceType}.");

            scenarioValues[period.Key] = value;
            if (!expandable)
                continue;

            var entries = bound.Definition.SourceType == AccountingPanelRowCatalog.Classification
                ? GetEntries(month, bound.Definition)
                : boundRows
                    .Where(candidate => string.Equals(
                        candidate.Definition.ParentCode,
                        bound.Definition.Code,
                        StringComparison.Ordinal))
                    .SelectMany(candidate => GetEntries(month, candidate.Definition))
                    .ToArray();
            scenarioCounts[period.Key] = entries.Count;

            if (scenarioData is not null)
                scenarioData[period.Key] = entries.Select(MapEntry).ToArray();
        }

        return new AccountingPanelRowDto
        {
            Code = bound.Definition.Code,
            StatementKey = bound.Definition.StatementKey,
            Name = bound.Definition.Name,
            RowType = bound.Definition.RowType,
            DisplayOrder = bound.Definition.DisplayOrder,
            Level = bound.Definition.Level,
            ParentCode = bound.Definition.ParentCode,
            Expandable = expandable,
            Source = new AccountingPanelRowSourceDto
            {
                SourceType = bound.Definition.SourceType,
                TotalizerId = bound.TotalizerId,
                ClassificationId = bound.ClassificationId,
                SourceParentTotalizerId = bound.ParentTotalizerId
            },
            Details = new AccountingPanelRowDetailsDto
            {
                Available = expandable,
                Counts = expandable
                    ? new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal)
                    {
                        [RealizedScenario] = scenarioCounts
                    }
                    : new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal),
                Data = scenarioData is null
                    ? null
                    : new Dictionary<string, Dictionary<string, IReadOnlyList<AccountingPanelEntryDto>>>(StringComparer.Ordinal)
                    {
                        [RealizedScenario] = scenarioData
                    }
            },
            Values = new Dictionary<string, Dictionary<string, decimal?>>(StringComparer.Ordinal)
            {
                [RealizedScenario] = scenarioValues
            }
        };
    }

    private static AccountingPanelRowDto MapCalculatedRow(
        AccountingPanelRowDefinition definition,
        IReadOnlyList<AccountingPanelRowDto> legacyRows,
        IReadOnlyList<AccountingPanelPeriodDto> periods)
    {
        if (definition.Code != "BALANCE_DIFFERENCE")
            throw new AccountingPanelV2MappingException(
                $"Cálculo do Painel Contábil V2 não implementado. Code: {definition.Code}.");

        var totalAssets = legacyRows.Single(row => row.Code == "TOTAL_ASSETS");
        var totalLiabilities = legacyRows.Single(row => row.Code == "TOTAL_LIABILITIES");
        var scenarioValues = new Dictionary<string, decimal?>(StringComparer.Ordinal);

        foreach (var period in periods)
        {
            if (!totalAssets.Values[RealizedScenario].TryGetValue(period.Key, out var assetValue) ||
                !totalLiabilities.Values[RealizedScenario].TryGetValue(period.Key, out var liabilityValue))
                continue;

            scenarioValues[period.Key] = assetValue - liabilityValue;
        }

        return new AccountingPanelRowDto
        {
            Code = definition.Code,
            StatementKey = definition.StatementKey,
            Name = definition.Name,
            RowType = definition.RowType,
            ValueType = "currency",
            DisplayOrder = definition.DisplayOrder,
            Level = definition.Level,
            ParentCode = definition.ParentCode,
            Expandable = false,
            Source = new AccountingPanelRowSourceDto
            {
                SourceType = definition.SourceType
            },
            Details = new AccountingPanelRowDetailsDto
            {
                Available = false,
                Counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal),
                Data = null
            },
            Values = new Dictionary<string, Dictionary<string, decimal?>>(StringComparer.Ordinal)
            {
                [RealizedScenario] = scenarioValues
            }
        };
    }

    private static bool TryGetValue(
        MonthPainelContabilRespone month,
        AccountingPanelRowDefinition definition,
        out decimal value)
    {
        value = default;

        if (definition.SourceType == AccountingPanelRowCatalog.GeneralTotal)
        {
            if (!string.Equals(
                    month.MonthPainelContabilTotalizer?.Name,
                    definition.LegacySourceName,
                    StringComparison.Ordinal))
                return false;

            value = month.MonthPainelContabilTotalizer.TotalValue;
            return true;
        }

        var totalizers = (month.Totalizer ?? new List<TotalizerParentRespone>())
            .Where(totalizer => string.Equals(
                totalizer.Name,
                definition.SourceType == AccountingPanelRowCatalog.Classification
                    ? definition.LegacyParentTotalizerName
                    : definition.LegacySourceName,
                StringComparison.Ordinal))
            .ToArray();
        if (totalizers.Length == 0)
            return false;

        if (definition.SourceType == AccountingPanelRowCatalog.Totalizer)
        {
            value = totalizers.Sum(totalizer => totalizer.TotalValue);
            return true;
        }

        var classifications = totalizers
            .SelectMany(totalizer => totalizer.Classifications ?? new List<ClassificationRespone>())
            .Where(classification => string.Equals(
                classification.Name,
                definition.LegacySourceName,
                StringComparison.Ordinal))
            .ToArray();
        if (classifications.Length == 0)
            return false;

        value = classifications.Sum(classification => classification.Value);
        return true;
    }

    private static IReadOnlyList<BalanceteDataResponse> GetEntries(
        MonthPainelContabilRespone month,
        AccountingPanelRowDefinition definition)
    {
        if (definition.SourceType != AccountingPanelRowCatalog.Classification)
            return Array.Empty<BalanceteDataResponse>();

        return (month.Totalizer ?? new List<TotalizerParentRespone>())
            .Where(totalizer => string.Equals(
                totalizer.Name,
                definition.LegacyParentTotalizerName,
                StringComparison.Ordinal))
            .SelectMany(totalizer => totalizer.Classifications ?? new List<ClassificationRespone>())
            .Where(classification => string.Equals(
                classification.Name,
                definition.LegacySourceName,
                StringComparison.Ordinal))
            .SelectMany(classification => classification.Datas ?? new List<BalanceteDataResponse>())
            .ToArray();
    }

    private static AccountingPanelEntryDto MapEntry(BalanceteDataResponse entry) =>
        new()
        {
            Id = entry.Id,
            TypeOrder = entry.TypeOrder,
            Name = entry.Name,
            CostCenter = entry.CostCenter,
            InitialValue = entry.InitialValue,
            CreditValue = entry.CreditValue,
            DebitValue = entry.DebitValue,
            Value = entry.Value
        };

    private static IReadOnlyList<AccountingPanelPeriodDto> GetPeriods(
        PainelBalancoContabilRespone assets,
        PainelBalancoContabilRespone liabilities,
        int year) =>
        new[] { assets, liabilities }
            .SelectMany(panel => panel.Months ?? new List<MonthPainelContabilRespone>())
            .Select(month => month.DateMonth)
            .Distinct()
            .OrderBy(month => month == 13 ? int.MaxValue : month)
            .Select(month => MapPeriod(month, year))
            .ToArray();

    private static AccountingPanelPeriodDto MapPeriod(int month, int year)
    {
        if (month == 13)
        {
            return new AccountingPanelPeriodDto
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
            throw new AccountingPanelV2MappingException(
                $"Período mensal legado inválido no Painel Contábil V2: {month}.");

        return new AccountingPanelPeriodDto
        {
            Key = $"{year:D4}-{month:D2}",
            Label = MonthLabels[month - 1],
            Year = year,
            Month = month,
            Type = "month",
            DisplayOrder = month
        };
    }

    private static MonthPainelContabilRespone? FindMonth(
        PainelBalancoContabilRespone panel,
        AccountingPanelPeriodDto period)
    {
        var monthNumber = period.Type == "accumulated" ? 13 : period.Month;
        return (panel.Months ?? new List<MonthPainelContabilRespone>())
            .FirstOrDefault(month => month.DateMonth == monthNumber);
    }
}
