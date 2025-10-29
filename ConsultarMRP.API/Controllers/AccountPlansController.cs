using _2___Application._1_Services.AccountPlans;
using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountPlansController : ControllerBase
    {
        private readonly AccountPlansService _service;

        public AccountPlansController(AccountPlansService service)
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
        public async Task<IActionResult> Create([FromBody] InsertAccountPlan dto)
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

        /// <summary>
        /// Lista o planos de contas de acordo com os Parametros passados
        /// </summary>
        [HttpGet("list")]
        [Authorize()]
        public async Task<IActionResult> GetByFilters( [FromQuery] int groupId,[FromQuery] int? companyId,[FromQuery] int? subCompanyId)
        {
            var result = await _service.GetAccountPlans(groupId, companyId, subCompanyId);

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
    }
}
