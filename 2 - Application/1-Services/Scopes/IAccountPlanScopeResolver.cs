namespace _2___Application._1_Services.Scope
{
    public interface IAccountPlanScopeResolver
    {
        Task<List<int>> ResolveAccountPlanIds(EntityScopeRequest scope);
        Task<int?> ResolveOwnAccountPlanId(EntityScopeRequest scope);
    }
}