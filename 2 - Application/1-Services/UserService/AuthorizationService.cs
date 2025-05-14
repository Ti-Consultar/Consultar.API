using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

public class AuthorizationService
{
    private readonly IAuthorizationPolicyProvider _policyProvider;

    public AuthorizationService(IAuthorizationPolicyProvider policyProvider)
    {
        _policyProvider = policyProvider;
    }

    public async Task<IEnumerable<string>> GetAllPoliciesAsync()
    {
        var policies = new List<string>();

        // Obtendo todas as policies registradas
        var policyNames = new[] { "Admin", "Gestor", "Usuario", "Consultor", "Comercial", "Desenvolvedor", "Designer" };

        foreach (var policyName in policyNames)
        {
            var policy = await _policyProvider.GetPolicyAsync(policyName);
            if (policy != null)
            {
                var roles = policy.Requirements
                    .OfType<RolesAuthorizationRequirement>()
                    .SelectMany(r => r.AllowedRoles);

                var rolesList = string.Join(", ", roles);
                policies.Add($"{policyName}: {rolesList}");
            }
        }

        return policies;
    }
}
