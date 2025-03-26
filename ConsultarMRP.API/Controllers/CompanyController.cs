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
       
        [HttpPost]
        [Route("create/sub-company")]
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

        [HttpGet]
        [Route("subcompanies/{userId}")]
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
        [Route("companies/user/{userId}")]
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
        [Route("companies-paginated/user/{userId}")]
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
    }
}
