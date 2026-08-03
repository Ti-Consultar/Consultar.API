using _2___Application._1_Services.ReclassifiedBalanceSheet.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

namespace ConsultarMRP.Tests.ReclassifiedBalanceSheet.V2;

internal sealed class ReclassifiedBalanceSheetV2Fixture
{
    private static readonly IReadOnlyDictionary<string, string[]> NestedCalculatedTotalizerSources =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Outros Ativos Operacionais Total"] =
                new[] { "Outros Ativos Operacionais", "Contas Transitórias Ativo" },
            ["Outros Passivos Operacionais Total"] =
                new[] { "Outros Passivos Operacionais", "Contas Transitórias Passivo" }
        };

    public const int Year = 2026;

    public ReclassifiedBalanceSheetV2Fixture()
    {
        Assets = BuildStatement(ReclassifiedBalanceSheetRowCatalog.Asset);
        Liabilities = BuildStatement(ReclassifiedBalanceSheetRowCatalog.Liability);
    }

    public PainelBalancoComparativoResponse Assets { get; }
    public PainelBalancoComparativoResponse Liabilities { get; }

    public PainelBalancoComparativoResponse Statement(string statementKey) =>
        statementKey == ReclassifiedBalanceSheetRowCatalog.Asset ? Assets : Liabilities;

    public static PainelBalancoContabilRespone? Panel(
        PainelBalancoComparativoResponse response,
        string scenario) => scenario switch
    {
        "realizado" => response.Realizado,
        "orcado" => response.Orcado,
        "variacao" => response.Variacao,
        _ => null
    };

    public static decimal LegacyValue(
        PainelBalancoComparativoResponse response,
        string scenario,
        int month,
        ReclassifiedBalanceSheetRowDefinition definition)
    {
        var legacyMonth = Panel(response, scenario)!.Months.Single(item => item.DateMonth == month);
        if (definition.SourceType == ReclassifiedBalanceSheetRowCatalog.LegacyTotalizer)
            return legacyMonth.MonthPainelContabilTotalizer.TotalValue;

        return legacyMonth.Totalizer
            .Single(item => item.Name == definition.LegacySourceName)
            .TotalValue;
    }

    private static PainelBalancoComparativoResponse BuildStatement(string statementKey) =>
        new()
        {
            Realizado = BuildPanel(statementKey, "realizado", 0, new[] { 1, 2, 13 }),
            Orcado = BuildPanel(
                statementKey,
                "orcado",
                10_000,
                statementKey == ReclassifiedBalanceSheetRowCatalog.Asset
                    ? new[] { 1, 13 }
                    : new[] { 1, 2, 13 }),
            Variacao = BuildPanel(statementKey, "variacao", -20_000, new[] { 1, 2, 13 })
        };

    private static PainelBalancoContabilRespone BuildPanel(
        string statementKey,
        string scenario,
        int scenarioOffset,
        IEnumerable<int> months) =>
        new()
        {
            Months = months
                .Select(month => BuildMonth(statementKey, scenario, scenarioOffset, month))
                .ToList()
        };

    private static MonthPainelContabilRespone BuildMonth(
        string statementKey,
        string scenario,
        int scenarioOffset,
        int month)
    {
        var definitions = ReclassifiedBalanceSheetRowCatalog.All
            .Where(definition =>
                definition.StatementKey == statementKey &&
                definition.SourceType == ReclassifiedBalanceSheetRowCatalog.LegacyGroup)
            .ToArray();

        var groups = definitions.Select(definition =>
        {
            var value = GroupValue(statementKey, scenario, scenarioOffset, month, definition);

            return new TotalizerParentRespone
            {
                Id = 1_000 + definition.DisplayOrder,
                Name = definition.LegacySourceName,
                TypeOrder = 7,
                TotalValue = value,
                Classifications = new List<ClassificationRespone>()
            };
        }).ToList();

        var componentTotalizers = definitions
            .SelectMany(definition => definition.LegacyDetailTotalizerNames
                .Distinct(StringComparer.Ordinal)
                .Select((detailName, detailIndex) =>
                {
                    var value = GroupValue(statementKey, scenario, scenarioOffset, month, definition) + detailIndex;
                    var hasDetails = month != 13 && scenario != "variacao";
                    var isNestedCalculatedTotalizer =
                        NestedCalculatedTotalizerSources.ContainsKey(detailName);

                    return new TotalizerParentRespone
                    {
                        Id = 4_000 + definition.DisplayOrder * 10 + detailIndex,
                        Name = detailName,
                        TypeOrder = detailIndex + 1,
                        TotalValue = value,
                        Classifications = isNestedCalculatedTotalizer
                            ? new List<ClassificationRespone>()
                            : new List<ClassificationRespone>
                        {
                            new()
                            {
                                Id = 5_000 + definition.DisplayOrder * 10 + detailIndex,
                                Name = $"Classificação {detailName}",
                                TypeOrder = 3,
                                Value = value,
                                Datas = hasDetails
                                    ? new List<BalanceteDataResponse>
                                    {
                                        new()
                                        {
                                            Id = 6_000 + definition.DisplayOrder * 10 + detailIndex + month,
                                            Name = $"Conta {detailName}",
                                            CostCenter = $"1.{definition.DisplayOrder}.{detailIndex}",
                                            InitialValue = value - 1,
                                            CreditValue = value > 0 ? value : 0,
                                            DebitValue = value < 0 ? -value : 0,
                                            Value = value
                                        }
                                    }
                                    : new List<BalanceteDataResponse>()
                            }
                        }
                    };
                }))
            .ToList();

        var nestedComponentTotalizers = definitions
            .SelectMany(definition => definition.LegacyDetailTotalizerNames
                .Where(NestedCalculatedTotalizerSources.ContainsKey)
                .SelectMany(detailName => NestedCalculatedTotalizerSources[detailName]
                    .Select((sourceName, sourceIndex) =>
                    {
                        var value = GroupValue(statementKey, scenario, scenarioOffset, month, definition) +
                                    sourceIndex;
                        var hasDetails = month != 13 && scenario != "variacao";

                        return new TotalizerParentRespone
                        {
                            Id = 7_000 + definition.DisplayOrder * 10 + sourceIndex,
                            Name = sourceName,
                            TypeOrder = sourceIndex + 1,
                            TotalValue = value,
                            Classifications = new List<ClassificationRespone>
                            {
                                new()
                                {
                                    Id = 8_000 + definition.DisplayOrder * 10 + sourceIndex,
                                    Name = $"Classificação interna {sourceName}",
                                    TypeOrder = sourceIndex + 1,
                                    Value = value,
                                    Datas = hasDetails
                                        ? new List<BalanceteDataResponse>
                                        {
                                            new()
                                            {
                                                Id = 9_000 + definition.DisplayOrder * 10 + sourceIndex + month,
                                                Name = $"Conta {sourceName}",
                                                CostCenter = $"2.{definition.DisplayOrder}.{sourceIndex}",
                                                InitialValue = value - 1,
                                                CreditValue = value > 0 ? value : 0,
                                                DebitValue = value < 0 ? -value : 0,
                                                Value = value
                                            }
                                        }
                                        : new List<BalanceteDataResponse>()
                                }
                            }
                        };
                    })))
            .ToList();

        groups.AddRange(componentTotalizers);
        groups.AddRange(nestedComponentTotalizers);

        return new MonthPainelContabilRespone
        {
            Id = month == 13 ? 0 : month,
            Name = month == 13 ? "ACUMULADO" : $"MÊS {month}",
            DateMonth = month,
            Totalizer = groups,
            MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
            {
                Name = statementKey == ReclassifiedBalanceSheetRowCatalog.Asset
                    ? "TOTAL DO ATIVO"
                    : "TOTAL DO PASSIVO",
                // Deliberadamente não corresponde à soma dos grupos.
                TotalValue = 900_000m + StatementOffset(statementKey) + scenarioOffset + month
            }
        };
    }

    private static decimal GroupValue(
        string statementKey,
        string scenario,
        int scenarioOffset,
        int month,
        ReclassifiedBalanceSheetRowDefinition definition)
    {
        if (statementKey == ReclassifiedBalanceSheetRowCatalog.Asset &&
            scenario == "realizado" && month == 1 && definition.Code == "FINANCIAL_ASSETS")
            return -321.45m;

        if (statementKey == ReclassifiedBalanceSheetRowCatalog.Asset &&
            scenario == "realizado" && month == 2 && definition.Code == "OPERATING_ASSETS")
            return 0m;

        // O acumulado é propositalmente diferente da soma dos meses.
        if (month == 13)
            return 700_000m + StatementOffset(statementKey) + scenarioOffset + definition.DisplayOrder;

        return StatementOffset(statementKey) + scenarioOffset + month * 100m + definition.DisplayOrder;
    }

    private static int StatementOffset(string statementKey) =>
        statementKey == ReclassifiedBalanceSheetRowCatalog.Asset ? 1_000 : 2_000;
}
