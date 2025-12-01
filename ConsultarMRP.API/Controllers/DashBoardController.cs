using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly EconomicIndicesService _Service;

        public DashBoardController(EconomicIndicesService service)
        {
            _Service = service;
        }

        [HttpGet]
        [Route("/group")]
        [Authorize()]
        public async Task<IActionResult> GetGroupDashboard([FromQuery] int groupId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetGroupDashboard(groupId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("/group/consolidado")]
        [Authorize()]
        public async Task<IActionResult> GetGroupDashboardConsolidado([FromQuery] int groupId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetGroupDashboardConsolidado(groupId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("")]
        [Authorize()]
        public async Task<IActionResult> GetDashboard([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetDashboard(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("gestao-prazo-medio")]
        [Authorize()]
        public async Task<IActionResult> GetDashboardGestaoPrazoMedio([FromQuery] int accountPlanId, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetDashboardGestaoPrazoMedio(accountPlanId, year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}