using _2___Application._2_Dto_s.Group;
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

        [HttpPut]
        [Route("update/id/{id}/user/{userId}")]
        public async Task<IActionResult> UpdateGroup(int id, int userId, [FromBody] UpdateGroupDto dto)
        {
            try
            {
                var result = await _groupService.UpdateGroup(id,userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch]
        [Route("user/{userId}/group/{id}/delete")]
        public async Task<IActionResult> DeleteGroup(int userId, int id)
        {
            try
            {
                var result = await _groupService.Delete(userId, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch]
        [Route("user/{userId}/group/{id}/restore")]
        public async Task<IActionResult> RestoreGroup(int userId, int id)
        {
            try
            {
                var result = await _groupService.Restore(userId, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route("paginated/user/{userId}")]
        //public async Task<IActionResult> GetGroupsByUserIdPaginated(int userId, int skip, int take)
        //{
        //    try
        //    {
        //        var result = await _groupService.GetGroupsByUserIdPaginated(userId, skip, take);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpGet]
        [Route("id/{id}")]
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
        [HttpGet]
        [Route("id/{id}/users")]
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
        [HttpGet]
        [Route("user/{userId}/groups")]
        public async Task<IActionResult> GetGroupsWithCompaniesByUserId(int userId)
        {
            try
            {
                var result = await _groupService.GetGroupsWithCompaniesByUserId(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("user/{userId}/groups/deleted")]
        public async Task<IActionResult> GetGroupsDeletedWithCompaniesByUserId(int userId)
        {
            try
            {
                var result = await _groupService.GetGroupsDeletedWithCompaniesByUserId(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("user/{userId}/group/{groupId}")]
        public async Task<IActionResult> GetGroupDetailsById(int userId, int groupId)
        {
            try
            {
                var result = await _groupService.GetGroupDetailsById(groupId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
