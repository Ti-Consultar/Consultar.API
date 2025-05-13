using _4_Application._1_Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceteDataController : ControllerBase
    {
        private readonly BalanceteDataService _balanceteDataService;

        public BalanceteDataController(BalanceteDataService balanceteDataService)
        {
            _balanceteDataService = balanceteDataService;
        }
    }
}
