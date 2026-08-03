using System.Reflection;
using System.Text.Json;
using _2___Application._1_Services.DRE.V2;
using _2___Application._2_Dto_s.TotalizerClassification;
using ConsultarMRP.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ConsultarMRP.Tests.DRE.V2;

public sealed class DreV2MapperTests
{
    [Fact]
    public void Maps_all_legacy_cells_with_exact_decimal_parity()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);

        foreach (var scenario in response.Data.Scenarios)
        foreach (var period in response.Data.Periods)
        foreach (var row in response.Data.Rows)
        {
            var actual = row.Values[scenario.Key][period.Key];
            var expected = fixture.Expected(scenario.Key, period.Key, row.Code);

            Assert.True(
                actual == expected,
                $"DRE V2 parity failure. Scenario: {scenario.Key}; Period: {period.Key}; " +
                $"Row: {row.Name}; Code: {row.Code}; Legacy value: {expected}; V2 value: {actual}.");
        }
    }

    [Fact]
    public void Copies_accumulated_variation_positive_negative_and_zero_without_recalculation()
    {
        var fixture = new DreV2Fixture();
        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);

        var grossRevenue = Row(response, "GROSS_REVENUE");
        Assert.Equal(fixture.Expected("realizado", "accumulated", "GROSS_REVENUE"),
            grossRevenue.Values["realizado"]["accumulated"]);
        Assert.NotEqual(
            grossRevenue.Values["realizado"]["2026-01"] + grossRevenue.Values["realizado"]["2026-02"],
            grossRevenue.Values["realizado"]["accumulated"]);

        Assert.Equal(fixture.Expected("variacao", "2026-01", "GROSS_REVENUE"),
            grossRevenue.Values["variacao"]["2026-01"]);
        Assert.True(Row(response, "DEPRECIATION_EXPENSE").Values["realizado"]["2026-01"] < 0);
        Assert.True(Row(response, "EBITDA_DEPRECIATION_ADDBACK").Values["realizado"]["2026-01"] > 0);
        Assert.Equal(0m, Row(response, "TAXES_AND_CONTRIBUTIONS").Values["realizado"]["2026-02"]);
    }

    [Fact]
    public void Keeps_unique_stable_codes_hierarchy_and_explicit_order_with_duplicate_legacy_orders()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var codes = response.Data.Rows.Select(row => row.Code).ToArray();

        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(response.Data.Rows.OrderBy(row => row.DisplayOrder).Select(row => row.Code), codes);
        Assert.Equal("OPERATING_EXPENSES", Row(response, "SALES_EXPENSES").ParentCode);
        Assert.Equal(1, Row(response, "SALES_EXPENSES").Level);
        Assert.Null(Row(response, "COST_OF_GOODS").ParentCode);
        Assert.Equal("percentage", Row(response, "EBITDA_MARGIN_PERCENT").ValueType);
        Assert.All(response.Data.Rows, row =>
            Assert.Contains(row.RowType, new[] { "section", "classification", "subtotal", "percentage", "adjustment" }));
    }

    [Fact]
    public void Renames_cost_of_services_only_in_the_v2_contract()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var definition = DreRowCatalog.All.Single(row => row.Code == "COST_OF_SERVICES");

        Assert.Equal("Custos Operacionais", Row(response, "COST_OF_SERVICES").Name);
        Assert.Equal("(-) Custos dos Serviços Prestados", definition.LegacySourceName);
        Assert.Equal("(=) Receita Líquida de Vendas", definition.LegacyParentTotalizerName);
        Assert.Equal(
            fixture.Expected("realizado", "2026-01", "COST_OF_SERVICES"),
            Row(response, "COST_OF_SERVICES").Values["realizado"]["2026-01"]);
    }

    [Fact]
    public void Orders_other_results_and_its_children_immediately_before_ebit()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var rows = response.Data.Rows.ToArray();

        Assert.Equal(rows.Length, rows.Select(row => row.DisplayOrder).Distinct().Count());
        Assert.True(rows.Zip(rows.Skip(1), (current, next) =>
            current.DisplayOrder < next.DisplayOrder).All(isIncreasing => isIncreasing));

        var expectedOrders = new Dictionary<string, int>
        {
            ["OPERATING_PROFIT"] = 250,
            ["OPERATING_MARGIN_PERCENT"] = 260,
            ["OTHER_RESULTS"] = 270,
            ["OTHER_INCOME"] = 280,
            ["OTHER_EXPENSES"] = 290,
            ["EBIT"] = 300,
            ["EBIT_MARGIN_PERCENT"] = 310,
            ["CAPITAL_GAINS_AND_LOSSES"] = 320,
            ["OTHER_NON_OPERATING_INCOME"] = 330,
            ["FINANCIAL_RESULT"] = 340,
            ["FINANCIAL_INCOME"] = 350,
            ["FINANCIAL_EXPENSES"] = 360
        };

        foreach (var expected in expectedOrders)
            Assert.Equal(expected.Value, Row(response, expected.Key).DisplayOrder);

        var otherResultsIndex = Array.FindIndex(rows, row => row.Code == "OTHER_RESULTS");
        Assert.Equal("OPERATING_MARGIN_PERCENT", rows[otherResultsIndex - 1].Code);
        Assert.Equal("OTHER_INCOME", rows[otherResultsIndex + 1].Code);
        Assert.Equal("OTHER_EXPENSES", rows[otherResultsIndex + 2].Code);
        Assert.Equal("EBIT", rows[otherResultsIndex + 3].Code);
        Assert.Equal("EBIT_MARGIN_PERCENT", rows[otherResultsIndex + 4].Code);
        Assert.Equal("OTHER_RESULTS", rows[otherResultsIndex + 1].ParentCode);
        Assert.Equal("OTHER_RESULTS", rows[otherResultsIndex + 2].ParentCode);

        var businessSequence = new[]
        {
            "OPERATING_PROFIT",
            "OPERATING_MARGIN_PERCENT",
            "OTHER_RESULTS",
            "EBIT",
            "EBIT_MARGIN_PERCENT"
        };
        Assert.Equal(
            new[]
            {
                "Lucro Operacional",
                "Margem Operacional %",
                "Outros Resultados",
                "Lucro Antes do Resultado Financeiro",
                "Margem LAJIR %"
            },
            rows.Where(row => businessSequence.Contains(row.Code)).Select(row => row.Name));
    }

    [Fact]
    public void Differentiates_depreciation_by_parent_context_even_with_the_same_classification_id()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var expense = Row(response, "DEPRECIATION_EXPENSE");
        var addback = Row(response, "EBITDA_DEPRECIATION_ADDBACK");

        Assert.Equal(expense.Source.ClassificationId, addback.Source.ClassificationId);
        Assert.NotEqual(expense.Source.SourceParentTotalizerId, addback.Source.SourceParentTotalizerId);
        Assert.NotEqual(
            expense.Values["realizado"]["2026-01"],
            addback.Values["realizado"]["2026-01"]);
    }

    [Fact]
    public void Keeps_depreciation_immediately_after_net_margin_without_parent_relationship()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var rows = response.Data.Rows.ToArray();
        var netMargin = Row(response, "NET_MARGIN_PERCENT");
        var depreciation = Row(response, "EBITDA_DEPRECIATION_ADDBACK");
        var netMarginIndex = Array.FindIndex(rows, row => row.Code == netMargin.Code);

        Assert.False(netMargin.Expandable);
        Assert.False(netMargin.Details.Available);
        Assert.Null(depreciation.ParentCode);
        Assert.Equal(0, depreciation.Level);
        Assert.Equal(netMargin.DisplayOrder + 10, depreciation.DisplayOrder);
        Assert.Equal(depreciation.Code, rows[netMarginIndex + 1].Code);
    }

    [Fact]
    public void Makes_ebt_expandable_with_irpj_and_csll_as_its_immediate_children()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var rows = response.Data.Rows.ToArray();
        var ebt = Row(response, "EBT");
        var irpj = Row(response, "IRPJ_PROVISION");
        var csll = Row(response, "CSLL_PROVISION");
        var ebtIndex = Array.FindIndex(rows, row => row.Code == ebt.Code);

        Assert.True(ebt.Expandable);
        Assert.True(ebt.Details.Available);
        Assert.Equal("EBT", irpj.ParentCode);
        Assert.Equal("EBT", csll.ParentCode);
        Assert.Equal(1, irpj.Level);
        Assert.Equal(1, csll.Level);
        Assert.Equal(irpj.Code, rows[ebtIndex + 1].Code);
        Assert.Equal(csll.Code, rows[ebtIndex + 2].Code);
        Assert.Equal("EBT_MARGIN_PERCENT", rows[ebtIndex + 3].Code);
    }

    [Fact]
    public void Calculates_financial_result_and_expands_income_and_expenses()
    {
        var fixture = new DreV2Fixture();

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var rows = response.Data.Rows.ToArray();
        var financialResult = Row(response, "FINANCIAL_RESULT");
        var financialIncome = Row(response, "FINANCIAL_INCOME");
        var financialExpenses = Row(response, "FINANCIAL_EXPENSES");
        var financialResultIndex = Array.FindIndex(rows, row => row.Code == financialResult.Code);

        Assert.True(financialResult.Expandable);
        Assert.True(financialResult.Details.Available);
        Assert.Equal(DreRowCatalog.Calculated, financialResult.Source.SourceType);
        Assert.Equal("FINANCIAL_RESULT", financialIncome.ParentCode);
        Assert.Equal("FINANCIAL_RESULT", financialExpenses.ParentCode);
        Assert.Equal(financialIncome.Code, rows[financialResultIndex + 1].Code);
        Assert.Equal(financialExpenses.Code, rows[financialResultIndex + 2].Code);

        foreach (var scenario in response.Data.Scenarios)
        foreach (var period in response.Data.Periods)
            Assert.Equal(
                financialIncome.Values[scenario.Key][period.Key] +
                financialExpenses.Values[scenario.Key][period.Key],
                financialResult.Values[scenario.Key][period.Key]);
    }

    [Fact]
    public void Main_contract_includes_classification_children_without_changing_other_row_shapes()
    {
        var fixture = new DreV2Fixture();
        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("initialValue", json, StringComparison.Ordinal);
        Assert.Contains("creditValue", json, StringComparison.Ordinal);
        Assert.Contains("debitValue", json, StringComparison.Ordinal);
        Assert.Contains("costCenter", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"entries\"", json, StringComparison.Ordinal);
        var classificationRow = Row(response, "ADMINISTRATIVE_GENERAL_EXPENSES");
        Assert.Equal(1, classificationRow.Details.Counts["realizado"]["2026-01"]);
        var embeddedEntry = Assert.Single(classificationRow.Details.Data!["realizado"]["2026-01"]);
        Assert.Equal(
            fixture.Expected("realizado", "2026-01", "ADMINISTRATIVE_GENERAL_EXPENSES"),
            embeddedEntry.Value);
        var variationEntry = Assert.Single(
            classificationRow.Details.Data["variacao"]["2026-01"]);
        Assert.Equal(
            fixture.Expected("variacao", "2026-01", "ADMINISTRATIVE_GENERAL_EXPENSES"),
            variationEntry.Value);
        Assert.Empty(classificationRow.Details.Data["realizado"]["accumulated"]);

        Assert.Null(Row(response, "GROSS_REVENUE").Details.Data);
        Assert.Null(Row(response, "EBITDA").Details.Data);

        var details = DreV2Mapper.MapDetails(
            fixture.Legacy,
            DreV2Fixture.Year,
            "ADMINISTRATIVE_GENERAL_EXPENSES",
            "realizado",
            "2026-01");

        var entry = Assert.Single(details.Data.Entries);
        Assert.Equal("ADMINISTRATIVE_GENERAL_EXPENSES", details.Data.RowCode);
        Assert.NotEmpty(entry.CostCenter);
        Assert.Equal(fixture.Expected("realizado", "2026-01", "ADMINISTRATIVE_GENERAL_EXPENSES"),
            entry.Value);
        Assert.Equal(embeddedEntry.Id, entry.Id);
        Assert.Equal(embeddedEntry.Value, entry.Value);
    }

    [Fact]
    public void Produces_deterministic_periods_scenarios_and_non_null_empty_collections()
    {
        var fixture = new DreV2Fixture();
        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);

        Assert.Equal(new[] { "2026-01", "2026-02", "accumulated" },
            response.Data.Periods.Select(period => period.Key));
        Assert.Equal(new[] { "Janeiro", "Fevereiro", "Acumulado" },
            response.Data.Periods.Select(period => period.Label));
        Assert.Equal(new[] { "realizado", "orcado", "variacao" },
            response.Data.Scenarios.Select(scenario => scenario.Key));

        var empty = DreV2Mapper.Map(new PainelBalancoComparativoResponse
        {
            Realizado = new PainelBalancoContabilRespone { Months = new() }
        }, DreV2Fixture.Year);

        Assert.NotNull(empty.Data.Periods);
        Assert.NotNull(empty.Data.Scenarios);
        Assert.NotNull(empty.Data.Rows);
        Assert.Empty(empty.Data.Periods);
        Assert.Empty(empty.Data.Rows);
    }

    [Fact]
    public void Omits_scenarios_that_have_no_legacy_values_instead_of_creating_zeros()
    {
        var fixture = new DreV2Fixture();
        fixture.Legacy.Orcado = new PainelBalancoContabilRespone { Months = new() };
        fixture.Legacy.Variacao = new PainelBalancoContabilRespone { Months = new() };

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);

        Assert.Equal(new[] { "realizado" }, response.Data.Scenarios.Select(item => item.Key));
        Assert.All(response.Data.Rows, row =>
        {
            Assert.True(row.Values.ContainsKey("realizado"));
            Assert.False(row.Values.ContainsKey("orcado"));
            Assert.False(row.Values.ContainsKey("variacao"));
        });
    }

    [Fact]
    public void Omits_optional_catalog_extensions_when_the_legacy_plan_does_not_have_them()
    {
        var fixture = new DreV2Fixture();

        foreach (var panel in new[]
                 {
                     fixture.Legacy.Realizado,
                     fixture.Legacy.Orcado,
                     fixture.Legacy.Variacao
                 })
        foreach (var month in panel.Months)
        {
            var netRevenue = month.Totalizer.Single(item =>
                item.Name == "(=) Receita Líquida de Vendas");
            netRevenue.Classifications.RemoveAll(item => item.Name == "(-) Custos Variáveis");
            month.Totalizer.RemoveAll(item => item.Name == "Outros Resultados");
        }

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);

        Assert.DoesNotContain(response.Data.Rows, row => row.Code == "VARIABLE_COSTS");
        Assert.DoesNotContain(response.Data.Rows, row => row.Code == "OTHER_RESULTS");
        Assert.DoesNotContain(response.Data.Rows, row => row.Code == "OTHER_INCOME");
        Assert.DoesNotContain(response.Data.Rows, row => row.Code == "OTHER_EXPENSES");
    }

    [Fact]
    public void Uses_the_first_canonical_name_match_like_v1_when_totalizers_and_classifications_are_duplicated()
    {
        var fixture = new DreV2Fixture();
        var expectedTotalizerId = fixture.Legacy.Realizado.Months[0].Totalizer
            .First(item => item.Name == "Receita Operacional Bruta")
            .Id;

        foreach (var panel in new[]
                 {
                     fixture.Legacy.Realizado,
                     fixture.Legacy.Orcado,
                     fixture.Legacy.Variacao
                 })
        foreach (var month in panel.Months)
        {
            var original = month.Totalizer.First(item => item.Name == "Receita Operacional Bruta");
            var originalProductSales = original.Classifications
                .First(item => item.Name == "Vendas de Produtos");

            original.Classifications.Remove(originalProductSales);
            var duplicateProductSales = new ClassificationRespone
            {
                Id = originalProductSales.Id + 900_000,
                Name = originalProductSales.Name,
                TypeOrder = originalProductSales.TypeOrder,
                Value = 999_999m,
                Datas = new()
            };

            month.Totalizer.Add(new TotalizerParentRespone
            {
                Id = original.Id + 900_000,
                Name = original.Name,
                TypeOrder = original.TypeOrder,
                TotalValue = 999_999m,
                Classifications = new() { originalProductSales, duplicateProductSales }
            });
        }

        var response = DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year);
        var grossRevenue = Row(response, "GROSS_REVENUE");
        var productSales = Row(response, "PRODUCT_SALES");

        Assert.Equal(expectedTotalizerId, grossRevenue.Source.TotalizerId);
        Assert.Equal(
            fixture.Expected("realizado", "2026-01", "GROSS_REVENUE"),
            grossRevenue.Values["realizado"]["2026-01"]);
        Assert.Equal(
            fixture.Expected("realizado", "2026-01", "PRODUCT_SALES"),
            productSales.Values["realizado"]["2026-01"]);
        Assert.NotEqual(grossRevenue.Source.TotalizerId, productSales.Source.SourceParentTotalizerId);
    }

    [Fact]
    public void Fails_explicitly_when_a_catalog_source_is_missing()
    {
        var fixture = new DreV2Fixture();
        fixture.Legacy.Realizado.Months[0].Totalizer.RemoveAll(
            totalizer => totalizer.Name == "EBITDA");

        var exception = Assert.Throws<DreV2MappingException>(
            () => DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year));

        Assert.Contains("EBITDA", exception.Message, StringComparison.Ordinal);
        Assert.Contains("sourceType", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Fails_explicitly_when_a_bound_source_disappears_from_another_period()
    {
        var fixture = new DreV2Fixture();
        fixture.Legacy.Orcado.Months[1].Totalizer.RemoveAll(
            totalizer => totalizer.Name == "EBITDA");

        var exception = Assert.Throws<DreV2MappingException>(
            () => DreV2Mapper.Map(fixture.Legacy, DreV2Fixture.Year));

        Assert.Contains("Scenario: orcado", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Period: 2026-02", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Code: EBITDA", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Preserves_legacy_contract_and_documents_versioned_authorized_routes()
    {
        Assert.Equal(
            new[] { "HasPendingClassifications", "Months", "PendingClassificationsCount", "Totalizador" },
            typeof(PainelBalancoContabilRespone)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .OrderBy(name => name));

        var controllerType = typeof(DreV2Controller);
        Assert.NotNull(controllerType.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("api/v2/dre",
            controllerType.GetCustomAttribute<RouteAttribute>()?.Template);

        var detailsRoute = controllerType.GetMethod(nameof(DreV2Controller.GetDetails))!
            .GetCustomAttribute<HttpGetAttribute>()?.Template;
        Assert.Equal("rows/{rowCode}/details", detailsRoute);

        var legacyMethod = typeof(ClassificationController)
            .GetMethod(nameof(ClassificationController.GetPainelBalancoReclassificadoComparativoAsync))!;
        Assert.NotNull(legacyMethod.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("/painel-reclassificado/comparativo",
            legacyMethod.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    private static _2___Application._2_Dto_s.DRE.V2.DreRowDto Row(
        _2___Application._2_Dto_s.DRE.V2.DreV2Response response,
        string code) =>
        Assert.Single(response.Data.Rows.Where(row => row.Code == code));
}
