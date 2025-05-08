using _2___Application._2_Dto_s.Invitation;
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

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationDto createInvitationDto)
        {
            try
            {
                var result = await _invitationService.CreateInvitation(createInvitationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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

        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetInvitationsByUserId(int userId)
        {
            try
            {
                var result = await _invitationService.GetInvitationsByUserId(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("invited/{userId}")]
        public async Task<IActionResult> GetInvitationsByInvitedById(int userId)
        {
            try
            {
                var result = await _invitationService.GetInvitationsByInvitedById(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPatch]
        [Route("update-status/{id}")]
        public async Task<IActionResult> UpdateInvitationStatus(int id,int groupId, [FromBody] UpdateStatus dto)
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

        [HttpDelete]
        [Route("delete/{id}/user/{UserId}")]
        public async Task<IActionResult> DeleteInvitation(int id, int userId)
        {
            try
            {
                var result = await _invitationService.DeleteInvitation(id, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("companyuser/{userId}/group/{groupId}")]
        public async Task<IActionResult> RemoveCompanyUser( int userId,int groupId,[FromQuery] int? companyId,[FromQuery] int? subCompanyId)
        {
            var result = await _invitationService.DeleteCompanyUser(userId, groupId, companyId, subCompanyId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
