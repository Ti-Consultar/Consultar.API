namespace ConsultarMRP.API.Scopes;

public interface IAccountPlanScopeResolver
{
    Task<List<int>> ResolveAccountPlanIds(EntityScopeRequest scope);
    Task<int?> ResolveOwnAccountPlanId(EntityScopeRequest scope);
}