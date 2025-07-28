using _2___Application._1_Services.Results;
using _2___Application._1_Services.TotalizerClassification;
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
        //[HttpGet]
        //[Route("template")]
        //// [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        //public async Task<IActionResult> GetAll()
        //{
        //    try
        //    {

        //        var response = await _Service.GetAll();
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}