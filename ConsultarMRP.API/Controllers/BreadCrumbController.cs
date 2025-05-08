using _2___Application._2_Dto_s.Breadcrumb;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreadcrumbController : ControllerBase
    {
        private readonly BreadcrumbService _breadcrumbService;

        public BreadcrumbController(BreadcrumbService breadcrumbService)
        {
            _breadcrumbService = breadcrumbService;
        }

        [HttpGet]
        [Route("{id}/{type}")]
        public async Task<IActionResult> GetBreadcrumb(int id, string type)
        {
            try
            {
                var result = await _breadcrumbService.GetBreadcrumbAsync(id, type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
