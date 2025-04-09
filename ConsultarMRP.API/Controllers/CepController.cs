using Microsoft.AspNetCore.Mvc;
using _2___Application.Base;

using System;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CepController : ControllerBase
    {
        private readonly CepService _cepService;

        public CepController(CepService cepService)
        {
            _cepService = cepService;
        }

        [HttpGet]
        [Route("{cep}")]
        public async Task<IActionResult> BuscarCep(string cep)
        {
            try
            {
                var result = await _cepService.BuscarCep(cep);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
