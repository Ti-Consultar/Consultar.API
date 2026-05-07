using _2___Application._2_Dto_s.ConfigPrincipal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ConfigService _Service;

        public ConfigController(ConfigService service)
        {
            _Service = service;
        }

        /* =========================
           CREATE VIEW CONFIG
        ========================= */

        [HttpPost]
        [Route("view-config")]
        [Authorize()]
        public async Task<IActionResult> CreateViewConfig([FromBody] InsertViewConfigDto dto)
        {
            try
            {
                var response = await _Service.CreateViewConfig(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("exists-view-config")]
        [Authorize()]
        public async Task<IActionResult> ExistsViewConfig([FromQuery] int accountPlanId)
        {
            try
            {
                var response = await _Service.ExistsViewConfig(accountPlanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("menu/v2")]
        [Authorize()]
        public async Task<IActionResult> GetMenuByAccountPlanId([FromQuery] int accountPlanId)
        {
            try
            {
                var response = await _Service.GetMenuByAccountPlanId(accountPlanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /* =========================
           CONFIG PRINCIPAL
        ========================= */

        [HttpGet]
        [Route("principals")]
        [Authorize()]
        public async Task<IActionResult> GetConfigPrincipals()
        {
            try
            {
                var response = await _Service.GetConfigPrincipals();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("menu")]
        [Authorize()]
        public async Task<IActionResult> GetMenuByAccountPlan([FromQuery] int accountPlanId)
        {
            try
            {
                var response = await _Service.GetMenuByAccountPlan(accountPlanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("principal-tree")]
       // [Authorize()]
        public async Task<IActionResult> GetConfigPrincipalTree()
        {
            try
            {
                var response = await _Service.GetConfigPrincipalTree();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /* =========================
           SON CONFIG
        ========================= */

        [HttpGet]
        [Route("sons")]
        [Authorize()]
        public async Task<IActionResult> GetSonConfigsByPrincipal([FromQuery] int configPrincipalId)
        {
            try
            {
                var response = await _Service.GetSonConfigsByPrincipal(configPrincipalId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("son")]
        [Authorize()]
        public async Task<IActionResult> GetSonConfigById([FromQuery] int id)
        {
            try
            {
                var response = await _Service.GetSonConfigById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /* =========================
           VIEW CONFIG
        ========================= */

        [HttpGet]
        [Route("views-by-son")]
        [Authorize()]
        public async Task<IActionResult> GetViewConfigsBySonConfig([FromQuery] int sonConfigId)
        {
            try
            {
                var response = await _Service.GetViewConfigsBySonConfig(sonConfigId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("views-by-principal")]
        [Authorize()]
        public async Task<IActionResult> GetViewConfigsByPrincipal([FromQuery] int configPrincipalId)
        {
            try
            {
                var response = await _Service.GetViewConfigsByPrincipal(configPrincipalId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("view")]
        [Authorize()]
        public async Task<IActionResult> GetViewConfigById([FromQuery] int id)
        {
            try
            {
                var response = await _Service.GetViewConfigById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}