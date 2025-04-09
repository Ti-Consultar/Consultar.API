
using _2___Application._2_Dto_s.CNPJ;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    // Controller: CnpjController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class CnpjController : ControllerBase
    {
        private readonly CnpjService _cnpjService;
        private readonly CompanyService _companyService;

        public CnpjController(CnpjService cnpjService, CompanyService companyService)
        {
            _cnpjService = cnpjService;
            _companyService = companyService;
        }

        [HttpGet]
        [Route("{cnpj}")]
        public async Task<IActionResult> GetCnpjData(string cnpj)
        {
            try
            {
                var result = await _cnpjService.BuscarCnpj(cnpj);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("save-cnpj")]
        //public async Task<IActionResult> SaveCnpjCompany([FromBody] CnpjResponseDto dto)
        //{
        //    await _companyService.SaveCompanyFromCnpj(dto);
        //    return Ok("Empresa salva com sucesso!");
        //}
    }
}
