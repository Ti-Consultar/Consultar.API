using _2___Application._1_Services;
using _2___Application._2_Dto_s.Classification;
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
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
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
        [Authorize()]
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

        [HttpPut]
        [Route("/create-bond")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> CreateBond([FromQuery]int accountPlanClassificationId, [FromBody] BalanceteDataAccountPlanClassificationCreate dto)
        {
            try
            {

                var response = await _Service.CreateBond(accountPlanClassificationId, dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut]
        [Route("/create-bond-list")]
        [Authorize()]
        public async Task<IActionResult> CreateBondList( [FromBody] BalanceteDataAccountPlanClassificationCreateList dto)
        {
            try
            {

                var response = await _Service.CreateBondList( dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("accountplan/{accountPlanId}/update-bond-list")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> CreateBondList(int accountPlanId,[FromBody] BalanceteDataAccountPlanClassificationCreateList dto)
        {
            try
            {

                var response = await _Service.UpdateBondList(accountPlanId,dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/bond")]
        [Authorize()]
        public async Task<IActionResult> GetPainelBalancoAsync([FromQuery] int accountPlanId, [FromQuery] int typeClassification)
        {
            try
            {

                var response = await _Service.GetBond(accountPlanId, typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("bond-list/{accountPlanId}")]
        public async Task<IActionResult> GetBondListByAccountPlanId(int accountPlanId)
        {
            var result = await _Service.GetBondListByAccountPlanId(accountPlanId);
            return Ok(result);
        }

        [HttpGet]
        [Route("/painel")]
        [Authorize()]
        public async Task<IActionResult> GetPainelBalancoAsync([FromQuery] int accountPlanId, [FromQuery] int year, [FromQuery] int typeClassification)
        {
            try
            {

                var response = await _Service.GetPainelBalancoAsync(accountPlanId, year,typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("/painel/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetPainelBalancoOrcadoAsync([FromQuery] int accountPlanId, [FromQuery] int year, [FromQuery] int typeClassification)
        {
            try
            {

                var response = await _Service.GetPainelBalancoOrcadoAsync(accountPlanId, year, typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route("/painel/comparativo-DRE")]
        //[Authorize()]
        //public async Task<IActionResult> GetPainelBalancoComparativoAsync([FromQuery] int accountPlanId, [FromQuery] int year)
        //{
        //    try
        //    {

        //        var response = await _Service.BuildPainelDREComparativoCompleto(accountPlanId, year);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
        [Authorize()]
        [HttpGet]
        [Route("/painel-reclassificado")]
     
        public async Task<IActionResult> GetPainelBalancoReclassificadoAsync([FromQuery] int accountPlanId, [FromQuery] int year, [FromQuery] int typeClassification)
        {
            try
            {

                var response = await _Service.GetPainelBalancoReclassificadoAsync(accountPlanId, year, typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/painel-reclassificado/orcado")]
        [Authorize()]
        public async Task<IActionResult> GetPainelBalancoReclassificadoOrcadoAsync([FromQuery] int accountPlanId, [FromQuery] int year, [FromQuery] int typeClassification)
        {
            try
            {

                var response = await _Service.GetPainelBalancoReclassificadoOrcadoAsync(accountPlanId, year, typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/painel-reclassificado/comparativo")]
        [Authorize()]
        public async Task<IActionResult> GetPainelBalancoReclassificadoComparativoAsync([FromQuery] int accountPlanId, [FromQuery] int year, [FromQuery] int typeClassification)
        {
            try
            {

                var response = await _Service.GetPainelBalancoReclassificadoComparativoAsync(accountPlanId, year, typeClassification);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("exists")]
        [Authorize()]
        public async Task<IActionResult> GetAccountPlanClassification([FromQuery] int accountPlanId)
        {
            try
            {

                var response = await _Service.GetAccountPlanClassification(accountPlanId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("/demonstracao-consolidado")]
    
        public async Task<IActionResult> GetDREGrupoEmpresasAno([FromQuery]int groupId, [FromQuery] List<int> companyIds, [FromQuery] int year)
        {
            try
            {

                var response = await _Service.GetDREGrupoEmpresasAno(groupId,companyIds, year);
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
