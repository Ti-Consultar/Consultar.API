using _2___Application._1_Services.AccountPlans;
using _2___Application._1_Services.AccountPlans.Balancete;
using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _4_Application._1_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceteController : ControllerBase
    {
        private readonly BalanceteService _service;

        public BalanceteController(BalanceteService service)
        {
            _service = service;
        }

        /// <summary>
        /// Cria um novo grupo.
        /// </summary>
        /// <param name="Create">Dados para criação de um plano de contas.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] InsertBalanceteDto dto)
        {
            try
            {
                var result = await _service.Create(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id,[FromBody] UpdateBalanceteDto dto)
        {
            try
            {
                var result = await _service. Update(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Lista o planos de contas de acordo com os Parametros passados
        /// </summary>
        [Authorize()]
        [HttpGet("accountplan/{accountPlanId}")]
        public async Task<IActionResult> GetBalancetes(int accountPlanId)
        {
            var result = await _service.GetAccountPlanWithBalancetes(accountPlanId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Lista o plano de contas por Id
        /// </summary>
        [Authorize()]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Deleta
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        #region Balancete Data

        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost("import")]
        public async Task<IActionResult> ImportBalanceteData(IFormFile file, [FromQuery] int balanceteId)
        {
            var result = await _service.ImportBalanceteData(file, balanceteId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpGet("{balanceteId}/data")]
        public async Task<IActionResult> GetByBalanceteId( int balanceteId)
        {
            var result = await _service.GetByBalanceteId( balanceteId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpGet("{balanceteId}/cost-center")]
        public async Task<IActionResult> GetAgrupadoPorCostCenter(int balanceteId)
        {
            var result = await _service.GetAgrupadoPorCostCenter(balanceteId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Deleta
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpDelete("{id}/balancete-data")]
        public async Task<IActionResult> DeleteBalanceteData(int balanceteId)
        {
            var result = await _service.DeleteBalanceteData(balanceteId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        #endregion
    }
}
