using _2___Application._1_Services.TotalizerClassification;
using _2___Application._2_Dto_s.TotalizerClassification;
using Xunit;

namespace ConsultarMRP.Tests.Accounting;

public sealed class ReclassifiedBalanceSheetAggregationTests
{
    [Fact]
    public void Groups_duplicate_canonical_names_without_losing_values_or_details()
    {
        var totalizers = new[]
        {
            Totalizer(1, "Ativo Financeiro", 100m, Classification(11, "Caixa", 100m, 111)),
            Totalizer(2, "ativo financeiro", 225m, Classification(22, "Caixa", 225m, 222))
        };

        var totalizerMap = ReclassifiedBalanceSheetAggregation.BuildTotalizerMap(totalizers);
        var classificationMap = ReclassifiedBalanceSheetAggregation.BuildClassificationMap(totalizers);

        var financialAssets = Assert.Single(totalizerMap);
        Assert.Equal("Ativo Financeiro", financialAssets.Key);
        Assert.Equal(325m, financialAssets.Value.TotalValue);
        Assert.Equal(2, financialAssets.Value.Classifications.Count);

        var cash = Assert.Single(classificationMap);
        Assert.Equal("Caixa", cash.Key);
        Assert.Equal(325m, cash.Value.Value);
        Assert.Equal(new[] { 111, 222 }, cash.Value.Datas.Select(data => data.Id));
    }

    [Fact]
    public void Recalculates_equity_after_replacing_accumulated_result_so_january_does_not_duplicate_dre()
    {
        var equity = Totalizer(
            1,
            "Patrimônio Liquido",
            -1_325m,
            Classification(11, "Capital e Reservas", -1_000m, 111),
            Classification(12, "Resultado do Exercício Acumulado", 325m, 112));

        var recalculatedEquity = ReclassifiedBalanceSheetAggregation.RecalculateTotalValue(equity);
        var accumulatedResult = equity.Classifications
            .Single(classification => classification.Name == "Resultado do Exercício Acumulado")
            .Value;
        var decemberEquity = -1_000m;
        var januaryVariation = (recalculatedEquity - accumulatedResult) - decemberEquity;

        Assert.Equal(-675m, recalculatedEquity);
        Assert.Equal(0m, januaryVariation);
    }

    private static TotalizerParentRespone Totalizer(
        int id,
        string name,
        decimal totalValue,
        params ClassificationRespone[] classifications) =>
        new()
        {
            Id = id,
            Name = name,
            TotalValue = totalValue,
            Classifications = classifications.ToList()
        };

    private static ClassificationRespone Classification(
        int id,
        string name,
        decimal value,
        int dataId) =>
        new()
        {
            Id = id,
            Name = name,
            Value = value,
            Datas = new List<BalanceteDataResponse>
            {
                new() { Id = dataId, Name = $"Conta {dataId}", Value = value }
            }
        };
}
