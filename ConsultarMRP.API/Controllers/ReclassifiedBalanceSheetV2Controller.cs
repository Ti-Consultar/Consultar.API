using _2___Application._1_Services.ReclassifiedBalanceSheet.V2;
using _2___Application._2_Dto_s.ReclassifiedBalanceSheet.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v2/reclassified-balance-sheet")]
[Produces("application/json")]
public sealed class ReclassifiedBalanceSheetV2Controller : ControllerBase
{
    private readonly ReclassifiedBalanceSheetV2Service _service;

    public ReclassifiedBalanceSheetV2Controller(ReclassifiedBalanceSheetV2Service service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna Ativo e Passivo reclassificados em um único contrato orientado por linhas e períodos.
    /// </summary>
    /// <remarks>
    /// Os valores, sinais, totais, acumulados e variações são copiados dos resultados legados.
    /// Os grupos expansíveis incluem em details.data seus totalizadores componentes; cada componente
    /// informa expandable e contém suas classificações e dados contábeis. Tudo é indexado por cenário
    /// e período. Totais principais e linhas calculadas não possuem details.data.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ReclassifiedBalanceSheetV2Response), StatusCodes.Status200OK)]
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
        catch (ReclassifiedBalanceSheetV2MappingException ex)
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
