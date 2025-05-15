using _2___Application.Base;
using _2___Application._2_Dto_s.Invitation;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _3_Domain._2_Enum_s;
using _4_InfraData._5_ConfigEnum;
using _2___Application._2_Dto_s.Permissions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security;
using _2___Application._2_Dto_s.Group;

public class InvitationService : BaseService
{
    private readonly InvitationRepository _invitationRepository;
    private readonly UserRepository _userRepository;
    private readonly CompanyRepository _companyRepository;
    private readonly GroupRepository _groupRepository;
    private readonly int _currentUserId;

    public InvitationService(
        InvitationRepository invitationRepository,
        UserRepository userRepository,
        CompanyRepository companyRepository,
        GroupRepository groupRepository,
        IAppSettings appSettings) : base(appSettings)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _groupRepository = groupRepository;
        // Obtendo o ID do usuário autenticado 
        _currentUserId = GetCurrentUserId();
    }

    public async Task<ResultValue> CreateInvitation(CreateInvitationDto dto)
    {
        try
        {
            var invitedUser = await _userRepository.GetByUserId(_currentUserId);
            var invitingUser = await _userRepository.GetByUserId(dto.InvitedByUserId);

            var group = await _groupRepository.GetById(dto.GroupId);
            var company = await _companyRepository.GetById(dto.CompanyId);

            if (invitedUser == null || invitingUser == null || group == null)
                return ErrorResponse(Message.NotFound);

            var invitation = new InvitationToCompany
            {
                UserId = invitedUser.Id,
                GroupId = dto.GroupId,
                CompanyId = dto.CompanyId,
                InvitedById = dto.InvitedByUserId,
                PermissionId = dto.PermissionId,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                SubCompanyId = dto.SubCompanyId > 0 ? dto.SubCompanyId : null
            };


            await _invitationRepository.Add(invitation);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetInvitationsByUserId()
    {
        try
        {
            var user = await _userRepository.GetByUserId(_currentUserId);
            if (user == null)
                return SuccessResponse(UserLoginMessage.UserNotFound);

            var invitations = await _invitationRepository.GetByUserId(user.Id);
            if (invitations == null || !invitations.Any())
                return SuccessResponse(new List<InvitationDto>());

            var response = invitations.Select(MapToInvitationDetailDto).ToList();

            return SuccessResponse(response.OrderBy(a => a.CreatedAt));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetInvitationsByInvitedById()
    {
        try
        {
            var user = await _userRepository.GetByUserId(_currentUserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.UserNotFound);

            var invitations = await _invitationRepository.GetInvitationsByInvitedById(user.Id);
            if (invitations == null || !invitations.Any())
                return SuccessResponse(new List<InvitationDto>());

            var response = invitations.Select(MapToInvitationDetailDto).ToList();

            return SuccessResponse(response.OrderBy(a => a.CreatedAt));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> UpdateInvitationStatus(int invitationId, int groupId, UpdateStatus dto)
    {
        try
        {
            var invitation = await _invitationRepository.GetById(invitationId);
            var user = await _userRepository.GetByUserId(_currentUserId);

            if (invitation == null || user == null)
                return ErrorResponse(Message.NotFound);

            if (user.Id != invitation.InvitedById)
                return ErrorResponse(Message.MessageError);

            invitation.Status = dto.Status;
            invitation.UpdatedAt = DateTime.UtcNow;

            var result = await HandleInvitationByContext(user.Id, groupId, invitation);
            if (!result.Success)
                return result;

            await _invitationRepository.Update(invitation);
            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> DeleteCompanyUser( int groupId,int? companyId, int? subCompanyId)
    {
        try
        {
            var userId = _currentUserId;

            var companyUser = await _companyRepository.GetCompanyUser(userId, groupId, companyId, subCompanyId);

            if (companyUser == null )
                return ErrorResponse(Message.NotFound);

            await _companyRepository.DeleteCompanyUser(userId, groupId, companyId, subCompanyId);
            return SuccessResponse(Message.DeletedSuccess);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetInvitationById(int id)
    {
        try
        {
            // Obtém o convite pelo ID com as entidades relacionadas
            var invitation = await _invitationRepository.GetById(id);
                
             

            if (invitation == null)
                return ErrorResponse(Message.NotFound);

            // Mapeia os dados para o response
            var response = new InvitationDetailDto
            {
                Id = invitation.Id,
                Group = invitation.Group != null ? new GroupSimpleDto
                {
                    Id = invitation.Company.Id,
                    Name = invitation.Company.Name
                } : null,
                Company = invitation.Company != null ? new CompanyDto
                {
                    Id = invitation.Company.Id,
                    Name = invitation.Company.Name
                } : null,
                SubCompany = invitation.SubCompany != null ? new SubCompanyDto
                {
                    Id = invitation.SubCompany.Id,
                    Name = invitation.SubCompany.Name
                } : null,
                User = invitation.User != null ? new UserDto
                {
                    Id = invitation.User.Id,
                    Name = invitation.User.Name,
                    Email = invitation.User.Email
                } : null,
                InvitedByUser = invitation.InvitedBy != null ? new UserDto
                {
                    Id = invitation.InvitedBy.Id,
                    Name = invitation.InvitedBy.Name,
                    Email = invitation.InvitedBy.Email
                } : null,
                Permission = invitation.Permission != null ? new PermissionDto
                {
                    Id = invitation.Permission.Id,
                    Name = invitation.Permission.Name
                } : null,
                Status = invitation.Status.GetDescription(),
                CreatedAt = invitation.CreatedAt,
                UpdatedAt = invitation.UpdatedAt
            };

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> DeleteInvitation(int invitationId)
    {
        try
        {
            var userId = _currentUserId;
            var invitation = await _invitationRepository.GetByUserId(invitationId,userId);

            if (invitation == null)
                return ErrorResponse(Message.NotFound);

            if(invitation.Status == InvitationStatus.Pending)
            {
                await _invitationRepository.Delete(invitationId);
            }

            return SuccessResponse(Message.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    #region Métodos Privados
    private async Task<ResultValue> HandleInvitationByContext(int userId, int groupId, InvitationToCompany invitation)
    {
        if (invitation.GroupId != null && invitation.CompanyId == null)
        {
            return await HandleGroupInvitation(userId, invitation);
        }

        if (invitation.SubCompanyId != null)
        {
            return await HandleSubCompanyInvitation(userId, groupId, invitation);
        }

        if (invitation.CompanyId != null)
        {
            return await HandleCompanyInvitation(userId, invitation);
        }

        return ErrorResponse(Message.InvalidInvitationType);
    }
    private async Task<ResultValue> HandleGroupInvitation(int userId, InvitationToCompany invitation)
    {
        var exists = await _companyRepository.ExistsGroupUser(userId, invitation.GroupId);

        if (exists)
            return ErrorResponse(Message.MessageError);

        await _companyRepository.AddUserToGroup(userId, invitation.GroupId);
        return SuccessResponse(Message.Success);
    }
    private async Task<ResultValue> HandleCompanyInvitation(int userId, InvitationToCompany invitation)
    {
        var exists = await _companyRepository.ExistsCompanyUser(userId, (int)invitation.CompanyId);

        if (exists)
            return ErrorResponse(Message.MessageError);

        await _companyRepository.AddUserToCompany(userId, (int)invitation.CompanyId, invitation.PermissionId);
        return SuccessResponse(Message.Success);
    }
    private async Task<ResultValue> HandleSubCompanyInvitation(int userId,int groupId, InvitationToCompany invitation)
    {
        var exists = await _companyRepository.ExistsSubCompanyUser(userId, invitation.CompanyId, (int)invitation.SubCompanyId);

        if (exists)
            return ErrorResponse(Message.MessageError);

        await _companyRepository.AddUserToCompanyOrSubCompany(userId, groupId,invitation.CompanyId, invitation.SubCompanyId, invitation.PermissionId);
        return SuccessResponse(Message.Success);
    }
    private InvitationDetailDto MapToInvitationDetailDto(InvitationToCompany invitation)
    {
        return new InvitationDetailDto
        {
            Id = invitation.Id,
            Group = MapToGroupDto(invitation.Group),
            Company = MapToCompanyDto(invitation.Company),
            SubCompany = MapToSubCompanyDto(invitation.SubCompany),
            User = MapToUserDto(invitation.User),
            InvitedByUser = MapToUserDto(invitation.InvitedBy),
            Permission = MapToPermissionDto(invitation.Permission),
            Status = invitation.Status.GetDescription(),
            CreatedAt = invitation.CreatedAt,
            UpdatedAt = invitation.UpdatedAt
        };
    }
    private GroupSimpleDto MapToGroupDto(GroupModel group)
    {
        return group != null ? new GroupSimpleDto { Id = group.Id, Name = group.Name } : null;
    }
    private CompanyDto MapToCompanyDto(CompanyModel company)
    {
        return company != null ? new CompanyDto { Id = company.Id, Name = company.Name } : null;
    }
    private SubCompanyDto MapToSubCompanyDto(SubCompanyModel subCompany)
    {
        return subCompany != null ? new SubCompanyDto { Id = subCompany.Id, Name = subCompany.Name } : null;
    }
    private UserDto MapToUserDto(UserModel user)
    {
        return user != null ? new UserDto { Id = user.Id, Name = user.Name, Email = user.Email } : null;
    }
    private PermissionDto MapToPermissionDto(PermissionModel permission)
    {
        return permission != null ? new PermissionDto { Id = permission.Id, Name = permission.Name } : null;
    }

    #endregion
}
