using _2___Application._1_Services.CashFlow;
using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
using _2___Application._1_Services.ValueTree;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValueTreeController : ControllerBase
    {
        private readonly ValueTreeService _Service;

        public ValueTreeController(ValueTreeService service)
        {
            _Service = service;
        }
        [HttpGet]
        [Route("")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> GetAll([FromQuery] int accountPlanId, [FromQuery] int month,[FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetAll(accountPlanId, month, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}