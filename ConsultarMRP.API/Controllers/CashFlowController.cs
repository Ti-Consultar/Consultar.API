using _2___Application._1_Services.CashFlow;
using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
using _2___Application._1_Services.Scope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashFlowController : ControllerBase
    {
        private readonly CashFlowService _Service;

        public CashFlowController(CashFlowService service)
        {
            _Service = service;
        }
        [HttpGet]
        [Route("")]
        [Authorize()]
        public async Task<IActionResult> GetCashFlow([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCashFlow(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("scope")]
        [Authorize()]
        public async Task<IActionResult> GetCashFlowByScope(
            [FromQuery] int groupId,
            [FromQuery] int? companyId,
            [FromQuery] int? subCompanyId,
            [FromQuery] int year,
            [FromQuery] bool includeChildren = true)
        {
            try
            {
                var response = await _Service.GetCashFlow(
                    CreateScope(groupId, companyId, subCompanyId, includeChildren),
                    year);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetCashFlowComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCashFlowComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("scope/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetCashFlowComparativoByScope(
            [FromQuery] int groupId,
            [FromQuery] int? companyId,
            [FromQuery] int? subCompanyId,
            [FromQuery] int year,
            [FromQuery] bool includeChildren = true)
        {
            try
            {
                var response = await _Service.GetCashFlowComparativo(
                    CreateScope(groupId, companyId, subCompanyId, includeChildren),
                    year);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retorna o Fluxo de Caixa comparativo com o agrupamento anual Rolling.
        /// </summary>
        /// <remarks>
        /// annual é posicionado após o acumulado e identificado por type=rolling e
        /// pelo ano consultado. O grupo contém Orçado, Rolling e Variação. Rolling
        /// combina realizado até o último período efetivamente disponível com o
        /// orçamento dos períodos posteriores; Orçado soma todo o orçamento disponível.
        /// Saldos inicial e final mantêm o tratamento não aditivo do Fluxo de Caixa.
        /// </remarks>
        [HttpGet]
        [Route("/rolling")]
        [Authorize()]
        [ProducesResponseType(typeof(_2___Application._2_Dto_s.CashFlow.PainelCashFlowComparativoRollingResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCashFlowComparativoRolling([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCashFlowComparativoRolling(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retorna o Fluxo de Caixa comparativo Rolling consolidado por escopo.
        /// </summary>
        [HttpGet]
        [Route("scope/rolling")]
        [Authorize()]
        [ProducesResponseType(typeof(_2___Application._2_Dto_s.CashFlow.PainelCashFlowComparativoRollingResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCashFlowComparativoRollingByScope(
            [FromQuery] int groupId,
            [FromQuery] int? companyId,
            [FromQuery] int? subCompanyId,
            [FromQuery] int year,
            [FromQuery] bool includeChildren = true)
        {
            try
            {
                var response = await _Service.GetCashFlowComparativoRolling(
                    CreateScope(groupId, companyId, subCompanyId, includeChildren),
                    year);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static EntityScopeRequest CreateScope(
            int groupId,
            int? companyId,
            int? subCompanyId,
            bool includeChildren)
        {
            return new EntityScopeRequest
            {
                GroupId = groupId,
                CompanyId = companyId,
                SubCompanyId = subCompanyId,
                IncludeChildren = includeChildren
            };
        }
    }
}
