using _3_Domain._1_Entities;

namespace _2___Application._1_Services.Scope
{
    public interface IAccountPlanScopeResolver
    {
        Task<AccountPlansModel?> ResolveCanonicalAccountPlanAsync(int groupId);
        Task<AccountPlansModel?> ResolveCanonicalAccountPlanByScopeAsync(int groupId, int? companyId, int? subCompanyId);
        Task<List<int>> ResolveAccountPlanIds(EntityScopeRequest scope);
        Task<int?> ResolveOwnAccountPlanId(EntityScopeRequest scope);
    }
}
