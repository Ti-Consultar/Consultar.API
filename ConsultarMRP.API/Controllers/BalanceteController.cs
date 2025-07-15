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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBalanceteDto dto)
        {
            try
            {
                var result = await _service.Update(id, dto);
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
        [HttpGet("accountplan/{accountPlanId}")]
        [Authorize()]
        public async Task<IActionResult> GetAccountPlanWithBalancetesMonth(int accountPlanId )
        {
            var result = await _service.GetAccountPlanWithBalancetesMonth(accountPlanId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Lista o planos de contas de acordo com os Parametros passados
        /// </summary>
        [HttpGet("accountplan/{accountPlanId}/filter")]
        [Authorize()]
        public async Task<IActionResult> GetBalancetes(int accountPlanId, [FromQuery] char tipo)
        {
            var result = await _service.GetAccountPlanWithBalancetes(accountPlanId, tipo);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Lista o plano de contas por Id
        /// </summary>
        [HttpGet("{id}")]
        [Authorize()]
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
        [HttpDelete("{id}")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        #region Balancete Data

        // [Authorize]
        [HttpPost("import")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> ImportBalanceteData(IFormFile file, [FromQuery] int balanceteId)
        {
            var result = await _service.ImportBalanceteData(file, balanceteId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        // [Authorize]
        [HttpGet("{balanceteId}/data")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor,Usuario")]
        public async Task<IActionResult> GetByBalanceteId(int balanceteId)
        {
            var result = await _service.GetByBalanceteId(balanceteId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{balanceteId}/cost-center")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor,Usuario")]
        public async Task<IActionResult> GetAgrupadoPorCostCenter(int balanceteId)
        {
            var result = await _service.GetAgrupadoPorCostCenter(balanceteId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{balanceteId}/cost-center/search")]
        //[Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor,Usuario")]
        public async Task<IActionResult> GetAgrupadoByCostCenter(int balanceteId, string? search)
        {
            var result = await _service.GetAgrupadoByCostCenter(balanceteId, search);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{balanceteId}/filter")]
         [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor,Usuario")]
        public async Task<IActionResult> GetAgrupadoPorTipo(int balanceteId, [FromQuery] char tipo)
        {
            var result = await _service.GetAgrupadoPorTipo(balanceteId, tipo);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

       

        /// <summary>
        /// Deleta
        /// </summary>
        [HttpDelete("{id}/balancete-data")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
