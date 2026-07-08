using _4_InfraData._1_Repositories;

namespace _2___Application._1_Services.Scope
{
    public class FinancialReportScope
    {
        public int AccountPlanId { get; set; }
        public int GroupId { get; set; }
        public int? CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
    }

    public static class FinancialReportScopeExtensions
    {
        public static async Task<FinancialReportScope?> ResolveFinancialReportScopeAsync(
            this IAccountPlanScopeResolver accountPlanScopeResolver,
            AccountPlansRepository accountPlansRepository,
            int? accountPlanId,
            int? groupId = null,
            int? companyId = null,
            int? subCompanyId = null)
        {
            if (groupId.HasValue || companyId.HasValue || subCompanyId.HasValue)
            {
                var scopeGroupId = groupId;
                var scopeCompanyId = companyId;

                if (subCompanyId.HasValue)
                {
                    var scopedPlan = await accountPlansRepository.GetSubCompanyAccountPlan(subCompanyId.Value);
                    if (scopedPlan == null && !scopeGroupId.HasValue)
                        return null;

                    if (scopedPlan != null)
                    {
                        scopeGroupId ??= scopedPlan.GroupId;
                        scopeCompanyId ??= scopedPlan.CompanyId;
                    }
                }
                else if (companyId.HasValue)
                {
                    var scopedPlan = await accountPlansRepository.GetCompanyAccountPlanByCompanyId(companyId.Value);
                    if (scopedPlan == null && !scopeGroupId.HasValue)
                        return null;

                    if (scopedPlan != null)
                        scopeGroupId ??= scopedPlan.GroupId;
                }

                if (!scopeGroupId.HasValue)
                    return null;

                var canonicalAccountPlan = await accountPlanScopeResolver
                    .ResolveCanonicalAccountPlanAsync(scopeGroupId.Value);

                return canonicalAccountPlan == null
                    ? null
                    : new FinancialReportScope
                    {
                        AccountPlanId = canonicalAccountPlan.Id,
                        GroupId = scopeGroupId.Value,
                        CompanyId = scopeCompanyId,
                        SubCompanyId = subCompanyId
                    };
            }

            if (!accountPlanId.HasValue)
                return null;

            var legacyAccountPlan = await accountPlansRepository.GetByIdSingleAsync(accountPlanId.Value);
            if (legacyAccountPlan == null)
                return null;

            var canonicalLegacyAccountPlan = await accountPlanScopeResolver
                .ResolveCanonicalAccountPlanAsync(legacyAccountPlan.GroupId);

            return canonicalLegacyAccountPlan == null
                ? null
                : new FinancialReportScope
                {
                    AccountPlanId = canonicalLegacyAccountPlan.Id,
                    GroupId = legacyAccountPlan.GroupId,
                    CompanyId = legacyAccountPlan.CompanyId,
                    SubCompanyId = legacyAccountPlan.SubCompanyId
                };
        }
    }
}
