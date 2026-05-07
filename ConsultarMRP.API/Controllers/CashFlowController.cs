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

        [HttpGet]
        [Route("/rolling")]
        [Authorize()]
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

        [HttpGet]
        [Route("scope/rolling")]
        [Authorize()]
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
