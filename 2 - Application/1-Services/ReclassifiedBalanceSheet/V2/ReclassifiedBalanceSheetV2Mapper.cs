using _2___Application._2_Dto_s.ReclassifiedBalanceSheet.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

namespace _2___Application._1_Services.ReclassifiedBalanceSheet.V2;

public sealed class ReclassifiedBalanceSheetV2MappingException : InvalidOperationException
{
    public ReclassifiedBalanceSheetV2MappingException(string message) : base(message)
    {
    }
}

public sealed record ReclassifiedBalanceSheetBoundRow(
    ReclassifiedBalanceSheetRowDefinition Definition,
    int? SourceId);

public static class ReclassifiedBalanceSheetV2Mapper
{
    private static readonly IReadOnlyDictionary<string, string[]> NestedCalculatedTotalizerSources =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Outros Ativos Operacionais Total"] =
                new[] { "Outros Ativos Operacionais", "Contas Transitórias Ativo" },
            ["Outros Passivos Operacionais Total"] =
                new[] { "Outros Passivos Operacionais", "Contas Transitórias Passivo" }
        };

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

    public static ReclassifiedBalanceSheetV2Response Map(
        PainelBalancoComparativoResponse assets,
        PainelBalancoComparativoResponse liabilities,
        int year)
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(liabilities);

        var statements = GetStatements();
        var scenarios = GetScenarios();
        var periods = GetPeriods(assets, liabilities, year);
        var statementSources = new Dictionary<string, PainelBalancoComparativoResponse>(StringComparer.Ordinal)
        {
            [ReclassifiedBalanceSheetRowCatalog.Asset] = assets,
            [ReclassifiedBalanceSheetRowCatalog.Liability] = liabilities
        };
        var boundRows = BindCatalog(statementSources);
        var legacyRows = boundRows
            .Where(bound => bound.Definition.SourceType != ReclassifiedBalanceSheetRowCatalog.Calculated)
            .Select(bound => MapRow(bound, statementSources[bound.Definition.StatementKey], periods, scenarios))
            .ToArray();
        var calculatedRows = boundRows
            .Where(bound => bound.Definition.SourceType == ReclassifiedBalanceSheetRowCatalog.Calculated)
            .Select(bound => MapCalculatedRow(bound.Definition, legacyRows, periods, scenarios));
        var rows = legacyRows
            .Concat(calculatedRows)
            .OrderBy(row => row.DisplayOrder)
            .ToArray();

        var duplicateCode = rows
            .GroupBy(row => row.Code, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateCode is not null)
            throw new ReclassifiedBalanceSheetV2MappingException(
                $"Código de linha duplicado no Balanço Reclassificado V2: {duplicateCode.Key}.");

        return new ReclassifiedBalanceSheetV2Response
        {
            Data = new ReclassifiedBalanceSheetV2DataDto
            {
                Periods = periods,
                Scenarios = scenarios,
                Statements = statements,
                Rows = rows
            }
        };
    }

    private static IReadOnlyList<ReclassifiedBalanceSheetBoundRow> BindCatalog(
        IReadOnlyDictionary<string, PainelBalancoComparativoResponse> statementSources)
    {
        var result = new List<ReclassifiedBalanceSheetBoundRow>();

        foreach (var statement in statementSources)
        {
            var referenceMonth = EnumeratePanels(statement.Value)
                .SelectMany(panel => panel.Months ?? new List<MonthPainelContabilRespone>())
                .FirstOrDefault(month => month.Totalizer is { Count: > 0 });
            if (referenceMonth is null)
                continue;

            foreach (var definition in ReclassifiedBalanceSheetRowCatalog.All
                         .Where(definition => definition.StatementKey == statement.Key))
            {
                if (definition.SourceType == ReclassifiedBalanceSheetRowCatalog.Calculated)
                {
                    result.Add(new ReclassifiedBalanceSheetBoundRow(definition, null));
                    continue;
                }

                if (definition.SourceType == ReclassifiedBalanceSheetRowCatalog.LegacyTotalizer)
                {
                    if (!string.Equals(
                            referenceMonth.MonthPainelContabilTotalizer?.Name,
                            definition.LegacySourceName,
                            StringComparison.Ordinal))
                        throw MissingSource(definition);

                    result.Add(new ReclassifiedBalanceSheetBoundRow(definition, null));
                    continue;
                }

                var source = referenceMonth.Totalizer
                    .FirstOrDefault(totalizer => string.Equals(
                        totalizer.Name,
                        definition.LegacySourceName,
                        StringComparison.Ordinal));
                if (source is null)
                    throw MissingSource(definition);

                result.Add(new ReclassifiedBalanceSheetBoundRow(definition, source.Id));
            }
        }

        return result;
    }

    private static ReclassifiedBalanceSheetV2MappingException MissingSource(
        ReclassifiedBalanceSheetRowDefinition definition) =>
        new($"Mapeamento do Balanço Reclassificado V2 ausente. Code: {definition.Code}; " +
            $"statementKey: {definition.StatementKey}; sourceType: {definition.SourceType}; " +
            $"legacySource: {definition.LegacySourceName}.");

    private static BalanceSheetRowDto MapRow(
        ReclassifiedBalanceSheetBoundRow bound,
        PainelBalancoComparativoResponse legacy,
        IReadOnlyList<BalanceSheetPeriodDto> periods,
        IReadOnlyList<BalanceSheetScenarioDto> scenarios)
    {
        var values = new Dictionary<string, Dictionary<string, decimal?>>(StringComparer.Ordinal);
        var counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        var data = bound.Definition.Expandable
            ? new Dictionary<string, Dictionary<string, IReadOnlyList<BalanceSheetTotalizerDetailDto>>>(StringComparer.Ordinal)
            : null;

        foreach (var scenario in scenarios)
        {
            var scenarioValues = new Dictionary<string, decimal?>(StringComparer.Ordinal);
            var scenarioCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var scenarioData = data is not null
                ? new Dictionary<string, IReadOnlyList<BalanceSheetTotalizerDetailDto>>(StringComparer.Ordinal)
                : null;
            var panel = GetPanel(legacy, scenario.Key);

            if (panel is not null)
            {
                foreach (var period in periods)
                {
                    var month = FindMonth(panel, period);
                    if (month is null)
                        continue;

                    if (!TryGetLegacyValue(
                            month,
                            bound,
                            out var value,
                            out var detailCount,
                            out var totalizerDetails))
                        throw new ReclassifiedBalanceSheetV2MappingException(
                            $"Origem do Balanço Reclassificado V2 não encontrada no resultado legado. " +
                            $"Scenario: {scenario.Key}; Period: {period.Key}; Code: {bound.Definition.Code}; " +
                            $"sourceType: {bound.Definition.SourceType}; sourceId: {bound.SourceId}.");

                    scenarioValues[period.Key] = value;
                    if (bound.Definition.Expandable)
                    {
                        scenarioCounts[period.Key] = detailCount;
                        scenarioData![period.Key] = totalizerDetails;
                    }
                }
            }

            values[scenario.Key] = scenarioValues;
            if (bound.Definition.Expandable)
            {
                counts[scenario.Key] = scenarioCounts;
                data![scenario.Key] = scenarioData!;
            }
        }

        return new BalanceSheetRowDto
        {
            Code = bound.Definition.Code,
            StatementKey = bound.Definition.StatementKey,
            Name = bound.Definition.Name,
            RowType = bound.Definition.RowType,
            ValueType = "currency",
            DisplayOrder = bound.Definition.DisplayOrder,
            Level = 0,
            ParentCode = null,
            Expandable = bound.Definition.Expandable,
            Source = new BalanceSheetRowSourceDto
            {
                SourceType = bound.Definition.SourceType,
                SourceId = bound.SourceId
            },
            Details = new BalanceSheetRowDetailsDto
            {
                Available = bound.Definition.Expandable,
                Counts = counts,
                Data = data
            },
            Values = values
        };
    }

    private static BalanceSheetRowDto MapCalculatedRow(
        ReclassifiedBalanceSheetRowDefinition definition,
        IReadOnlyList<BalanceSheetRowDto> legacyRows,
        IReadOnlyList<BalanceSheetPeriodDto> periods,
        IReadOnlyList<BalanceSheetScenarioDto> scenarios)
    {
        if (definition.Code != "BALANCE_DIFFERENCE")
            throw new ReclassifiedBalanceSheetV2MappingException(
                $"Cálculo do Balanço Reclassificado V2 não implementado. Code: {definition.Code}.");

        var totalAssets = legacyRows.Single(row => row.Code == "TOTAL_ASSETS");
        var totalLiabilities = legacyRows.Single(row => row.Code == "TOTAL_LIABILITIES");
        var values = new Dictionary<string, Dictionary<string, decimal?>>(StringComparer.Ordinal);

        foreach (var scenario in scenarios)
        {
            var scenarioValues = new Dictionary<string, decimal?>(StringComparer.Ordinal);
            foreach (var period in periods)
            {
                if (!totalAssets.Values[scenario.Key].TryGetValue(period.Key, out var assetValue) ||
                    !totalLiabilities.Values[scenario.Key].TryGetValue(period.Key, out var liabilityValue))
                    continue;

                scenarioValues[period.Key] = assetValue - liabilityValue;
            }

            values[scenario.Key] = scenarioValues;
        }

        return new BalanceSheetRowDto
        {
            Code = definition.Code,
            StatementKey = definition.StatementKey,
            Name = definition.Name,
            RowType = definition.RowType,
            ValueType = "currency",
            DisplayOrder = definition.DisplayOrder,
            Level = 0,
            ParentCode = null,
            Expandable = false,
            Source = new BalanceSheetRowSourceDto
            {
                SourceType = definition.SourceType,
                SourceId = null
            },
            Details = new BalanceSheetRowDetailsDto
            {
                Available = false,
                Counts = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal)
            },
            Values = values
        };
    }

    private static bool TryGetLegacyValue(
        MonthPainelContabilRespone month,
        ReclassifiedBalanceSheetBoundRow bound,
        out decimal value,
        out int detailCount,
        out IReadOnlyList<BalanceSheetTotalizerDetailDto> totalizerDetails)
    {
        value = default;
        detailCount = 0;
        totalizerDetails = Array.Empty<BalanceSheetTotalizerDetailDto>();

        if (bound.Definition.SourceType == ReclassifiedBalanceSheetRowCatalog.LegacyTotalizer)
        {
            var total = month.MonthPainelContabilTotalizer;
            if (total is null || !string.Equals(
                    total.Name,
                    bound.Definition.LegacySourceName,
                    StringComparison.Ordinal))
                return false;

            value = total.TotalValue;
            return true;
        }

        var group = month.Totalizer?
            .SingleOrDefault(totalizer => totalizer.Id == bound.SourceId);
        if (group is null)
            return false;

        value = group.TotalValue;
        var monthTotalizers = month.Totalizer ?? new List<TotalizerParentRespone>();
        var missingDetailTotalizers = bound.Definition.LegacyDetailTotalizerNames
            .Where(detailName => !monthTotalizers.Any(totalizer =>
                string.Equals(totalizer.Name, detailName, StringComparison.Ordinal)))
            .ToArray();

        if (missingDetailTotalizers.Length > 0)
            throw new ReclassifiedBalanceSheetV2MappingException(
                $"Totalizador(es) de detalhe ausente(s) no Balanço Reclassificado V2. " +
                $"Code: {bound.Definition.Code}; missing: [{string.Join(", ", missingDetailTotalizers)}].");

        var detailTotalizers = bound.Definition.LegacyDetailTotalizerNames
            .Select(detailName => monthTotalizers.First(totalizer =>
                string.Equals(totalizer.Name, detailName, StringComparison.Ordinal)))
            .ToArray();

        totalizerDetails = detailTotalizers
            .Select(totalizer => MapTotalizer(totalizer, monthTotalizers))
            .ToArray();
        detailCount = totalizerDetails.Sum(totalizer =>
            totalizer.Classifications.Sum(classification => classification.Datas.Count));
        return true;
    }

    private static BalanceSheetTotalizerDetailDto MapTotalizer(
        TotalizerParentRespone totalizer,
        IReadOnlyList<TotalizerParentRespone> monthTotalizers)
    {
        var classifications = (totalizer.Classifications ?? new List<ClassificationRespone>())
            .Select(MapClassification)
            .ToArray();

        if (NestedCalculatedTotalizerSources.TryGetValue(totalizer.Name, out var sourceNames))
        {
            var sources = sourceNames
                .Select(sourceName => monthTotalizers.FirstOrDefault(candidate =>
                    string.Equals(candidate.Name, sourceName, StringComparison.Ordinal)))
                .ToArray();
            var missingSources = sourceNames
                .Where((_, index) => sources[index] is null)
                .ToArray();

            if (missingSources.Length > 0)
                throw new ReclassifiedBalanceSheetV2MappingException(
                    $"Componente(s) de detalhe ausente(s) no Balanço Reclassificado V2. " +
                    $"Totalizer: {totalizer.Name}; missing: [{string.Join(", ", missingSources)}].");

            classifications = sources
                .Select(source => MapNestedTotalizerAsClassification(source!))
                .ToArray();
        }

        return new BalanceSheetTotalizerDetailDto
        {
            Id = totalizer.Id,
            TypeOrder = totalizer.TypeOrder,
            Name = totalizer.Name,
            TotalValue = totalizer.TotalValue,
            Expandable = classifications.Length > 0,
            Classifications = classifications
        };
    }

    private static BalanceSheetClassificationDetailDto MapNestedTotalizerAsClassification(
        TotalizerParentRespone totalizer) =>
        new()
        {
            Id = totalizer.Id,
            TypeOrder = totalizer.TypeOrder,
            Name = totalizer.Name,
            Value = totalizer.TotalValue,
            Datas = (totalizer.Classifications ?? new List<ClassificationRespone>())
                .SelectMany(classification => classification.Datas ?? new List<BalanceteDataResponse>())
                .Select(MapAccountingData)
                .ToArray()
        };

    private static BalanceSheetClassificationDetailDto MapClassification(
        ClassificationRespone classification) =>
        new()
        {
            Id = classification.Id,
            TypeOrder = classification.TypeOrder,
            Name = classification.Name,
            Value = classification.Value,
            Datas = (classification.Datas ?? new List<BalanceteDataResponse>())
                .Select(MapAccountingData)
                .ToArray()
        };

    private static BalanceSheetAccountingDataDto MapAccountingData(BalanceteDataResponse data) =>
        new()
        {
            Id = data.Id,
            TypeOrder = data.TypeOrder,
            Name = data.Name,
            CostCenter = data.CostCenter,
            InitialValue = data.InitialValue,
            CreditValue = data.CreditValue,
            DebitValue = data.DebitValue,
            Value = data.Value
        };

    private static IReadOnlyList<BalanceSheetPeriodDto> GetPeriods(
        PainelBalancoComparativoResponse assets,
        PainelBalancoComparativoResponse liabilities,
        int year) =>
        new[] { assets, liabilities }
            .SelectMany(EnumeratePanels)
            .SelectMany(panel => panel.Months ?? new List<MonthPainelContabilRespone>())
            .Select(month => month.DateMonth)
            .Distinct()
            .OrderBy(month => month == 13 ? int.MaxValue : month)
            .Select(month => MapPeriod(month, year))
            .ToArray();

    private static BalanceSheetPeriodDto MapPeriod(int month, int year)
    {
        if (month == 13)
        {
            return new BalanceSheetPeriodDto
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
            throw new ReclassifiedBalanceSheetV2MappingException($"Período legado inválido: {month}.");

        return new BalanceSheetPeriodDto
        {
            Key = $"{year:D4}-{month:D2}",
            Label = MonthLabels[month - 1],
            Year = year,
            Month = month,
            Type = "month",
            DisplayOrder = month
        };
    }

    private static IReadOnlyList<BalanceSheetScenarioDto> GetScenarios() =>
        ScenarioDefinitions
            .Select(definition => new BalanceSheetScenarioDto
            {
                Key = definition.Key,
                Label = definition.Label,
                DisplayOrder = definition.Order
            })
            .ToArray();

    private static IReadOnlyList<BalanceSheetStatementDto> GetStatements() =>
        new[]
        {
            new BalanceSheetStatementDto
            {
                Key = ReclassifiedBalanceSheetRowCatalog.Asset,
                Label = "ATIVO",
                DisplayOrder = 1,
                TotalRowCode = "TOTAL_ASSETS"
            },
            new BalanceSheetStatementDto
            {
                Key = ReclassifiedBalanceSheetRowCatalog.Liability,
                Label = "PASSIVO",
                DisplayOrder = 2,
                TotalRowCode = "TOTAL_LIABILITIES"
            }
        };

    private static IEnumerable<PainelBalancoContabilRespone> EnumeratePanels(
        PainelBalancoComparativoResponse response)
    {
        if (response.Realizado is not null)
            yield return response.Realizado;
        if (response.Orcado is not null)
            yield return response.Orcado;
        if (response.Variacao is not null)
            yield return response.Variacao;
    }

    private static PainelBalancoContabilRespone? GetPanel(
        PainelBalancoComparativoResponse response,
        string scenario) => scenario switch
    {
        "realizado" => response.Realizado,
        "orcado" => response.Orcado,
        "variacao" => response.Variacao,
        _ => null
    };

    private static MonthPainelContabilRespone? FindMonth(
        PainelBalancoContabilRespone panel,
        BalanceSheetPeriodDto period)
    {
        var month = period.Type == "accumulated" ? 13 : period.Month;
        return panel.Months?.FirstOrDefault(item => item.DateMonth == month);
    }
}
