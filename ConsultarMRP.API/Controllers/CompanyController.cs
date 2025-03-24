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

        // Endpoint para criar uma empresa (e possivelmente uma subempresa)
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateCompany([FromBody] InsertCompanyDto createCompanyDto)
        {
            try
            {
                // Chama o serviço para criar a empresa
                var company = await _companyService.CreateCompany(createCompanyDto);
                return Ok(company);  // Retorna a resposta da criação da empresa
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });  // Retorna uma resposta de erro caso ocorra algum problema
            }
        }
        // Endpoint para criar uma empresa (e possivelmente uma subempresa)
        [HttpPost]
        [Route("create/sub-company")]
        public async Task<IActionResult> CreateSubCompany([FromBody] InsertSubCompanyDto createsubCompanyDto)
        {
            try
            {
                // Chama o serviço para criar a empresa
                var company = await _companyService.CreateSubCompany(createsubCompanyDto);
                return Ok(company);  // Retorna a resposta da criação da empresa
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });  // Retorna uma resposta de erro caso ocorra algum problema
            }
        }

        // Endpoint para obter todas as subempresas associadas a um usuário
        [HttpGet]
        [Route("subcompanies/{userId}")]
        public async Task<IActionResult> GetSubCompaniesByUserId(int userId)
        {
            try
            {
                // Chama o serviço para obter as subempresas
                var subCompanies = await _companyService.GetSubCompaniesByUserId(userId);
                return Ok(subCompanies);  // Retorna a lista de subempresas
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });  // Retorna uma resposta de erro
            }
        }

        // Endpoint para obter todas as empresas associadas a um usuário
        [HttpGet]
        [Route("companies/{userId}")]
        public async Task<IActionResult> GetCompaniesByUserId(int userId)
        {
            try
            {
                // Chama o serviço para obter as empresas
                var companies = await _companyService.GetCompaniesByUserId(userId);
                return Ok(companies);  // Retorna a lista de empresas
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });  // Retorna uma resposta de erro
            }
        }
    }
}
