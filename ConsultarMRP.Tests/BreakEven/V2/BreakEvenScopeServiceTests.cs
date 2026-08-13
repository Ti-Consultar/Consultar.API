using _2___Application._1_Services.BreakEven.V2;
using _2___Application._1_Services.Scope;
using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConsultarMRP.Tests.BreakEven.V2;

public sealed class BreakEvenScopeServiceTests
{
    [Fact]
    public async Task Group_access_authorizes_a_company_scope_resolved_from_explicit_ids()
    {
        await using var context = CreateContext();
        context.CompanyUsers.Add(Access(userId: 7, groupId: 10, companyId: null, subCompanyId: null));
        await context.SaveChangesAsync();
        var service = new BreakEvenScopeService(new StubScopeResolver(123), context);

        var accountPlanId = await service.ResolveAuthorizedAccountPlanIdAsync(
            Scope(groupId: 10, companyId: 20, subCompanyId: null),
            7,
            CancellationToken.None);

        Assert.Equal(123, accountPlanId);
    }

    [Fact]
    public async Task Subcompany_access_does_not_authorize_a_sibling_scope()
    {
        await using var context = CreateContext();
        context.CompanyUsers.Add(Access(userId: 7, groupId: 10, companyId: 20, subCompanyId: 30));
        await context.SaveChangesAsync();
        var service = new BreakEvenScopeService(new StubScopeResolver(123), context);

        await Assert.ThrowsAsync<BreakEvenAccessDeniedException>(() =>
            service.ResolveAuthorizedAccountPlanIdAsync(
                Scope(groupId: 10, companyId: 20, subCompanyId: 31),
                7,
                CancellationToken.None));
    }

    private static CoreServiceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CoreServiceDbContext>()
            .UseInMemoryDatabase($"break-even-scope-{Guid.NewGuid():N}")
            .Options;
        return new CoreServiceDbContext(options);
    }

    private static EntityScopeRequest Scope(int groupId, int? companyId, int? subCompanyId) =>
        new()
        {
            GroupId = groupId,
            CompanyId = companyId,
            SubCompanyId = subCompanyId,
            IncludeChildren = false
        };

    private static CompanyUserModel Access(
        int userId,
        int groupId,
        int? companyId,
        int? subCompanyId) =>
        new()
        {
            UserId = userId,
            GroupId = groupId,
            CompanyId = companyId,
            SubCompanyId = subCompanyId,
            PermissionId = 1,
            User = null!,
            Group = null!,
            Permission = null!
        };

    private sealed class StubScopeResolver : IAccountPlanScopeResolver
    {
        private readonly int? _accountPlanId;

        public StubScopeResolver(int? accountPlanId)
        {
            _accountPlanId = accountPlanId;
        }

        public Task<List<int>> ResolveAccountPlanIds(EntityScopeRequest scope) =>
            Task.FromResult(_accountPlanId.HasValue
                ? new List<int> { _accountPlanId.Value }
                : new List<int>());

        public Task<int?> ResolveOwnAccountPlanId(EntityScopeRequest scope) =>
            Task.FromResult(_accountPlanId);
    }
}
