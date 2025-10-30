using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public CompanyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// Cria uma nova empresa.
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateCompany([FromBody] InsertCompanyDto createCompanyDto)
        {
            try
            {
                var company = await _companyService.CreateCompany(createCompanyDto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza uma empresa existente pelo ID.
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyDto dto)
        {
            try
            {
                var company = await _companyService.UpdateCompany(id, dto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Exclui logicamente uma empresa pelo ID (somente gestor).
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPatch]
        [Route("{id}/group/{groupId}/delete")]
        public async Task<IActionResult> DeleteCompany(int id, int groupId)
        {
            try
            {
                var company = await _companyService.DeleteCompany(id, groupId);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Restaura empresas excluídas logicamente (somente gestor).
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPatch]
        [Route("group/{groupId}/restore")]
        public async Task<IActionResult> RestoreCompanies(int groupId, [FromBody] List<int> companyIds)
        {
            try
            {
                var result = await _companyService.RestoreCompanies(companyIds, groupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém as empresas associadas ao usuário em um grupo específico.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("group/{groupId}")]
        public async Task<IActionResult> GetCompaniesByUserId(int groupId)
        {
            try
            {
                var companies = await _companyService.GetCompaniesByUserId(groupId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Obtém as empresas associadas ao usuário em um grupo específico.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetCompaniesByUser()
        {
            try
            {
                var companies = await _companyService.GetCompaniesByUser();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Obtém as empresas excluídas logicamente associadas a um grupo.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("group/{groupId}/deleted")]
        public async Task<IActionResult> GetByIdByCompaniesDeleted(int groupId)
        {
            try
            {
                var companies = await _companyService.GetByIdByCompaniesDeleted(groupId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém uma empresa específica pelo ID e grupo.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("{id}/group/{groupId}")]
        public async Task<IActionResult> GetCompanyById(int id, int groupId)
        {
            try
            {
                var companies = await _companyService.GetCompanyById(id, groupId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os usuários vinculados a uma empresa específica.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("{id}/group/{groupId}/users")]
        public async Task<IActionResult> GetUsersByCompanyId(int groupId, int id)
        {
            try
            {
                var companies = await _companyService.GetUsersByCompanyId(groupId, id);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os usuários vinculados a uma empresa específica.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("/group/{groupId}/filiais")]
        public async Task<IActionResult> GetSimpleCompaniesByGroupId(int groupId)
        {
            try
            {
                var companies = await _companyService.GetSimpleCompaniesByGroupId(groupId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Obtém as empresas de um grupo de forma paginada.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("group/{groupId}/paginated")]
        public async Task<IActionResult> GetCompaniesByUserIdPaginated(int groupId, int skip, int take)
        {
            try
            {
                var companies = await _companyService.GetCompaniesByUserIdPaginated(groupId, skip, take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém as empresas excluídas logicamente de um grupo de forma paginada.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("group/{groupId}/paginated/deleted")]
        public async Task<IActionResult> GetByIdByCompaniesDeletedPaginated(int groupId, int skip, int take)
        {
            try
            {
                var companies = await _companyService.GetByIdByCompaniesDeletedPaginated(groupId, skip, take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cria um vínculo de usuário com uma empresa (somente gestor).
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost]
        [Route("bond")]
        public async Task<IActionResult> CreateUserCompany([FromBody] CreateCompanyUserDto dto)
        {
            try
            {
                var company = await _companyService.CreateUserCompany(dto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
