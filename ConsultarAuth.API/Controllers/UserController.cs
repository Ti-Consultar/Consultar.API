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

        /// <summary>
        /// Realiza o login de um usuário e retorna o token de autenticação.
        /// </summary>
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

        /// <summary>
        /// Registra um novo usuário no sistema.
        /// </summary>
        [HttpPost("/register")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> InsertUser([FromBody] InsertDto request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.InsertUser(request);

            return Ok(user);
        }

        /// <summary>
        /// Registra um novo usuário no sistema.
        /// </summary>
        [HttpPost("/register-fake")]
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        public async Task<IActionResult> InsertUser([FromBody] InsertSimpleDto request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.InsertSimpleUser(request);

            return Ok(user);
        }

        /// <summary>
        /// Redefine a senha do usuário com base no e-mail informado e envia uma nova senha por e-mail.
        /// </summary>
        [HttpPut("redefine-password")]
        public async Task<IActionResult> RedefinePassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.RedefinePassword(request);

            return Ok(user);
        }

        /// <summary>
        /// Atualiza a senha do usuário autenticado.
        /// </summary>
        [HttpPut("/reset-password")]
        [Authorize()]
        public async Task<IActionResult> ResetPassword([FromBody] UpdatePasswordDto request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.ResetPassword(request);

            return Ok(user);
        }

        /// <summary>
        /// Atualiza a permissão do usuário desde que um Gestor altere.
        /// </summary>
        [HttpPut("/permission")]
        [Authorize()]
        public async Task<IActionResult> UpdateRoleUser([FromBody] UpdateUserByGestor request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.UpdateRoleUser(request);

            return Ok(user);
        }

        /// <summary>
        /// Atualiza os dados do usuário autenticado.
        /// </summary>
        [HttpPut()]
        [Authorize()]
        public async Task<IActionResult> Update([FromBody] UpdateUser request)
        {
            if (request == null)
            {
                return NotFound(new { message = "Dados inválidos" });
            }

            var user = await _userService.UpdateUser(request);

            return Ok(user);
        }


        /// <summary>
        /// lista Usuário por Email.
        /// </summary>
        [HttpGet("/find/{find}")]
        [Authorize()]
        public async Task<IActionResult> GetUserByEmailOrContact(string find)
        {
            try
            {
                var user = await _userService.GetUserByEmailOrContact(find);

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

        /// <summary>
        /// Retorna os dados completos de um usuário, incluindo grupos, empresas e filiais.
        /// </summary>
        [HttpGet("")]
        [Authorize()]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var user = await _userService.GetUser();

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

        ///// <summary>
        ///// Retorna todos os usuários do sistema.
        ///// </summary>
        //[HttpGet("/user")]
        //[Authorize()]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    try
        //    {
        //        var user = await _userService.GetAllUsers();

        //        if (user == null)
        //        {
        //            return NotFound(new { message = "Usuário não encontrado" });
        //        }

        //        return Ok(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Erro ao buscar usuários", details = ex.Message });
        //    }
        //}

        /// <summary>
        /// Retorna os dados básicos de um usuário específico.
        /// </summary>
        [HttpGet("/simple")]
        [Authorize()]
        public async Task<IActionResult> GetUserbyId()
        {
            try
            {
                var user = await _userService.GetById();

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

        /// <summary>
        /// Retorna todas as policies de autorização disponíveis no sistema.
        /// </summary>
        [HttpGet("/policies")]
    
        public async Task<IActionResult> GetPolicies([FromServices] AuthorizationService authorizationService)
        {
            var policies = await authorizationService.GetAllPoliciesAsync();
            return Ok(policies);
        }
    }
}
