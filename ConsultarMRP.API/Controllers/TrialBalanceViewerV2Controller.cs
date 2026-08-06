using System.Security.Claims;
using _2___Application._1_Services.AccountPlans.Balancete.V2;
using _2___Application._2_Dto_s.AccountPlan.Balancete.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers;

[ApiController]
[Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor,Usuario")]
[Route("api/v2/trial-balances/{trialBalanceId:int}/viewer/rows")]
[Produces("application/json")]
public sealed class TrialBalanceViewerV2Controller : ControllerBase
{
    private readonly TrialBalanceViewerV2Service _service;

    public TrialBalanceViewerV2Controller(TrialBalanceViewerV2Service service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna um bloco de linhas do balancete para visualização em grade infinita.
    /// </summary>
    /// <remarks>
    /// Use offset como startRow e limit como endRow-startRow do AG Grid. A paginação é
    /// estável e o total considera search, levels e onlyWithMovement antes do bloco.
    /// Ordenações permitidas: id, sourceOrder, accountCode, description, level,
    /// previousBalance, debit, credit e finalBalance. Esta versão aceita uma ordenação.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(TrialBalanceViewerRowsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromRoute] int trialBalanceId,
        [FromQuery] TrialBalanceViewerRowsRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        try
        {
            return Ok(await _service.GetRowsAsync(
                trialBalanceId,
                userId,
                request,
                cancellationToken));
        }
        catch (TrialBalanceViewerNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }
}
