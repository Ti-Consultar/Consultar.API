using _2___Application._1_Services.Results;
using _2___Application._1_Services.TotalizerClassification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiquidManagementController : ControllerBase
    {
        private readonly LiquidManagementService _Service;

        public LiquidManagementController(LiquidManagementService service)
        {
            _Service = service;
        }
        [HttpGet]
        [Route("liquidity-management")]
        [Authorize()]
        public async Task<IActionResult> GetLiquidityManagement([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetLiquidityManagement(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("liquidity-management/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetLiquidityManagementComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetLiquidityManagementComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("liquidity-management/month")]
        [Authorize()]
        public async Task<IActionResult> GetLiquidityManagementMonth([FromQuery] int accountPlanId, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {

                var response = await _Service.GetLiquidityManagementMonth(accountPlanId, year, month);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("capital-dynamics")]
        [Authorize()]
        public async Task<IActionResult> GetCapitalDynamics([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCapitalDynamics(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("capital-dynamics/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetCapitalDynamicsComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCapitalDynamicsComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("turnover")]
        [Authorize()]
        public async Task<IActionResult> GetTurnover([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetTurnover(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("turnover/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetTurnoverComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetTurnoverComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("liquidity")]
        [Authorize()]
        public async Task<IActionResult> GetLiquidity([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetLiquidity(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("liquidity/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetLiquidityComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetLiquidityComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("gross-cash-flow")]
        [Authorize()]
        public async Task<IActionResult> GetGrossCashFlow([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetGrossCashFlow(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("capital-structure")]
        [Authorize()]
        public async Task<IActionResult> GetCapitalStructure([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCapitalStructure(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("capital-structure/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetCapitalStructureComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetCapitalStructureComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}