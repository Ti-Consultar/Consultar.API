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

public class InvitationService : BaseService
{
    private readonly InvitationRepository _invitationRepository;
    private readonly UserRepository _userRepository;
    private readonly CompanyRepository _companyRepository;

    public InvitationService(
        InvitationRepository invitationRepository,
        UserRepository userRepository,
        CompanyRepository companyRepository,
        IAppSettings appSettings) : base(appSettings)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
    }

    public async Task<ResultValue> CreateInvitation(CreateInvitationDto dto)
    {
        try
        {
            var invitedUser = await _userRepository.GetByUserId(dto.UserId);
            var invitingUser = await _userRepository.GetByUserId(dto.InvitedByUserId);
            var company = await _companyRepository.GetById(dto.CompanyId);

            if (invitedUser == null || invitingUser == null || company == null)
                return ErrorResponse(Message.NotFound);

            var invitation = new InvitationToCompany
            {
                UserId = dto.UserId,
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

    public async Task<ResultValue> GetInvitationsByUserId(int userId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var invitations = await _invitationRepository.GetByUserId(userId);
            if (invitations == null || !invitations.Any())
                return ErrorResponse(Message.NotFound);

            var response = invitations.Select(MapToInvitationDetailDto).ToList();

            return SuccessResponse(response.OrderBy(a => a.CreatedAt));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetInvitationsByInvitedById(int userId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var invitations = await _invitationRepository.GetInvitationsByInvitedById(userId);
            if (invitations == null || !invitations.Any())
                return ErrorResponse(Message.NotFound);

            var response = invitations.Select(MapToInvitationDetailDto).ToList();

            return SuccessResponse(response.OrderBy(a => a.CreatedAt));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> UpdateInvitationStatus(int invitationId, UpdateStatus dto)
    {
        try
        {
            var invitation = await _invitationRepository.GetById(invitationId);
            var user = await _userRepository.GetByUserId(dto.UserId);

            if (invitation == null || user == null)
                return ErrorResponse(Message.NotFound);

            if (user.Id != invitation.InvitedById)
                return ErrorResponse(Message.MessageError);

            invitation.Status = dto.Status;
            invitation.UpdatedAt = DateTime.UtcNow;

            if (invitation.SubCompanyId == null)
            {
                var result = await HandleCompanyInvitation(user.Id, invitation);
                if (!result.Success)
                    return result;
            }
            else
            {
                var result = await HandleSubCompanyInvitation(user.Id, invitation);
                if (!result.Success)
                    return result;
            }

            await _invitationRepository.Update(invitation);
            return SuccessResponse(Message.Success);
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

    public async Task<ResultValue> DeleteInvitation(int invitationId, int userId)
    {
        try
        {
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
    private async Task<ResultValue> HandleCompanyInvitation(int userId, InvitationToCompany invitation)
    {
        var exists = await _companyRepository.ExistsCompanyUser(userId, invitation.CompanyId);

        if (exists)
            return ErrorResponse(Message.MessageError);

        await _companyRepository.AddUserToCompany(userId, invitation.CompanyId, invitation.PermissionId);
        return SuccessResponse(Message.Success);
    }

    private async Task<ResultValue> HandleSubCompanyInvitation(int userId, InvitationToCompany invitation)
    {
        var exists = await _companyRepository.ExistsSubCompanyUser(userId, invitation.CompanyId, (int)invitation.SubCompanyId);

        if (exists)
            return ErrorResponse(Message.MessageError);

        await _companyRepository.AddUserToCompanyOrSubCompany(userId, invitation.CompanyId, invitation.SubCompanyId, invitation.PermissionId);
        return SuccessResponse(Message.Success);
    }

    private InvitationDetailDto MapToInvitationDetailDto(InvitationToCompany invitation)
    {
        return new InvitationDetailDto
        {
            Id = invitation.Id,
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
