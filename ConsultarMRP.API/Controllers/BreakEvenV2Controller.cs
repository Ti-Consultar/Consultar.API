using System.Security.Claims;
using _2___Application._1_Services.BreakEven.V2;
using _2___Application._1_Services.DRE.V2;
using _2___Application._1_Services.Scope;
using _2___Application._2_Dto_s.BreakEven.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v2/break-even")]
[Produces("application/json")]
public sealed class BreakEvenV2Controller : ControllerBase
{
    private readonly BreakEvenV2Service _service;

    public BreakEvenV2Controller(BreakEvenV2Service service)
    {
        _service = service;
    }

    /// <summary>Calcula o Ponto de Equilíbrio mensal sem simulações e com fator zero.</summary>
    /// <remarks>
    /// Os valores monetários são rebuscados da DRE realizada do escopo e exclusivamente do mês
    /// informado. O cálculo é stateless e não persiste cenários nem altera a DRE.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(BreakEvenV2Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromQuery] int groupId,
        [FromQuery] int? companyId,
        [FromQuery] int? subCompanyId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId, out var unauthorized))
            return unauthorized!;

        return await ExecuteAsync(() => _service.GetAsync(
            CreateScope(groupId, companyId, subCompanyId),
            year,
            month,
            userId,
            cancellationToken), cancellationToken);
    }

    /// <summary>Calcula uma simulação mensal do Ponto de Equilíbrio.</summary>
    /// <remarks>
    /// Percentuais usam fração decimal: 0.10 = 10% e -0.05 = -5%. O fator é aplicado uma única
    /// vez sobre o PE base. Apenas rowCode, percentage, escopo e período influenciam o cálculo;
    /// valores monetários eventualmente enviados pelo cliente são ignorados pelo contrato.
    /// </remarks>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(BreakEvenV2Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Simulate(
        [FromBody] BreakEvenSimulationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId, out var unauthorized))
            return unauthorized!;

        return await ExecuteAsync(
            () => _service.SimulateAsync(request, userId, cancellationToken),
            cancellationToken);
    }

    private async Task<IActionResult> ExecuteAsync(
        Func<Task<BreakEvenV2Response>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await action());
        }
        catch (BreakEvenAccessDeniedException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (BreakEvenNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BreakEvenValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (BreakEvenCalculationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (OverflowException)
        {
            return BadRequest(new { message = "Os percentuais informados excedem a faixa decimal suportada." });
        }
        catch (DreV2MappingException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    private bool TryGetUserId(out int userId, out IActionResult? unauthorized)
    {
        var value = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(value, out userId) && userId > 0)
        {
            unauthorized = null;
            return true;
        }

        unauthorized = Unauthorized(new { message = "Usuário autenticado inválido." });
        return false;
    }

    private static EntityScopeRequest CreateScope(int groupId, int? companyId, int? subCompanyId) =>
        new()
        {
            GroupId = groupId,
            CompanyId = companyId,
            SubCompanyId = subCompanyId,
            IncludeChildren = false
        };
}
