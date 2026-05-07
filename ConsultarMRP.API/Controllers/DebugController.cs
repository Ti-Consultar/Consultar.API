using _2___Application._1_Services.Scope;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ScopeController : ControllerBase
{
    private readonly IAccountPlanScopeResolver _scopeResolver;

    public ScopeController(IAccountPlanScopeResolver scopeResolver)
    {
        _scopeResolver = scopeResolver;
    }

    [HttpGet("debug")]
    public async Task<IActionResult> Debug(
        int groupId,
        int? companyId,
        int? subCompanyId,
        bool includeChildren = true)
    {
        var scope = new EntityScopeRequest
        {
            GroupId = groupId,
            CompanyId = companyId,
            SubCompanyId = subCompanyId,
            IncludeChildren = includeChildren
        };

        var ids = await _scopeResolver.ResolveAccountPlanIds(scope);

        return Ok(ids);
    }
}