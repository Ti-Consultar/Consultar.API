using _2___Application._1_Services.CashFlow;
using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
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
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
    }
}