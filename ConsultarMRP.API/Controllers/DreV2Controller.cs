using _2___Application._1_Services.DRE.V2;
using _2___Application._2_Dto_s.DRE.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v2/dre")]
[Produces("application/json")]
public sealed class DreV2Controller : ControllerBase
{
    private readonly DreV2Service _service;

    public DreV2Controller(DreV2Service service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna a DRE comparativa V2 orientada por linhas e períodos.
    /// </summary>
    /// <remarks>
    /// periods define as colunas; scenarios define realizado, orçado e variação.
    /// Em cada linha, values é indexado primeiro pelo cenário e depois pelo período.
    /// code é a identidade estável, displayOrder define a ordem visual, parentCode
    /// define a hierarquia e valueType distingue moeda de percentual.
    /// Linhas de classificação incluem os lançamentos já disponíveis no resultado
    /// legado em details.data, também indexados por cenário e período.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(DreV2Response), StatusCodes.Status200OK)]
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
        catch (DreV2MappingException ex)
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

    /// <summary>
    /// Retorna os lançamentos contábeis de uma linha, cenário e período da DRE V2.
    /// </summary>
    [HttpGet("rows/{rowCode}/details")]
    [ProducesResponseType(typeof(DreRowDetailsV2Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(
        [FromRoute] string rowCode,
        [FromQuery] int accountPlanId,
        [FromQuery] int year,
        [FromQuery] string scenario,
        [FromQuery] string period,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetDetailsAsync(
                accountPlanId,
                year,
                rowCode,
                scenario,
                period,
                cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DreV2MappingException ex)
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
