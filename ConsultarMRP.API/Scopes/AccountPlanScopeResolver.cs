using _4_InfraData._1_Repositories;
using ConsultarMRP.API.Scopes;

public class AccountPlanScopeResolver : IAccountPlanScopeResolver
{
    private readonly AccountPlansRepository _accountPlansRepository;

    public AccountPlanScopeResolver(AccountPlansRepository accountPlansRepository)
    {
        _accountPlansRepository = accountPlansRepository;
    }

    // 🔹 Escopo exato
    public async Task<int?> ResolveOwnAccountPlanId(EntityScopeRequest scope)
    {
        var plan = await _accountPlansRepository.GetExactScope(
            scope.GroupId,
            scope.CompanyId,
            scope.SubCompanyId
        );

        return plan?.Id;
    }

    // 🔹 Escopo agregado
    public async Task<List<int>> ResolveAccountPlanIds(EntityScopeRequest scope)
    {
        // Caso simples
        if (!scope.IncludeChildren || scope.SubCompanyId.HasValue)
        {
            var own = await ResolveOwnAccountPlanId(scope);
            return own.HasValue ? new List<int> { own.Value } : new List<int>();
        }

        // 🔽 Grupo completo
        if (!scope.CompanyId.HasValue)
        {
            var all = await _accountPlansRepository.GetAllByGroup(scope.GroupId);
            return all.Select(x => x.Id).ToList();
        }

        // 🔽 Empresa + subempresas
        var companyPlans = await _accountPlansRepository.GetCompanyAccountPlans(
            scope.GroupId,
            new List<int> { scope.CompanyId.Value }
        );

        return companyPlans.Select(x => x.Id).ToList();
    }
}