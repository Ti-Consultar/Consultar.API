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
    public class PermissionController : ControllerBase
    {
        private readonly PermissionService _Service;

        public PermissionController(PermissionService service)
        {
            _Service = service;
        }


      

        [HttpGet]
        [Route("permissions")]
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
        [Route("permission/{id}")]
        public async Task<IActionResult> GetCompaniesByUserId(int id)
        {
            try
            {

                var companies = await _Service.GetById(id);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

      
       
    }
}
