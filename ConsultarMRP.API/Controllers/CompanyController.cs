using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
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

        [HttpPut]
        [Route("update/id/{id}")]
        public async Task<IActionResult> UpdateCompany(int id,[FromBody] UpdateCompanyDto dto)
        {
            try
            {

                var company = await _companyService.UpdateCompany(id,dto);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch]
        [Route("user/{userId}/group/{groupId}/company/{id}/delete")]
        public async Task<IActionResult> DeleteCompany(int userId, int id, int groupId)
        {
            try
            {

                var company = await _companyService.DeleteCompany(userId, id, groupId);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPatch]
        [Route("user/{userId}/group/{groupId}/companies/restore")]
        public async Task<IActionResult> RestoreCompanies(int userId, int groupId, [FromBody] List<int> companyIds)
        {
            try
            {
                var result = await _companyService.RestoreCompanies(userId, companyIds, groupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("user/{userId}/group/{groupId}")]
        public async Task<IActionResult> GetCompaniesByUserId(int userId, int groupId)
        {
            try
            {
               
                var companies = await _companyService.GetCompaniesByUserId(userId, groupId);
                return Ok(companies);  
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });  
            }
        }
        [HttpGet]
        [Route("user/{userId}/group/{groupId}/deleted")]
        public async Task<IActionResult> GetByIdByCompaniesDeleted(int userId, int groupId)
        {
            try
            {

                var companies = await _companyService.GetByIdByCompaniesDeleted(userId, groupId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("{id}/user/{userId}/group/{groupId}")]
        public async Task<IActionResult> GetCompanyById(int id,int userId, int groupId)
        {
            try
            {

                var companies = await _companyService.GetCompanyById(id, userId, groupId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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
        [HttpGet]
        [Route("paginated/user/{userId}/group/{groupId}")]
        public async Task<IActionResult> GetCompaniesByUserIdPaginated(int userId, int groupId ,int skip, int take)
        {
            try
            {
               
                var companies = await _companyService.GetCompaniesByUserIdPaginated(userId, groupId, skip, take);
                return Ok(companies); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
        }
        [HttpGet]
        [Route("paginated/user/{userId}/group/{groupId}/deleted")]
        public async Task<IActionResult> GetByIdByCompaniesDeletedPaginated(int userId, int groupId, int skip, int take)
        {
            try
            {

                var companies = await _companyService.GetByIdByCompaniesDeletedPaginated(userId, groupId, skip, take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
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
