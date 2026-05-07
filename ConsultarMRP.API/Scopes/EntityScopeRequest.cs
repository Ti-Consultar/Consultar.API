namespace ConsultarMRP.API.Scopes;

public sealed class EntityScopeRequest
{
    public int GroupId { get; init; }
    public int? CompanyId { get; init; }
    public int? SubCompanyId { get; init; }
    public bool IncludeChildren { get; init; } = true;
}