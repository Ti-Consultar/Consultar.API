using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationalEfficiencyController : ControllerBase
    {
        private readonly OperationalEfficiencyService _Service;

        public OperationalEfficiencyController(OperationalEfficiencyService service)
        {
            _Service = service;
        }
        [HttpGet]
        [Route("")]
        [Authorize()]
        public async Task<IActionResult> GetOperationalEfficiency([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetOperationalEfficiency(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetOperationalEfficiencyComparativo([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetOperationalEfficiencyComparativo(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}