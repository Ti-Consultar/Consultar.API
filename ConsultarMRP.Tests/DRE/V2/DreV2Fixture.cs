using _2___Application._1_Services.DRE.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

namespace ConsultarMRP.Tests.DRE.V2;

internal sealed class DreV2Fixture
{
    public const int Year = 2026;

    private readonly Dictionary<(string Scenario, string Period, string Code), decimal> _expected = new();
    private readonly Dictionary<string, int> _totalizerIds;
    private readonly Dictionary<string, int> _classificationIds;

    public DreV2Fixture()
    {
        _totalizerIds = DreRowCatalog.All
            .Where(item => item.SourceType == DreRowCatalog.Totalizer)
            .Select((item, index) => (item.LegacySourceName, Id: 10_000 + index))
            .ToDictionary(item => item.LegacySourceName, item => item.Id, StringComparer.Ordinal);

        _classificationIds = DreRowCatalog.All
            .Where(item => item.SourceType == DreRowCatalog.Classification)
            .Select((item, index) => (item.Code, Id: 20_000 + index))
            .ToDictionary(item => item.Code, item => item.Id, StringComparer.Ordinal);

        // A mesma classificação física aparece nos dois contextos financeiros.
        _classificationIds["EBITDA_DEPRECIATION_ADDBACK"] =
            _classificationIds["DEPRECIATION_EXPENSE"];

        Legacy = new PainelBalancoComparativoResponse
        {
            Realizado = BuildPanel("realizado", 0),
            Orcado = BuildPanel("orcado", 1_000),
            Variacao = BuildPanel("variacao", -2_000)
        };
    }

    public PainelBalancoComparativoResponse Legacy { get; }

    public decimal Expected(string scenario, string period, string code) =>
        code == "FINANCIAL_RESULT"
            ? _expected[(scenario, period, "FINANCIAL_INCOME")] +
              _expected[(scenario, period, "FINANCIAL_EXPENSES")]
            : _expected[(scenario, period, code)];

    public PainelBalancoContabilRespone CreatePanel(
        string scenario,
        int scenarioOffset,
        IEnumerable<int> months)
    {
        var monthly = months
            .Distinct()
            .OrderBy(month => month)
            .Select(month => BuildMonth(
                scenario,
                month,
                $"{Year:D4}-{month:D2}",
                scenarioOffset))
            .ToList();
        monthly.Add(BuildMonth(scenario, 13, "accumulated", scenarioOffset));
        return new PainelBalancoContabilRespone { Months = monthly };
    }

    private PainelBalancoContabilRespone BuildPanel(string scenario, int scenarioOffset) =>
        new()
        {
            Months = new List<MonthPainelContabilRespone>
            {
                BuildMonth(scenario, 1, "2026-01", scenarioOffset),
                BuildMonth(scenario, 2, "2026-02", scenarioOffset),
                BuildMonth(scenario, 13, "accumulated", scenarioOffset)
            }
        };

    private MonthPainelContabilRespone BuildMonth(
        string scenario,
        int month,
        string period,
        int scenarioOffset)
    {
        var totalizers = DreRowCatalog.All
            .Where(item => item.SourceType == DreRowCatalog.Totalizer)
            .Select((definition, index) =>
            {
                var value = ValueFor(definition, scenario, month, scenarioOffset);
                _expected[(scenario, period, definition.Code)] = value;

                var classifications = DreRowCatalog.All
                    .Where(item =>
                        item.SourceType == DreRowCatalog.Classification &&
                        item.LegacyParentTotalizerName == definition.LegacySourceName)
                    .Select(classification =>
                    {
                        var classificationValue = ValueFor(
                            classification,
                            scenario,
                            month,
                            scenarioOffset);
                        _expected[(scenario, period, classification.Code)] = classificationValue;

                        return new ClassificationRespone
                        {
                            Id = _classificationIds[classification.Code],
                            Name = classification.LegacySourceName,
                            // Duplicado de propósito: identidade não pode depender de TypeOrder.
                            TypeOrder = 7,
                            Value = classificationValue,
                            Datas = month == 13
                                ? new List<BalanceteDataResponse>()
                                : new List<BalanceteDataResponse>
                                {
                                    new()
                                    {
                                        Id = definition.DisplayOrder + classification.DisplayOrder + month,
                                        Name = $"Conta {classification.Code}",
                                        CostCenter = $"3.{classification.DisplayOrder}",
                                        InitialValue = classificationValue - 1,
                                        CreditValue = classificationValue > 0 ? classificationValue : 0,
                                        DebitValue = classificationValue < 0 ? -classificationValue : 0,
                                        Value = classificationValue
                                    }
                                }
                        };
                    })
                    .ToList();

                return new TotalizerParentRespone
                {
                    Id = _totalizerIds[definition.LegacySourceName],
                    Name = definition.LegacySourceName,
                    // Duplicado de propósito em totalizadores diferentes.
                    TypeOrder = index % 3,
                    TotalValue = value,
                    Classifications = classifications
                };
            })
            .ToList();

        return new MonthPainelContabilRespone
        {
            Id = month == 13 ? 0 : month,
            Name = month == 13 ? "ACUMULADO" : period,
            DateMonth = month,
            Totalizer = totalizers
        };
    }

    private static decimal ValueFor(
        DreRowDefinition definition,
        string scenario,
        int month,
        int scenarioOffset)
    {
        if (definition.Code == "TAXES_AND_CONTRIBUTIONS" && month == 2)
            return 0m;

        if (definition.Code == "DEPRECIATION_EXPENSE")
            return month == 13 ? -999m : -50m - month;

        if (definition.Code == "EBITDA_DEPRECIATION_ADDBACK")
            return month == 13 ? 999m : 50m + month;

        // O acumulado e a variação são deliberadamente diferentes de qualquer
        // recomposição possível a partir dos outros cenários/períodos.
        if (month == 13)
            return 70_000m + scenarioOffset + definition.DisplayOrder / 10m;
        if (scenario == "variacao")
            return -30_000m - definition.DisplayOrder - month;

        return scenarioOffset + definition.DisplayOrder * 10m + month;
    }
}
