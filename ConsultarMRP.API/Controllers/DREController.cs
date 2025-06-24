
using Microsoft.AspNetCore.Mvc;
using _2___Application._1_Services.DRE;
using _2___Application._2_Dto_s.DRE;
using _2___Application._2_Dto_s.DRE.BalanceteDRE;
using Microsoft.AspNetCore.Authorization;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DREController : ControllerBase
    {
        private readonly DREService _service;

        public DREController(DREService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Create(InsertDRE dto)
        {
            try
            {
                var response = await _service.Create(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize()]
        public async Task<IActionResult> GetAll([FromQuery] int accountplanId)
        {
            try
            {
                if (accountplanId <= 0)
                    return BadRequest(new { message = "accountplanId inválido" });

                var response = await _service.GetAll(accountplanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize()]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("accountPlan/{accountplanId}")]
        [Authorize()]
        public async Task<IActionResult> GetAllByAccountPlan( int accountplanId)
        {
            try
            {
                if (accountplanId <= 0)
                    return BadRequest(new { message = "accountplanId inválido" });

                var response = await _service.GetDREGroupedByAccountPlanAsync(accountplanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("/bond")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> CreateBondDreBalanceteData(BalanceteDRE dto)
        {
            try
            {
                var response = await _service.Vincular(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
