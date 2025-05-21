using _2___Application._2_Dto_s.Invitation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace _5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly InvitationService _invitationService;

        public InvitationController(InvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        /// <summary>
        /// Cria um novo convite.
        /// </summary>
        /// <param name="createInvitationDto">Dados para criação do convite.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationBatchDto createInvitationDto)
        {
            try
            {
                var result = await _invitationService.CreateInvitationBatch(createInvitationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém um convite pelo ID.
        /// </summary>
        /// <param name="id">ID do convite.</param>
        [Authorize]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetInvitationById(int id)
        {
            try
            {
                var result = await _invitationService.GetInvitationById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os convites enviados pelo usuário autenticado.
        /// </summary>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpGet]
        [Route("sent")]
        public async Task<IActionResult> GetInvitationsByUserId()
        {
            try
            {
                var result = await _invitationService.GetInvitationsByUserId();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os convites recebidos pelo usuário autenticado.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("received")]
        public async Task<IActionResult> GetInvitationsByInvitedById()
        {
            try
            {
                var result = await _invitationService.GetInvitationsByInvitedById();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza o status de um convite.
        /// </summary>
        /// <param name="id">ID do convite.</param>
        /// <param name="groupId">ID do grupo associado.</param>
        /// <param name="dto">Dados de atualização do status.</param>
        [Authorize]
        [HttpPatch]
        [Route("{id}/update-status")]
        public async Task<IActionResult> UpdateInvitationStatus(int id, int groupId, [FromBody] UpdateStatus dto)
        {
            try
            {
                var result = await _invitationService.UpdateInvitationStatus(id, groupId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Exclui um convite.
        /// </summary>
        /// <param name="id">ID do convite.</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpDelete]
        [Route("{id}/delete")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            try
            {
                var result = await _invitationService.DeleteInvitation(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remove um usuário da empresa ou subempresa.
        /// </summary>
        /// <param name="groupId">ID do grupo.</param>
        /// <param name="companyId">ID da empresa (opcional).</param>
        /// <param name="subCompanyId">ID da subempresa (opcional).</param>
        [Authorize(Roles = "Gestor,Admin,Consultor,Desenvolvedor")]
        [HttpDelete]
        [Route("group/{groupId}/company-user")]
        public async Task<IActionResult> RemoveCompanyUser(int groupId, [FromQuery] int? companyId, [FromQuery] int? subCompanyId)
        {
            try
            {
                var result = await _invitationService.DeleteCompanyUser(groupId, companyId, subCompanyId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
