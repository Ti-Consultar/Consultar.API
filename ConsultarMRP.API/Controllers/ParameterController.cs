using _2___Application._1_Services.Parameter;
using _2___Application._2_Dto_s.Classification.AccountPlanClassification;
using _2___Application._2_Dto_s.Parameter;
using _4_Application._1_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParameterController : ControllerBase
    {
        private readonly ParameterService _service;

        public ParameterController(ParameterService service)
        {
            _service = service;
        }


        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Create([FromBody] InsertParameterDto dto)
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
        [Route("")]
        [Authorize()]
        public async Task<IActionResult> GetAll(int accountPlanId)
        {
            try
            {

                var response = await _service.GetAll(accountPlanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/{id}")]
        [Authorize()]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {

                var response = await _service.GetById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("wacc")]
        [Authorize()]
        public async Task<IActionResult> GetWACCById([FromQuery] int accountPlanId, [FromQuery]int year)
        {
            try
            {

                var response = await _service.GetWACCById(accountPlanId , year);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("")]
       [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Update(UpdateParameterDto dto)
        {
            try
            {

                var response = await _service.Update(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Update([FromQuery]int id)
        {
            try
            {

                var response = await _service.Delete(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
