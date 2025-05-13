using _4_Application._1_Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InteractionController : ControllerBase
    {
        private readonly InteractionService _interactionService;

        public InteractionController(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }
    }
}
