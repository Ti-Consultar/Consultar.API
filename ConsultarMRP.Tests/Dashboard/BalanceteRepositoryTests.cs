using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Context;
using _4_InfraData._1_Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConsultarMRP.Tests.Dashboard;

public class BalanceteRepositoryTests
{
    [Fact]
    public async Task GetLatestWithDataByAccountPlanId_ReturnsLatestPopulatedPeriodForRequestedPlan()
    {
        await using var context = CreateContext();
        var december = CreateBalancete(1, 2025, EMonth.December);
        var january = CreateBalancete(1, 2026, EMonth.January);
        var februaryWithoutData = CreateBalancete(1, 2026, EMonth.February);
        var anotherPlan = CreateBalancete(2, 2027, EMonth.March);

        context.Balancete.AddRange(december, january, februaryWithoutData, anotherPlan);
        context.BalanceteData.AddRange(CreateBalanceteData(december), CreateBalanceteData(january), CreateBalanceteData(anotherPlan));
        await context.SaveChangesAsync();

        var repository = new BalanceteRepository(context);

        var result = await repository.GetLatestWithDataByAccountPlanId(1);

        Assert.NotNull(result);
        Assert.Equal(2026, result.DateYear);
        Assert.Equal(EMonth.January, result.DateMonth);
    }

    [Fact]
    public async Task GetLatestWithDataByAccountPlanId_ReturnsNullWhenPlanHasNoPopulatedBalancetes()
    {
        await using var context = CreateContext();
        context.Balancete.Add(CreateBalancete(1, 2026, EMonth.January));
        await context.SaveChangesAsync();
        var repository = new BalanceteRepository(context);

        var result = await repository.GetLatestWithDataByAccountPlanId(1);

        Assert.Null(result);
    }

    private static CoreServiceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CoreServiceDbContext>()
            .UseInMemoryDatabase($"latest-dashboard-period-{Guid.NewGuid():N}")
            .Options;

        return new CoreServiceDbContext(options);
    }

    private static BalanceteModel CreateBalancete(int accountPlanId, int year, EMonth month)
    {
        return new BalanceteModel
        {
            AccountPlansId = accountPlanId,
            DateYear = year,
            DateMonth = month,
            Status = ESituationBalancete.Accepted
        };
    }

    private static BalanceteDataModel CreateBalanceteData(BalanceteModel balancete)
    {
        return new BalanceteDataModel
        {
            Balancete = balancete,
            CostCenter = "1",
            Name = "Conta"
        };
    }
}
