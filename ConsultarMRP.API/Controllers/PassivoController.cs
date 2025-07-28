using _2___Application._1_Services;
using _2___Application._1_Services.Passivo;
using _2___Application._2_Dto_s.DRE.BalanceteDRE;
using _2___Application._2_Dto_s.Passivo;
using _4_Application._1_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassivoController : ControllerBase
    {
        private readonly PassivoService _Service;

        public PassivoController(PassivoService service)
        {
            _Service = service;
        }

        [HttpGet]
        [Route("")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> GetAll()
        {
            try
            {

                var response = await _Service.GetAll();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {

                var result = await _Service.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost()]
        [Route("bond-passivo")]
        //[Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> CreateBondDreBalanceteData(BondPassivoBalanceteData dto)
        {
            try
            {
                var response = await _Service.Vincular(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
