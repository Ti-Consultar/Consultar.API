using _2___Application._2_Dto_s.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly GroupService _groupService;

        public GroupController(GroupService groupService)
        {
            _groupService = groupService;
        }

        /// <summary>
        /// Cria um novo grupo.
        /// </summary>
        /// <param name="createGroupDto">Dados para criação do grupo.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateGroup([FromBody] InsertGroupDto createGroupDto)
        {
            try
            {
                var result = await _groupService.CreateGroup(createGroupDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza as informações de um grupo.
        /// </summary>
        /// <param name="id">ID do grupo.</param>
        /// <param name="dto">Dados para atualização.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPut]
        [Route("{id}/update")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupDto dto)
        {
            try
            {
                var result = await _groupService.UpdateGroup(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Exclui um grupo (soft delete).
        /// </summary>
        /// <param name="id">ID do grupo.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPatch]
        [Route("{id}/delete")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                var result = await _groupService.Delete(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Restaura um grupo excluído.
        /// </summary>
        /// <param name="id">ID do grupo.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPatch]
        [Route("{id}/restore")]
        public async Task<IActionResult> RestoreGroup(int id)
        {
            try
            {
                var result = await _groupService.Restore(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém um grupo pelo ID.
        /// </summary>
        /// <param name="id">ID do grupo.</param>
        [Authorize]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            try
            {
                var result = await _groupService.GetGroupById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os usuários de um grupo.
        /// </summary>
        /// <param name="id">ID do grupo.</param>
        [Authorize]
        [HttpGet]
        [Route("{id}/users")]
        public async Task<IActionResult> GetUsersByGroupId(int id)
        {
            try
            {
                var result = await _groupService.GetUsersByGroupId(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos os grupos.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllGroups()
        {
            try
            {
                var result = await _groupService.GetAllGroups();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os grupos associados ao usuário autenticado.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetGroupsWithCompaniesByUserId()
        {
            try
            {
                var result = await _groupService.GetGroupsWithCompaniesByUserId();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os grupos excluídos associados ao usuário autenticado.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("deleted")]
        public async Task<IActionResult> GetGroupsDeletedWithCompaniesByUserId()
        {
            try
            {
                var result = await _groupService.GetGroupsDeletedWithCompaniesByUserId();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os detalhes de um grupo pelo ID.
        /// </summary>
        /// <param name="groupId">ID do grupo.</param>
        [Authorize]
        [HttpGet]
        [Route("detail/{groupId}")]
        public async Task<IActionResult> GetGroupDetailsById(int groupId)
        {
            try
            {
                var result = await _groupService.GetGroupDetailsById(groupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
