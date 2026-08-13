using _2___Application._1_Services.Scope;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;

namespace _2___Application._1_Services.BreakEven.V2;

public interface IBreakEvenScopeService
{
    Task<int> ResolveAuthorizedAccountPlanIdAsync(
        EntityScopeRequest scope,
        int userId,
        CancellationToken cancellationToken);
}

public sealed class BreakEvenScopeService : IBreakEvenScopeService
{
    private readonly IAccountPlanScopeResolver _scopeResolver;
    private readonly CoreServiceDbContext _context;

    public BreakEvenScopeService(
        IAccountPlanScopeResolver scopeResolver,
        CoreServiceDbContext context)
    {
        _scopeResolver = scopeResolver;
        _context = context;
    }

    public async Task<int> ResolveAuthorizedAccountPlanIdAsync(
        EntityScopeRequest scope,
        int userId,
        CancellationToken cancellationToken)
    {
        ValidateScope(scope, userId);
        cancellationToken.ThrowIfCancellationRequested();

        // O plano é resolvido pelo escopo financeiro explícito; accountPlanId não define o nível.
        var accountPlanId = await _scopeResolver.ResolveOwnAccountPlanId(scope);
        if (!accountPlanId.HasValue)
            throw new BreakEvenNotFoundException("Não existe plano de contas para o escopo informado.");

        var hasAccess = await _context.CompanyUsers
            .AsNoTracking()
            .AnyAsync(access =>
                access.UserId == userId &&
                access.GroupId == scope.GroupId &&
                (
                    access.CompanyId == null ||
                    (
                        scope.CompanyId.HasValue &&
                        access.CompanyId == scope.CompanyId &&
                        (
                            access.SubCompanyId == null ||
                            (scope.SubCompanyId.HasValue && access.SubCompanyId == scope.SubCompanyId)
                        )
                    )
                ), cancellationToken);

        if (!hasAccess)
            throw new BreakEvenAccessDeniedException();

        return accountPlanId.Value;
    }

    private static void ValidateScope(EntityScopeRequest scope, int userId)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (userId <= 0)
            throw new BreakEvenAccessDeniedException();
        if (scope.GroupId <= 0)
            throw new BreakEvenValidationException("groupId deve ser maior que zero.");
        if (scope.CompanyId <= 0)
            throw new BreakEvenValidationException("companyId deve ser maior que zero quando informado.");
        if (scope.SubCompanyId <= 0)
            throw new BreakEvenValidationException("subCompanyId deve ser maior que zero quando informado.");
        if (scope.SubCompanyId.HasValue && !scope.CompanyId.HasValue)
            throw new BreakEvenValidationException("companyId é obrigatório quando subCompanyId é informado.");
    }
}
