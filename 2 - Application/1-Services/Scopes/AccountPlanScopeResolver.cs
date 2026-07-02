using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;

namespace _2___Application._1_Services.Scope;

public class AccountPlanScopeResolver : IAccountPlanScopeResolver
{
    private readonly AccountPlansRepository _accountPlansRepository;
    private readonly GroupRepository _groupRepository;

    public AccountPlanScopeResolver(
        AccountPlansRepository accountPlansRepository,
        GroupRepository groupRepository)
    {
        _accountPlansRepository = accountPlansRepository;
        _groupRepository = groupRepository;
    }

    public async Task<AccountPlansModel?> ResolveCanonicalAccountPlanAsync(int groupId)
    {
        var group = await _groupRepository.GetById(groupId);

        if (group is null)
            return null;

        return await _accountPlansRepository.GetByGroupId(groupId);
    }

    public Task<AccountPlansModel?> ResolveCanonicalAccountPlanByScopeAsync(
        int groupId,
        int? companyId,
        int? subCompanyId)
    {
        return ResolveCanonicalAccountPlanAsync(groupId);
    }

    // Legado: mantido para relatórios que ainda operam por escopo hierárquico.
    public async Task<int?> ResolveOwnAccountPlanId(EntityScopeRequest scope)
    {
        var plan = await _accountPlansRepository.GetExactScope(
            scope.GroupId,
            scope.CompanyId,
            scope.SubCompanyId
        );

        return plan?.Id;
    }

    // Legado: mantido para relatórios que ainda agregam planos de empresa/filial.
    public async Task<List<int>> ResolveAccountPlanIds(EntityScopeRequest scope)
    {
        if (!scope.IncludeChildren || scope.SubCompanyId.HasValue)
        {
            var own = await ResolveOwnAccountPlanId(scope);
            return own.HasValue ? new List<int> { own.Value } : new List<int>();
        }

        if (!scope.CompanyId.HasValue)
        {
            var all = await _accountPlansRepository.GetAllByGroup(scope.GroupId);
            return all.Select(x => x.Id).ToList();
        }

        var companyPlans = await _accountPlansRepository.GetCompanyAccountPlans(
            scope.GroupId,
            new List<int> { scope.CompanyId.Value }
        );

        return companyPlans.Select(x => x.Id).ToList();
    }
}
