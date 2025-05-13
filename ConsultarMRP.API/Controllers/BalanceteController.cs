using _4_Application._1_Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceteController : ControllerBase
    {
        private readonly BalanceteService _balanceteService;

        public BalanceteController(BalanceteService balanceteService)
        {
            _balanceteService = balanceteService;
        }
    }
}
