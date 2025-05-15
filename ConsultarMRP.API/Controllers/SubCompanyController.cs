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
    public class SubCompanyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public SubCompanyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// Cria uma nova subempresa.
        /// </summary>
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> CreateSubCompany([FromBody] InsertSubCompanyDto createsubCompanyDto)
        {
            try
            {
                var company = await _companyService.CreateSubCompany(createsubCompanyDto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza uma subempresa existente pelo ID.
        /// </summary>
        [Authorize]
        [HttpPut]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> UpdateSubCompany(int id, [FromBody] UpdateSubCompanyDto dto)
        {
            try
            {
                var company = await _companyService.UpdateSubCompany(id, dto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Exclui logicamente uma subempresa pelo ID (somente gestor).
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPatch]
        [Route("{subCompanyId}/company/{id}/delete")]
        public async Task<IActionResult> DeleteCompany(int id, int subCompanyId)
        {
            try
            {
                var company = await _companyService.DeleteSubCompany(id, subCompanyId);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Restaura subempresas excluídas logicamente (somente gestor).
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPatch]
        [Route("company/{companyId}/subcompanies/restore")]
        public async Task<IActionResult> RestoreSubCompanies(int companyId, [FromBody] List<int> subCompanyIds)
        {
            try
            {
                var result = await _companyService.RestoreSubCompanies(companyId, subCompanyIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém as subempresas associadas ao usuário.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("user")]
        public async Task<IActionResult> GetSubCompaniesByUserId()
        {
            try
            {
                var subCompanies = await _companyService.GetSubCompaniesByUserId();
                return Ok(subCompanies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém uma subempresa específica pelo ID.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("{id}/company/{companyId}")]
        public async Task<IActionResult> GetSubCompaniesByUserId(int companyId, int id)
        {
            try
            {
                var subCompanies = await _companyService.GetSubCompaniesById(companyId, id);
                return Ok(subCompanies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os usuários vinculados a uma subempresa específica.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("{id}/company/{companyId}/users")]
        public async Task<IActionResult> GetUsersBySubCompanyId(int groupId, int companyId, int id)
        {
            try
            {
                var subCompanies = await _companyService.GetUsersBySubCompanyId(groupId, companyId, id);
                return Ok(subCompanies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém as subempresas excluídas associadas ao usuário.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("/deleted")]
        public async Task<IActionResult> GetSubCompaniesDeletedByUserId()
        {
            try
            {
                var subCompanies = await _companyService.GetSubCompaniesDeletedByUserId();
                return Ok(subCompanies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cria um vínculo de usuário com uma subempresa (somente gestor).
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost]
        [Route("bond")]
        public async Task<IActionResult> CreateUserSubCompany([FromBody] CreateSubCompanyUserDto dto)
        {
            try
            {
                var company = await _companyService.CreateUserSubCompany(dto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém as subempresas de uma empresa de forma paginada.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("company/{companyId}/paginated")]
        public async Task<IActionResult> GetCompaniesByUserIdPaginated(int companyId, int skip, int take)
        {
            try
            {
                var companies = await _companyService.GetSubCompaniesByUserIdPaginated(companyId, skip, take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém as subempresas excluídas de uma empresa de forma paginada.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("company/{companyId}/paginated/deleted")]
        public async Task<IActionResult> GetSubCompaniesDeletedByUserIdPaginated(int companyId, int skip, int take)
        {
            try
            {
                var companies = await _companyService.GetByIdBySubCompaniesDeleted(companyId, skip ,take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
