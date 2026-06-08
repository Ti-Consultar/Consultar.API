using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconomicIndicesController : ControllerBase
    {
        private readonly EconomicIndicesService _Service;

        public EconomicIndicesController(EconomicIndicesService service)
        {
            _Service = service;
        }
        [HttpGet]
        [Route("profitability")]
        [Authorize()]
        public async Task<IActionResult> GetProfitability([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetProfitability(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("profitability/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetProfitabilityComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetProfitabilityComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("rentability")]
        [Authorize()]
        public async Task<IActionResult> GetRentabilibty([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetRentabilibty(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("rentability/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetRentabilityComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetRentabilityComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("return-expectation")]
        [Authorize()]
        public async Task<IActionResult> GetReturnExpectation([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetReturnExpectation(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("return-expectation/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetReturnExpectationComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetReturnExpectationComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("ebitda")]
        [Authorize()]
        public async Task<IActionResult> GetEBITDA([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetEBITDA(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ebitda/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetEBITDAOrcado([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetEBITDAOrcado(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ebitda/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetEBITDAOrcados([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetEBITDAComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("nopat")]
        [Authorize()]
        public async Task<IActionResult> GetNOPAT([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetNOPAT(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("nopat/variacao")]
        [Authorize()]
        public async Task<IActionResult> GetNOPATComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetNOPATComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
