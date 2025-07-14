using _2___Application._1_Services;
using _2___Application._2_Dto_s.Classification.AccountPlanClassification;
using _3_Domain._2_Enum_s;
using _4_Application._1_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarMRP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassificationController : ControllerBase
    {
        private readonly ClassificationService _Service;

        public ClassificationController(ClassificationService service)
        {
            _Service = service;
        }

        #region Template

        [HttpGet]
        [Route("")]
       // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Route("template/typeClassification")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> GetAllTemplate( ETypeClassification typeClassification)
        {
            try
            {

                var response = await _Service.GetByTypeClassificationTemplate(typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region AccountPlan Classification

        [HttpPost]
        [Route("")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Create( CreateAccountPlanClassification dto)
        {
            try
            {

                var response = await _Service.Create(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut]
        [Route("{accountPlanId}/create-item")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> CreateItemClassification(int accountPlanId, [FromBody]CreateItemClassification dto)
        {
            try
            {

                var response = await _Service.CreateItemClassification(accountPlanId,dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("{accountPlanId}/update-item")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> Update(int accountPlanId,[FromQuery]int id ,[FromBody] UpdateItemClassification dto)
        {
            try
            {

                var response = await _Service.Update(accountPlanId, id,dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("accountPlan/{accountPlanId}/typeClassification")]
        // [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> GetByTypeClassificationReal(int accountPlanId, ETypeClassification typeClassification)
        {
            try
            {

                var response = await _Service.GetByTypeClassificationReal(accountPlanId, typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
