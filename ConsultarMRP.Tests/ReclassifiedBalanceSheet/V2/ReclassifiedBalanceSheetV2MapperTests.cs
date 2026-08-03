using System.Reflection;
using System.Text.Json;
using _2___Application._1_Services;
using _2___Application._1_Services.ReclassifiedBalanceSheet.V2;
using _2___Application._2_Dto_s.ReclassifiedBalanceSheet.V2;
using ConsultarMRP.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ConsultarMRP.Tests.ReclassifiedBalanceSheet.V2;

public sealed class ReclassifiedBalanceSheetV2MapperTests
{
    [Fact]
    public void Returns_asset_and_liability_with_unique_codes_canonical_order_and_statements()
    {
        var fixture = new ReclassifiedBalanceSheetV2Fixture();

        var response = ReclassifiedBalanceSheetV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            ReclassifiedBalanceSheetV2Fixture.Year);
        var rows = response.Data.Rows.ToArray();

        Assert.Equal(new[] { "2026-01", "2026-02", "accumulated" },
            response.Data.Periods.Select(period => period.Key));
        Assert.Equal(new[] { "realizado", "orcado", "variacao" },
            response.Data.Scenarios.Select(scenario => scenario.Key));
        Assert.Equal(new[] { "asset", "liability" },
            response.Data.Statements.Select(statement => statement.Key));
        Assert.Equal(new[] { "TOTAL_ASSETS", "TOTAL_LIABILITIES" },
            response.Data.Statements.Select(statement => statement.TotalRowCode));
        Assert.Equal(11, rows.Length);
        Assert.Equal(rows.Length, rows.Select(row => row.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(rows.OrderBy(row => row.DisplayOrder).Select(row => row.Code),
            rows.Select(row => row.Code));
        Assert.Equal(
            new[]
            {
                "FINANCIAL_ASSETS",
                "OPERATING_ASSETS",
                "NON_CURRENT_ASSETS",
                "FIXED_ASSETS",
                "TOTAL_ASSETS",
                "FINANCIAL_LIABILITIES",
                "OPERATING_LIABILITIES",
                "NON_CURRENT_LIABILITIES",
                "EQUITY",
                "TOTAL_LIABILITIES",
                "BALANCE_DIFFERENCE"
            },
            rows.Select(row => row.Code));
        Assert.All(rows.Where(row => row.DisplayOrder <= 50),
            row => Assert.Equal("asset", row.StatementKey));
        Assert.All(rows.Where(row => row.DisplayOrder >= 60),
            row => Assert.Equal("liability", row.StatementKey));

        Assert.All(rows.Where(row => row.RowType == "section"), row =>
        {
            Assert.True(row.Expandable);
            Assert.True(row.Details.Available);
            Assert.Equal("legacyGroup", row.Source.SourceType);
            Assert.NotNull(row.Source.SourceId);
        });
        Assert.All(rows.Where(row => row.RowType == "total"), row =>
        {
            Assert.False(row.Expandable);
            Assert.False(row.Details.Available);
            Assert.Empty(row.Details.Counts);
            Assert.Equal("legacyTotalizer", row.Source.SourceType);
            Assert.Null(row.Source.SourceId);
        });

        var balanceDifference = Row(response, "BALANCE_DIFFERENCE");
        Assert.Equal("liability", balanceDifference.StatementKey);
        Assert.Equal("validation", balanceDifference.RowType);
        Assert.Equal(110, balanceDifference.DisplayOrder);
        Assert.Equal(balanceDifference.Code, rows[^1].Code);
        Assert.False(balanceDifference.Expandable);
        Assert.Equal("calculated", balanceDifference.Source.SourceType);
    }

    [Fact]
    public void Copies_every_available_legacy_value_without_recalculating_totals_variation_or_accumulated()
    {
        var fixture = new ReclassifiedBalanceSheetV2Fixture();
        var response = ReclassifiedBalanceSheetV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            ReclassifiedBalanceSheetV2Fixture.Year);

        foreach (var row in response.Data.Rows)
        {
            var definition = ReclassifiedBalanceSheetRowCatalog.All.Single(item => item.Code == row.Code);
            if (definition.SourceType == ReclassifiedBalanceSheetRowCatalog.Calculated)
                continue;

            var statement = fixture.Statement(row.StatementKey);

            foreach (var scenario in response.Data.Scenarios)
            foreach (var period in response.Data.Periods)
            {
                var panel = ReclassifiedBalanceSheetV2Fixture.Panel(statement, scenario.Key)!;
                var monthNumber = period.Type == "accumulated" ? 13 : period.Month!.Value;
                var legacyMonth = panel.Months.FirstOrDefault(month => month.DateMonth == monthNumber);

                if (legacyMonth is null)
                {
                    Assert.False(row.Values[scenario.Key].ContainsKey(period.Key));
                    continue;
                }

                Assert.Equal(
                    ReclassifiedBalanceSheetV2Fixture.LegacyValue(
                        statement,
                        scenario.Key,
                        monthNumber,
                        definition),
                    row.Values[scenario.Key][period.Key]);
            }
        }

        Assert.Equal(-321.45m,
            Row(response, "FINANCIAL_ASSETS").Values["realizado"]["2026-01"]);
        Assert.Equal(0m,
            Row(response, "OPERATING_ASSETS").Values["realizado"]["2026-02"]);
        Assert.False(Row(response, "FINANCIAL_ASSETS").Values["orcado"].ContainsKey("2026-02"));

        Assert.Equal(
            fixture.Assets.Realizado.Months.Single(month => month.DateMonth == 13)
                .MonthPainelContabilTotalizer.TotalValue,
            Row(response, "TOTAL_ASSETS").Values["realizado"]["accumulated"]);
        Assert.Equal(
            fixture.Liabilities.Variacao.Months.Single(month => month.DateMonth == 1)
                .MonthPainelContabilTotalizer.TotalValue,
            Row(response, "TOTAL_LIABILITIES").Values["variacao"]["2026-01"]);
    }

    [Fact]
    public void Calculates_balance_difference_from_legacy_totals_for_every_available_cell()
    {
        var fixture = new ReclassifiedBalanceSheetV2Fixture();
        var response = ReclassifiedBalanceSheetV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            ReclassifiedBalanceSheetV2Fixture.Year);
        var totalAssets = Row(response, "TOTAL_ASSETS");
        var totalLiabilities = Row(response, "TOTAL_LIABILITIES");
        var difference = Row(response, "BALANCE_DIFFERENCE");

        foreach (var scenario in response.Data.Scenarios)
        foreach (var period in response.Data.Periods)
        {
            var hasAssets = totalAssets.Values[scenario.Key].TryGetValue(period.Key, out var assetValue);
            var hasLiabilities = totalLiabilities.Values[scenario.Key].TryGetValue(period.Key, out var liabilityValue);

            if (!hasAssets || !hasLiabilities)
            {
                Assert.False(difference.Values[scenario.Key].ContainsKey(period.Key));
                continue;
            }

            Assert.Equal(
                assetValue - liabilityValue,
                difference.Values[scenario.Key][period.Key]);
        }

        Assert.Empty(difference.Details.Counts);
        Assert.False(difference.Details.Available);
    }

    [Fact]
    public void Embeds_all_legacy_classification_and_accounting_data_by_scenario_and_period()
    {
        var fixture = new ReclassifiedBalanceSheetV2Fixture();
        var response = ReclassifiedBalanceSheetV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            ReclassifiedBalanceSheetV2Fixture.Year);

        var financialAssets = Row(response, "FINANCIAL_ASSETS");
        Assert.Equal(2, financialAssets.Details.Counts["realizado"]["2026-01"]);
        Assert.Equal(2, financialAssets.Details.Counts["orcado"]["2026-01"]);
        Assert.Equal(0, financialAssets.Details.Counts["variacao"]["2026-01"]);
        Assert.Equal(0, financialAssets.Details.Counts["realizado"]["accumulated"]);

        var legacyTotalizer = fixture.Assets.Realizado.Months
            .Single(month => month.DateMonth == 1)
            .Totalizer.Single(totalizer => totalizer.Name == "Caixa e Equivalente de Caixa");
        var legacyClassification = Assert.Single(legacyTotalizer.Classifications);
        var legacyData = Assert.Single(legacyClassification.Datas);
        var totalizer = financialAssets.Details.Data!["realizado"]["2026-01"].First();
        var classification = Assert.Single(totalizer.Classifications);
        var accountingData = Assert.Single(classification.Datas);

        Assert.Equal(legacyTotalizer.Id, totalizer.Id);
        Assert.Equal(legacyTotalizer.TypeOrder, totalizer.TypeOrder);
        Assert.Equal(legacyTotalizer.Name, totalizer.Name);
        Assert.Equal(legacyTotalizer.TotalValue, totalizer.TotalValue);
        Assert.True(totalizer.Expandable);
        Assert.Equal(legacyClassification.Id, classification.Id);
        Assert.Equal(legacyClassification.TypeOrder, classification.TypeOrder);
        Assert.Equal(legacyClassification.Name, classification.Name);
        Assert.Equal(legacyClassification.Value, classification.Value);
        Assert.Equal(legacyData.Id, accountingData.Id);
        Assert.Equal(legacyData.TypeOrder, accountingData.TypeOrder);
        Assert.Equal(legacyData.Name, accountingData.Name);
        Assert.Equal(legacyData.CostCenter, accountingData.CostCenter);
        Assert.Equal(legacyData.InitialValue, accountingData.InitialValue);
        Assert.Equal(legacyData.CreditValue, accountingData.CreditValue);
        Assert.Equal(legacyData.DebitValue, accountingData.DebitValue);
        Assert.Equal(legacyData.Value, accountingData.Value);

        var variationTotalizer = financialAssets.Details.Data["variacao"]["2026-01"].First();
        var variationClassification = Assert.Single(variationTotalizer.Classifications);
        Assert.Empty(variationClassification.Datas);

        var expectedDetails = new Dictionary<string, string[]>
        {
            ["FINANCIAL_ASSETS"] = new[] { "Caixa e Equivalente de Caixa", "Aplicação Financeira" },
            ["OPERATING_ASSETS"] = new[] { "Clientes", "Estoques", "Outros Ativos Operacionais Total" },
            ["NON_CURRENT_ASSETS"] = new[] { "Ativo Não Circulante Operacional" },
            ["FIXED_ASSETS"] = new[] { "Investimentos", "Imobilizado", "Depreciação / Amort. Acumulada", "Intangível" },
            ["FINANCIAL_LIABILITIES"] = new[] { "Empréstimos e Financiamentos" },
            ["OPERATING_LIABILITIES"] = new[] { "Fornecedores", "Obrigações Tributárias e Trabalhistas", "Outros Passivos Operacionais Total" },
            ["NON_CURRENT_LIABILITIES"] = new[] { "Passivo Não Circulante Financeiro" },
            ["EQUITY"] = new[] { "Capital Social", "Reservas", "Lucros / Prejuízos Acumulados", "Distribuição de Lucro", "Resultado Acumulado" }
        };

        foreach (var expected in expectedDetails)
        {
            var detailTotalizers = Row(response, expected.Key)
                .Details.Data!["realizado"]["2026-01"];
            Assert.Equal(expected.Value, detailTotalizers.Select(item => item.Name));
            Assert.All(detailTotalizers, item => Assert.True(item.Expandable));
        }

        AssertNestedCalculatedTotalizer(
            response,
            fixture.Assets,
            "OPERATING_ASSETS",
            "Outros Ativos Operacionais Total",
            "Outros Ativos Operacionais",
            "Contas Transitórias Ativo");
        AssertNestedCalculatedTotalizer(
            response,
            fixture.Liabilities,
            "OPERATING_LIABILITIES",
            "Outros Passivos Operacionais Total",
            "Outros Passivos Operacionais",
            "Contas Transitórias Passivo");
        Assert.Equal(4, Row(response, "OPERATING_ASSETS").Details.Counts["realizado"]["2026-01"]);
        Assert.Equal(4, Row(response, "OPERATING_LIABILITIES").Details.Counts["realizado"]["2026-01"]);

        Assert.Null(Row(response, "TOTAL_ASSETS").Details.Data);
        Assert.Null(Row(response, "TOTAL_LIABILITIES").Details.Data);
        Assert.Null(Row(response, "BALANCE_DIFFERENCE").Details.Data);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("costCenter", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("initialValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("creditValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("debitValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("datas", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Keeps_collections_non_null_when_legacy_results_are_empty()
    {
        var empty = new _2___Application._2_Dto_s.TotalizerClassification.PainelBalancoComparativoResponse();

        var response = ReclassifiedBalanceSheetV2Mapper.Map(
            empty,
            new _2___Application._2_Dto_s.TotalizerClassification.PainelBalancoComparativoResponse(),
            ReclassifiedBalanceSheetV2Fixture.Year);

        Assert.NotNull(response.Data.Periods);
        Assert.NotNull(response.Data.Scenarios);
        Assert.NotNull(response.Data.Statements);
        Assert.NotNull(response.Data.Rows);
        Assert.Empty(response.Data.Periods);
        Assert.Empty(response.Data.Rows);
        Assert.Equal(3, response.Data.Scenarios.Count);
        Assert.Equal(2, response.Data.Statements.Count);
    }

    [Fact]
    public void Documents_the_new_authorized_route_without_changing_legacy_routes()
    {
        var controllerType = typeof(ReclassifiedBalanceSheetV2Controller);
        Assert.NotNull(controllerType.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("api/v2/reclassified-balance-sheet",
            controllerType.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(controllerType.GetMethod(nameof(ReclassifiedBalanceSheetV2Controller.Get))!
            .GetCustomAttribute<HttpGetAttribute>());
        Assert.Equal(
            typeof(ReclassifiedBalanceSheetV2Response),
            controllerType.GetMethod(nameof(ReclassifiedBalanceSheetV2Controller.Get))!
                .GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Single(attribute => attribute.StatusCode == 200)
                .Type);

        var legacyMethod = typeof(ClassificationController)
            .GetMethod(nameof(ClassificationController.GetPainelBalancoReclassificadoComparativoAsync))!;
        Assert.Equal("/painel-reclassificado/comparativo",
            legacyMethod.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Contains(legacyMethod.GetParameters(), parameter => parameter.Name == "typeClassification");
        Assert.NotNull(legacyMethod.GetCustomAttribute<AuthorizeAttribute>());

        var legacyProjectionMethod = typeof(ClassificationService)
            .GetMethod(nameof(ClassificationService.GetReclassifiedBalanceSheetComparativeLegacyResultAsync));
        Assert.NotNull(legacyProjectionMethod);
    }

    private static BalanceSheetRowDto Row(
        ReclassifiedBalanceSheetV2Response response,
        string code) =>
        Assert.Single(response.Data.Rows.Where(row => row.Code == code));

    private static void AssertNestedCalculatedTotalizer(
        ReclassifiedBalanceSheetV2Response response,
        _2___Application._2_Dto_s.TotalizerClassification.PainelBalancoComparativoResponse legacy,
        string rowCode,
        string totalizerName,
        params string[] expectedClassificationNames)
    {
        var nestedTotalizer = Row(response, rowCode)
            .Details.Data!["realizado"]["2026-01"]
            .Single(totalizer => totalizer.Name == totalizerName);
        var legacyMonth = legacy.Realizado.Months.Single(month => month.DateMonth == 1);

        Assert.True(nestedTotalizer.Expandable);
        Assert.Equal(expectedClassificationNames,
            nestedTotalizer.Classifications.Select(classification => classification.Name));

        foreach (var classification in nestedTotalizer.Classifications)
        {
            var legacySource = legacyMonth.Totalizer
                .Single(totalizer => totalizer.Name == classification.Name);
            var expectedDatas = legacySource.Classifications
                .SelectMany(item => item.Datas)
                .ToArray();

            Assert.Equal(legacySource.Id, classification.Id);
            Assert.Equal(legacySource.TypeOrder, classification.TypeOrder);
            Assert.Equal(legacySource.TotalValue, classification.Value);
            Assert.Equal(expectedDatas.Select(data => data.Id),
                classification.Datas.Select(data => data.Id));
            Assert.Equal(expectedDatas.Select(data => data.Name),
                classification.Datas.Select(data => data.Name));
            Assert.Equal(expectedDatas.Select(data => data.Value),
                classification.Datas.Select(data => data.Value));
        }
    }
}
