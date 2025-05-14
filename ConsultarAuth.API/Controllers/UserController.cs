using _2___Application._1_Services.User;
using _2___Application._2_Dto_s.UserDto.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultarAuth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var userResponse = await _userService.Login(request);

            if (userResponse == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(userResponse);
        }

        [HttpPost("/register")]
        public async Task<IActionResult> InsertUser([FromBody] InsertDto request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.InsertUser(request);

            return Ok(user);
        }

        [HttpPut("redefine-password")]
        public async Task<IActionResult> RedefinePassword([FromBody] _2___Application._2_Dto_s.UserDto.Request.ResetPasswordRequest request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.RedefinePassword(request);

            return Ok(user);
        }

        [HttpPut("/{userId}/reset-password")]
        [Authorize()]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] UpdatePasswordDto request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.ResetPassword(userId, request);

            return Ok(user);
        }

        // Novo método para buscar usuário e suas empresas/subempresas
        [HttpGet("/{userId}")]
        //[Authorize()]
        public async Task<IActionResult> GetUser(int userId)
        {
            try
            {
                var user = await _userService.GetUser(userId);

                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar usuário", details = ex.Message });
            }
        }

        [HttpGet("")]
       // [Authorize()]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var user = await _userService.GetAllUsers();

                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar usuário", details = ex.Message });
            }
        }

        [HttpGet("/{id}/simple")]
        // [Authorize()]
        public async Task<IActionResult> GetUserbyId(int id)
        {
            try
            {
                var user = await _userService.GetById(id);

                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar usuário", details = ex.Message });
            }
        }
       
        [HttpGet("/policies")]
        
        public async Task<IActionResult> GetPolicies([FromServices] AuthorizationService authorizationService)
        {
            var policies = await authorizationService.GetAllPoliciesAsync();
            return Ok(policies);
        }
    }
}
