using _2___Application._2_Dto_s.Group;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class GroupService : BaseService
{
    private readonly GroupRepository _groupRepository;
    private readonly UserRepository _userRepository;

    public GroupService(GroupRepository groupRepository, UserRepository userRepository, IAppSettings appSettings)
        : base(appSettings)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    #region Groups
    public async Task<ResultValue> CreateGroup(InsertGroupDto createGroupDto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(createGroupDto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var group = new GroupModel
            {
                Name = createGroupDto.Name,
                DateCreate = DateTime.Now
            };

            await _groupRepository.Add(group);
            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> UpdateGroup(int id, UpdateGroupDto dto)
    {
        try
        {
            var group = await _groupRepository.GetById(id);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            group.Name = dto.Name;
            await _groupRepository.Update(group);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> DeleteGroup(int id, int userId)
    {
        try
        {
            var group = await _groupRepository.GetById(id);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            await _groupRepository.Delete(id);
            return SuccessResponse(Message.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetGroupById(int id)
    {
        try
        {
            var group = await _groupRepository.GetById(id);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            var response = new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                DateCreate = group.DateCreate
            };

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetAllGroups()
    {
        try
        {
            var groups = await _groupRepository.GetAll();
            var groupDtos = groups.Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                DateCreate = g.DateCreate
            }).ToList();

            return SuccessResponse(groupDtos);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }


    #endregion
}
