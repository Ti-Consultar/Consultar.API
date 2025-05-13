using _2___Application._1_Services.AccountPlans;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountPlansController : ControllerBase
    {
        private readonly AccountPlansService _accountPlansService;

        public AccountPlansController(AccountPlansService accountPlansService)
        {
            _accountPlansService = accountPlansService;
        }
    }
}
