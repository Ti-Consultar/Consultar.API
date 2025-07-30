using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
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
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Route("rentability")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Route("ebitda")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Route("nopat")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
    }
}