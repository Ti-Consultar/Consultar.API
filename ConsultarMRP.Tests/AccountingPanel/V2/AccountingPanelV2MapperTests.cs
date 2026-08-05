using System.Reflection;
using System.Text.Json;
using _2___Application._1_Services;
using _2___Application._1_Services.AccountingPanel.V2;
using _2___Application._2_Dto_s.AccountingPanel.V2;
using _2___Application._2_Dto_s.TotalizerClassification;
using ConsultarMRP.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ConsultarMRP.Tests.AccountingPanel.V2;

public sealed class AccountingPanelV2MapperTests
{
    [Fact]
    public void Returns_asset_and_liability_in_the_requested_canonical_hierarchy()
    {
        var fixture = new AccountingPanelV2Fixture();

        var response = AccountingPanelV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            AccountingPanelV2Fixture.Year);
        var rows = response.Data.Rows.ToArray();

        Assert.Equal(new[] { "2026-01", "2026-02" }, response.Data.Periods.Select(period => period.Key));
        Assert.Equal(new[] { "realizado" }, response.Data.Scenarios.Select(scenario => scenario.Key));
        Assert.Equal(new[] { "asset", "liability" }, response.Data.Statements.Select(statement => statement.Key));
        Assert.Equal(new[] { "TOTAL_ASSETS", "TOTAL_LIABILITIES" },
            response.Data.Statements.Select(statement => statement.TotalRowCode));
        Assert.Equal(33, rows.Length);
        Assert.Equal(rows.Length, rows.Select(row => row.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(rows.OrderBy(row => row.DisplayOrder).Select(row => row.Code), rows.Select(row => row.Code));
        Assert.Equal(rows.Length, rows.Select(row => row.DisplayOrder).Distinct().Count());

        var expectedNames = new[]
        {
            "Total Ativo Circulante",
            "Caixas",
            "Bancos",
            "Aplicações Financeiras",
            "Clientes",
            "Adiantamentos",
            "Empréstimos",
            "Realizavel Longo Prazo",
            "Empréstimos a Coligadas e Controlada",
            "Total Ativo Não Circulante",
            "Investimentos",
            "Imobilizado",
            "Intangível",
            "Depreciação / Amortização Acumuladas",
            "TOTAL GERAL DO ATIVO",
            "Total Passivo Circulante",
            "Fornecedores Diversos",
            "Outras Contas a Pagar",
            "Creditos de Clientes",
            "Empréstimos e Financiamentos",
            "Obrigações Sociais a Pagar",
            "Obrigações Fiscais a Pagar",
            "Total Passivo Não Circulante",
            "Empréstimos e Financiamentos a Longo Prazo",
            "Impostos Parcelados",
            "Patrimônio Liquido",
            "Capital Social",
            "Reservas",
            "Lucros / Prejuízos Acumulados",
            "Distribuição de Lucro",
            "Resultado do Exercício Acumulado",
            "TOTAL GERAL DO PASSIVO",
            "Diferença do Balanço"
        };
        Assert.Equal(expectedNames, rows.Select(row => row.Name));

        AssertChildren(rows, "TOTAL_CURRENT_ASSETS",
            "CASH", "BANKS", "FINANCIAL_INVESTMENTS", "CUSTOMERS", "ADVANCES", "LOANS_RECEIVABLE");
        AssertChildren(rows, "LONG_TERM_RECEIVABLES", "RELATED_PARTY_LOANS_RECEIVABLE");
        AssertChildren(rows, "TOTAL_NON_CURRENT_ASSETS",
            "INVESTMENTS", "FIXED_ASSETS", "INTANGIBLE_ASSETS", "ACCUMULATED_DEPRECIATION");
        AssertChildren(rows, "TOTAL_CURRENT_LIABILITIES",
            "MISCELLANEOUS_SUPPLIERS", "OTHER_ACCOUNTS_PAYABLE", "CUSTOMER_CREDITS",
            "LOANS_AND_FINANCING", "SOCIAL_OBLIGATIONS_PAYABLE", "TAX_OBLIGATIONS_PAYABLE");
        AssertChildren(rows, "TOTAL_NON_CURRENT_LIABILITIES",
            "LONG_TERM_LOANS_AND_FINANCING", "INSTALLMENT_TAXES");
        AssertChildren(rows, "EQUITY",
            "SHARE_CAPITAL", "RESERVES", "RETAINED_EARNINGS", "PROFIT_DISTRIBUTION",
            "ACCUMULATED_PERIOD_RESULT");

        Assert.All(rows.Where(row => row.RowType is "section" or "classification"),
            row => Assert.True(row.Expandable));
        Assert.All(rows.Where(row => row.RowType is "total" or "validation"), row =>
        {
            Assert.False(row.Expandable);
            Assert.False(row.Details.Available);
            Assert.Empty(row.Details.Counts);
        });

        var difference = rows[^1];
        Assert.Equal("BALANCE_DIFFERENCE", difference.Code);
        Assert.Equal("liability", difference.StatementKey);
        Assert.Equal("validation", difference.RowType);
        Assert.Equal(330, difference.DisplayOrder);
        Assert.Equal("calculated", difference.Source.SourceType);
        Assert.Null(difference.ParentCode);
    }

    [Fact]
    public void Copies_every_legacy_value_and_embeds_accounting_entries_without_recalculation()
    {
        var fixture = new AccountingPanelV2Fixture();
        var response = AccountingPanelV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            AccountingPanelV2Fixture.Year);

        foreach (var row in response.Data.Rows
                     .Where(row => row.Source.SourceType != AccountingPanelRowCatalog.Calculated))
        foreach (var period in response.Data.Periods)
        {
            var month = period.Month!.Value;
            var legacyMonth = fixture.Statement(row.StatementKey).Months
                .FirstOrDefault(item => item.DateMonth == month);

            if (legacyMonth is null)
            {
                Assert.False(row.Values["realizado"].ContainsKey(period.Key));
                continue;
            }

            Assert.Equal(
                fixture.Expected(row.StatementKey, month, row.Code),
                row.Values["realizado"][period.Key]);
        }

        var cash = Row(response, "CASH");
        var legacyClassification = fixture.Assets.Months.Single(month => month.DateMonth == 1)
            .Totalizer.Single(totalizer => totalizer.Name == "Total Ativo Circulante")
            .Classifications.Single(classification => classification.Name == "Caixas");
        var legacyEntry = Assert.Single(legacyClassification.Datas);
        var entry = Assert.Single(cash.Details.Data!["realizado"]["2026-01"]);

        Assert.Equal(1, cash.Details.Counts["realizado"]["2026-01"]);
        Assert.Equal(legacyEntry.Id, entry.Id);
        Assert.Equal(legacyEntry.TypeOrder, entry.TypeOrder);
        Assert.Equal(legacyEntry.Name, entry.Name);
        Assert.Equal(legacyEntry.CostCenter, entry.CostCenter);
        Assert.Equal(legacyEntry.InitialValue, entry.InitialValue);
        Assert.Equal(legacyEntry.CreditValue, entry.CreditValue);
        Assert.Equal(legacyEntry.DebitValue, entry.DebitValue);
        Assert.Equal(legacyEntry.Value, entry.Value);

        Assert.Null(Row(response, "TOTAL_CURRENT_ASSETS").Details.Data);
        Assert.Equal(6, Row(response, "TOTAL_CURRENT_ASSETS").Details.Counts["realizado"]["2026-01"]);
        Assert.Null(Row(response, "TOTAL_ASSETS").Details.Data);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("costCenter", json, StringComparison.Ordinal);
        Assert.Contains("initialValue", json, StringComparison.Ordinal);
        Assert.Contains("creditValue", json, StringComparison.Ordinal);
        Assert.Contains("debitValue", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Calculates_balance_difference_from_general_totals_for_every_available_period()
    {
        var fixture = new AccountingPanelV2Fixture();
        var response = AccountingPanelV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            AccountingPanelV2Fixture.Year);
        var totalAssets = Row(response, "TOTAL_ASSETS");
        var totalLiabilities = Row(response, "TOTAL_LIABILITIES");
        var difference = Row(response, "BALANCE_DIFFERENCE");

        foreach (var period in response.Data.Periods)
        {
            var hasAssets = totalAssets.Values["realizado"].TryGetValue(period.Key, out var assetValue);
            var hasLiabilities = totalLiabilities.Values["realizado"].TryGetValue(period.Key, out var liabilityValue);

            if (!hasAssets || !hasLiabilities)
            {
                Assert.False(difference.Values["realizado"].ContainsKey(period.Key));
                continue;
            }

            Assert.Equal(
                assetValue - liabilityValue,
                difference.Values["realizado"][period.Key]);
        }

        Assert.False(difference.Expandable);
        Assert.False(difference.Details.Available);
        Assert.Empty(difference.Details.Counts);
        Assert.Null(difference.Details.Data);
    }

    [Fact]
    public void Consolidates_duplicate_canonical_sources_instead_of_throwing_duplicate_key_errors()
    {
        var fixture = new AccountingPanelV2Fixture();
        var january = fixture.Assets.Months.Single(month => month.DateMonth == 1);
        january.Totalizer.Add(new TotalizerParentRespone
        {
            Id = 99_001,
            Name = "Total Ativo Circulante",
            TotalValue = 25m,
            Classifications = new List<ClassificationRespone>
            {
                new()
                {
                    Id = 99_002,
                    Name = "Caixas",
                    Value = 25m,
                    Datas = new List<BalanceteDataResponse>
                    {
                        new() { Id = 99_003, Name = "Conta duplicada", Value = 25m }
                    }
                }
            }
        });

        var response = AccountingPanelV2Mapper.Map(
            fixture.Assets,
            fixture.Liabilities,
            AccountingPanelV2Fixture.Year);

        Assert.Equal(
            fixture.Expected("asset", 1, "TOTAL_CURRENT_ASSETS") + 25m,
            Row(response, "TOTAL_CURRENT_ASSETS").Values["realizado"]["2026-01"]);
        Assert.Equal(
            fixture.Expected("asset", 1, "CASH") + 25m,
            Row(response, "CASH").Values["realizado"]["2026-01"]);
        Assert.Equal(2, Row(response, "CASH").Details.Counts["realizado"]["2026-01"]);
        Assert.Null(Row(response, "CASH").Source.ClassificationId);
        Assert.Null(Row(response, "CASH").Source.SourceParentTotalizerId);
    }

    [Fact]
    public void Documents_the_authorized_v2_route_without_changing_the_legacy_route()
    {
        var controllerType = typeof(AccountingPanelV2Controller);
        Assert.NotNull(controllerType.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("v2/painel", controllerType.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(controllerType.GetMethod(nameof(AccountingPanelV2Controller.Get))!
            .GetCustomAttribute<HttpGetAttribute>());
        Assert.Equal(
            typeof(AccountingPanelV2Response),
            controllerType.GetMethod(nameof(AccountingPanelV2Controller.Get))!
                .GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Single(attribute => attribute.StatusCode == 200)
                .Type);

        var legacyMethod = typeof(ClassificationController)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(ClassificationController.GetPainelBalancoAsync) &&
                method.GetParameters().Any(parameter => parameter.Name == "year"));
        Assert.Equal("/painel", legacyMethod.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Contains(legacyMethod.GetParameters(), parameter => parameter.Name == "typeClassification");

        Assert.NotNull(typeof(ClassificationService).GetMethod(
            nameof(ClassificationService.GetAccountingPanelLegacyResultAsync)));
    }

    [Fact]
    public void Keeps_collections_non_null_when_both_legacy_panels_are_empty()
    {
        var response = AccountingPanelV2Mapper.Map(
            new PainelBalancoContabilRespone(),
            new PainelBalancoContabilRespone(),
            AccountingPanelV2Fixture.Year);

        Assert.Empty(response.Data.Periods);
        Assert.Empty(response.Data.Rows);
        Assert.Single(response.Data.Scenarios);
        Assert.Equal(2, response.Data.Statements.Count);
    }

    private static void AssertChildren(
        IReadOnlyList<AccountingPanelRowDto> rows,
        string parentCode,
        params string[] expectedChildCodes)
    {
        var parentIndex = rows.ToList().FindIndex(row => row.Code == parentCode);
        var children = rows.Where(row => row.ParentCode == parentCode).ToArray();

        Assert.Equal(expectedChildCodes, children.Select(child => child.Code));
        Assert.Equal(expectedChildCodes,
            rows.Skip(parentIndex + 1).Take(expectedChildCodes.Length).Select(row => row.Code));
        Assert.All(children, child => Assert.Equal(1, child.Level));
    }

    private static AccountingPanelRowDto Row(AccountingPanelV2Response response, string code) =>
        Assert.Single(response.Data.Rows.Where(row => row.Code == code));
}
