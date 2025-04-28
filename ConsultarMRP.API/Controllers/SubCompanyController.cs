using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpPost]
        [Route("create")]
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

        [HttpPut]
        [Route("update/{id}")]
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

        [HttpPatch]
        [Route("{subCompanyId}/user/{userId}/company/{id}")]
        public async Task<IActionResult> DeleteCompany(int userId, int id, int subCompanyId)
        {
            try
            {

                var company = await _companyService.DeleteSubCompany(userId, id, subCompanyId);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch]
        [Route("{subCompanyId}/user/{userId}/company/{id}/restore")]
        public async Task<IActionResult> RestoreSubCompany(int userId, int id, int subCompanyId)
        {
            try
            {

                var company = await _companyService.RestoreSubCompany(userId, id, subCompanyId);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetSubCompaniesByUserId(int userId)
        {
            try
            {

                var subCompanies = await _companyService.GetSubCompaniesByUserId(userId);
                return Ok(subCompanies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("user/{userId}/deleted")]
        public async Task<IActionResult> GetSubCompaniesDeletedByUserId(int userId)
        {
            try
            {

                var subCompanies = await _companyService.GetSubCompaniesDeletedByUserId(userId);
                return Ok(subCompanies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
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
        [HttpGet]
        [Route("paginated/user/{userId}/company/{companyId}")]
        public async Task<IActionResult> GetCompaniesByUserIdPaginated(int userId, int companyId,int skip, int take)
        {
            try
            {

                var companies = await _companyService.GetSubCompaniesByUserIdPaginated(userId,companyId ,skip, take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("paginated/user/{userId}/company/{companyId}/deleted")]
        public async Task<IActionResult> GetSubCompaniesDeletedByUserIdPaginated(int userId, int companyId, int skip, int take)
        {
            try
            {

                var companies = await _companyService.GetSubCompaniesDeletedByUserIdPaginated(userId, companyId, skip, take);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
