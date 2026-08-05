using _2___Application._1_Services.AccountingPanel.V2;
using _2___Application._2_Dto_s.AccountingPanel.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers;

[ApiController]
[Authorize]
[Route("v2/painel")]
[Produces("application/json")]
public sealed class AccountingPanelV2Controller : ControllerBase
{
    private readonly AccountingPanelV2Service _service;

    public AccountingPanelV2Controller(AccountingPanelV2Service service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna Ativo e Passivo do Painel Contábil em um contrato único orientado por linhas.
    /// </summary>
    /// <remarks>
    /// Os valores e sinais são copiados do endpoint legado /painel. Totalizadores são expandidos
    /// por parentCode e classifications incluem seus lançamentos em details.data.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(AccountingPanelV2Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(
        [FromQuery] int accountPlanId,
        [FromQuery] int year,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetAsync(accountPlanId, year, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (AccountingPanelV2MappingException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
