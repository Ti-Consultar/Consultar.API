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

        [HttpDelete]
        [Route("user/{userId}/company/{id}")]
        public async Task<IActionResult> DeleteCompany(int userId, int id)
        {
            try
            {

                var company = await _companyService.DeleteCompany(userId, id);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetCompaniesByUserId(int userId)
        {
            try
            {
               
                var companies = await _companyService.GetCompaniesByUserId(userId);
                return Ok(companies);  
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });  
            }
        }

        [HttpGet]
        [Route("paginated/user/{userId}")]
        public async Task<IActionResult> GetCompaniesByUserIdPaginated(int userId, int skip, int take)
        {
            try
            {
               
                var companies = await _companyService.GetCompaniesByUserIdPaginated(userId, skip, take);
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
