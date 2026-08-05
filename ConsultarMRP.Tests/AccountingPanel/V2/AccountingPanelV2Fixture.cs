using _2___Application._1_Services.AccountingPanel.V2;
using _2___Application._2_Dto_s.TotalizerClassification;

namespace ConsultarMRP.Tests.AccountingPanel.V2;

internal sealed class AccountingPanelV2Fixture
{
    public const int Year = 2026;

    private readonly Dictionary<(string Statement, int Month, string Code), decimal> _expected = new();

    public AccountingPanelV2Fixture()
    {
        Assets = BuildPanel(AccountingPanelRowCatalog.Asset, new[] { 1, 2 });
        Liabilities = BuildPanel(AccountingPanelRowCatalog.Liability, new[] { 1 });
    }

    public PainelBalancoContabilRespone Assets { get; }
    public PainelBalancoContabilRespone Liabilities { get; }

    public PainelBalancoContabilRespone Statement(string statementKey) =>
        statementKey == AccountingPanelRowCatalog.Asset ? Assets : Liabilities;

    public decimal Expected(string statementKey, int month, string code) =>
        _expected[(statementKey, month, code)];

    private PainelBalancoContabilRespone BuildPanel(string statementKey, IEnumerable<int> months) =>
        new()
        {
            Months = months.Select(month => BuildMonth(statementKey, month)).ToList()
        };

    private MonthPainelContabilRespone BuildMonth(string statementKey, int month)
    {
        var totalizers = AccountingPanelRowCatalog.All
            .Where(definition =>
                definition.StatementKey == statementKey &&
                definition.SourceType == AccountingPanelRowCatalog.Totalizer)
            .Select(definition =>
            {
                var totalizerValue = Value(statementKey, month, definition.DisplayOrder);
                _expected[(statementKey, month, definition.Code)] = totalizerValue;

                var classifications = AccountingPanelRowCatalog.All
                    .Where(child => child.ParentCode == definition.Code)
                    .Select(child =>
                    {
                        var classificationValue = Value(statementKey, month, child.DisplayOrder);
                        _expected[(statementKey, month, child.Code)] = classificationValue;

                        return new ClassificationRespone
                        {
                            Id = 20_000 + child.DisplayOrder,
                            TypeOrder = 7,
                            Name = child.LegacySourceName,
                            Value = classificationValue,
                            Datas = new List<BalanceteDataResponse>
                            {
                                new()
                                {
                                    Id = 30_000 + child.DisplayOrder + month,
                                    TypeOrder = child.DisplayOrder / 10,
                                    Name = $"Conta {child.Code}",
                                    CostCenter = $"1.{child.DisplayOrder}",
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
                    Id = 10_000 + definition.DisplayOrder,
                    TypeOrder = definition.DisplayOrder / 10,
                    Name = definition.LegacySourceName,
                    TotalValue = totalizerValue,
                    Classifications = classifications
                };
            })
            .ToList();

        var totalDefinition = AccountingPanelRowCatalog.All.Single(definition =>
            definition.StatementKey == statementKey &&
            definition.SourceType == AccountingPanelRowCatalog.GeneralTotal);
        var totalValue = 900_000m + StatementOffset(statementKey) + month;
        _expected[(statementKey, month, totalDefinition.Code)] = totalValue;

        return new MonthPainelContabilRespone
        {
            Id = month,
            Name = $"MÊS {month}",
            DateMonth = month,
            Totalizer = totalizers,
            MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
            {
                Name = totalDefinition.LegacySourceName,
                TotalValue = totalValue
            }
        };
    }

    private static decimal Value(string statementKey, int month, int displayOrder) =>
        StatementOffset(statementKey) + month * 100m + displayOrder;

    private static int StatementOffset(string statementKey) =>
        statementKey == AccountingPanelRowCatalog.Asset ? 1_000 : -2_000;
}
