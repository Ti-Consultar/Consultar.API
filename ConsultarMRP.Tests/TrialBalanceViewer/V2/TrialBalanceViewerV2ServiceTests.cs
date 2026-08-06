using _2___Application._1_Services.AccountPlans.Balancete.V2;
using _2___Application._2_Dto_s.AccountPlan.Balancete.V2;
using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConsultarMRP.Tests.TrialBalanceViewer.V2;

public sealed class TrialBalanceViewerV2ServiceTests
{
    private const int UserId = 73;
    private const int TrialBalanceId = 20862;

    [Fact]
    public async Task Paginates_with_filtered_total_and_stable_consecutive_blocks()
    {
        await using var context = CreateContext();
        SeedScope(context);

        for (var index = 1; index <= 450; index++)
            AddRow(context, index, $"1.{index}", $"Conta {index}");

        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        var first = await service.GetRowsAsync(
            TrialBalanceId,
            UserId,
            new TrialBalanceViewerRowsRequest { Offset = 0, Limit = 200 },
            CancellationToken.None);
        var second = await service.GetRowsAsync(
            TrialBalanceId,
            UserId,
            new TrialBalanceViewerRowsRequest { Offset = 200, Limit = 200 },
            CancellationToken.None);

        Assert.Equal(450, first.Pagination.Total);
        Assert.Equal(200, first.Pagination.Returned);
        Assert.True(first.Pagination.HasMore);
        Assert.Equal(200, second.Pagination.Returned);
        Assert.Empty(first.Items.Select(item => item.Id).Intersect(second.Items.Select(item => item.Id)));
        Assert.Equal(Enumerable.Range(1, 400), first.Items.Concat(second.Items).Select(item => item.Id));
    }

    [Fact]
    public async Task Searches_account_code_or_description()
    {
        await using var context = CreateContext();
        SeedScope(context);
        AddRow(context, 1, "1.1.01", "CAIXA");
        AddRow(context, 2, "2.4.99", "Clientes especiais");
        AddRow(context, 3, "3.1", "Receita");
        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        var byCode = await service.GetRowsAsync(
            TrialBalanceId, UserId,
            new TrialBalanceViewerRowsRequest { Search = "1.01" },
            CancellationToken.None);
        var byDescription = await service.GetRowsAsync(
            TrialBalanceId, UserId,
            new TrialBalanceViewerRowsRequest { Search = "especiais" },
            CancellationToken.None);

        Assert.Equal(1, byCode.Pagination.Total);
        Assert.Equal("1.1.01", Assert.Single(byCode.Items).AccountCode);
        Assert.Equal(1, byDescription.Pagination.Total);
        Assert.Equal("Clientes especiais", Assert.Single(byDescription.Items).Description);
    }

    [Fact]
    public async Task Filters_multiple_levels_ignoring_duplicates_and_maps_children()
    {
        await using var context = CreateContext();
        SeedScope(context);
        AddRow(context, 1, "1", "Ativo");
        AddRow(context, 2, "1.1", "Circulante");
        AddRow(context, 3, "1.1.01", "Caixa");
        AddRow(context, 4, "1.1.01.001", "Caixa filial");
        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        var response = await service.GetRowsAsync(
            TrialBalanceId, UserId,
            new TrialBalanceViewerRowsRequest { Levels = "1, 3,3" },
            CancellationToken.None);

        Assert.Equal(new[] { 1, 3 }, response.Items.Select(item => item.Level));
        Assert.All(response.Items, item => Assert.True(item.HasChildren));
    }

    [Fact]
    public async Task Filters_only_rows_with_debit_or_credit_movement()
    {
        await using var context = CreateContext();
        SeedScope(context);
        AddRow(context, 1, "1", "Sem movimento");
        AddRow(context, 2, "2", "Débito", debit: 0.01m);
        AddRow(context, 3, "3", "Crédito", credit: -4.25m);
        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        var response = await service.GetRowsAsync(
            TrialBalanceId, UserId,
            new TrialBalanceViewerRowsRequest { OnlyWithMovement = true },
            CancellationToken.None);

        Assert.Equal(2, response.Pagination.Total);
        Assert.Equal(new[] { 2, 3 }, response.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Sorts_only_by_whitelisted_field_with_source_order_tie_breaker()
    {
        await using var context = CreateContext();
        SeedScope(context);
        AddRow(context, 30, "1.10", "Mesmo nome", debit: 2m);
        AddRow(context, 10, "1.2", "Mesmo nome", debit: 2m);
        AddRow(context, 20, "1.3", "Outro", debit: 9m);
        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        var response = await service.GetRowsAsync(
            TrialBalanceId, UserId,
            new TrialBalanceViewerRowsRequest { Sort = "debit", Direction = "asc" },
            CancellationToken.None);

        Assert.Equal(new[] { 10, 30, 20 }, response.Items.Select(item => item.Id));
        Assert.Equal(response.Items.Select(item => item.Id), response.Items.Select(item => item.SourceOrder));

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetRowsAsync(
            TrialBalanceId, UserId,
            new TrialBalanceViewerRowsRequest { Sort = "DROP TABLE" },
            CancellationToken.None));
    }

    [Theory]
    [InlineData(-1, 200, null, "sourceOrder", "asc")]
    [InlineData(0, 0, null, "sourceOrder", "asc")]
    [InlineData(0, 501, null, "sourceOrder", "asc")]
    [InlineData(0, 200, "0", "sourceOrder", "asc")]
    [InlineData(0, 200, "1,", "sourceOrder", "asc")]
    [InlineData(0, 200, "6", "sourceOrder", "asc")]
    [InlineData(0, 200, null, "unknown", "asc")]
    [InlineData(0, 200, null, "sourceOrder", "sideways")]
    public async Task Rejects_invalid_parameters(
        int offset,
        int limit,
        string? levels,
        string sort,
        string direction)
    {
        await using var context = CreateContext();
        var service = new TrialBalanceViewerV2Service(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetRowsAsync(
            TrialBalanceId,
            UserId,
            new TrialBalanceViewerRowsRequest
            {
                Offset = offset,
                Limit = limit,
                Levels = levels,
                Sort = sort,
                Direction = direction
            },
            CancellationToken.None));
    }

    [Fact]
    public async Task Returns_not_found_for_missing_or_inaccessible_trial_balance()
    {
        await using var context = CreateContext();
        SeedScope(context);
        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        await Assert.ThrowsAsync<TrialBalanceViewerNotFoundException>(() => service.GetRowsAsync(
            99999, UserId, new TrialBalanceViewerRowsRequest(), CancellationToken.None));
        await Assert.ThrowsAsync<TrialBalanceViewerNotFoundException>(() => service.GetRowsAsync(
            TrialBalanceId, 999, new TrialBalanceViewerRowsRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task Returns_empty_accessible_trial_balance_and_preserves_decimal_precision()
    {
        await using var context = CreateContext();
        SeedScope(context);
        await context.SaveChangesAsync();
        var service = new TrialBalanceViewerV2Service(context);

        var empty = await service.GetRowsAsync(
            TrialBalanceId, UserId, new TrialBalanceViewerRowsRequest(), CancellationToken.None);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.Pagination.Total);

        AddRow(context, 1, "1.1", "Precisão", previous: 123456789.12m, debit: 0.01m,
            credit: 0.02m, final: 123456789.11m);
        await context.SaveChangesAsync();
        var response = await service.GetRowsAsync(
            TrialBalanceId, UserId, new TrialBalanceViewerRowsRequest(), CancellationToken.None);
        var row = Assert.Single(response.Items);

        Assert.Equal(123456789.12m, row.PreviousBalance);
        Assert.Equal(123456789.11m, row.FinalBalance);
    }

    private static CoreServiceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CoreServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CoreServiceDbContext(options);
    }

    private static void SeedScope(CoreServiceDbContext context)
    {
        var accountPlan = new AccountPlansModel { Id = 910, GroupId = 12 };
        var trialBalance = new BalanceteModel
        {
            Id = TrialBalanceId,
            AccountPlansId = accountPlan.Id,
            AccountPlans = accountPlan
        };

        context.AccountPlans.Add(accountPlan);
        context.Balancete.Add(trialBalance);
        context.CompanyUsers.Add(new CompanyUserModel
        {
            Id = 1,
            UserId = UserId,
            GroupId = accountPlan.GroupId,
            CompanyId = null,
            SubCompanyId = null
        });
    }

    private static void AddRow(
        CoreServiceDbContext context,
        int id,
        string accountCode,
        string description,
        decimal previous = 0m,
        decimal debit = 0m,
        decimal credit = 0m,
        decimal final = 0m)
    {
        context.BalanceteData.Add(new BalanceteDataModel
        {
            Id = id,
            BalanceteId = TrialBalanceId,
            CostCenter = accountCode,
            Name = description,
            InitialValue = previous,
            Debit = debit,
            Credit = credit,
            FinalValue = final
        });
    }
}
