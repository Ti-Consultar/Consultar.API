﻿using _2___Application._2_Dto_s.Group;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _2___Application._2_Dto_s.Permissions;
using _2___Application._2_Dto_s.BusinesEntity;

public class GroupService : BaseService
{
    private readonly GroupRepository _groupRepository;
    private readonly UserRepository _userRepository;
    private readonly CompanyRepository _companyRepository;
    private readonly BusinessEntityRepository _businessEntityRepository;

    public GroupService(GroupRepository groupRepository, UserRepository userRepository, CompanyRepository companyRepository, BusinessEntityRepository businessEntityRepository, IAppSettings appSettings)
        : base(appSettings)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _businessEntityRepository = businessEntityRepository;
    }

    #region Groups

    public async Task<ResultValue> CreateGroup(InsertGroupDto dto)
    {
        try
        {
            var user = await _userRepository.GetById(dto.UserId);
            if (user == null) return ErrorResponse(UserLoginMessage.InvalidCredentials);

            if (await _businessEntityRepository.CnpjExists(dto.BusinessEntity.Cnpj))
                return SuccessResponse("Já existe um cadastro com este CNPJ.");

            var businessEntity = MapToBusinessEntity(dto.BusinessEntity);
            await _businessEntityRepository.AddAsync(businessEntity);

            var group = new GroupModel
            {
                Name = dto.Name,
                DateCreate = DateTime.Now,
                BusinessEntityId = businessEntity.Id
            };

            await _groupRepository.Add(group);

            await _companyRepository.AddUserToGroup(dto.UserId, group.Id);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> UpdateGroup(int id, int userId, UpdateGroupDto dto)
    {
        try
        {
            if (!await _groupRepository.UserHasManagerPermissionInGroup(userId, id))
                return SuccessResponse("Você não tem permissão para editar este grupo.");

            var group = await _groupRepository.GetById(id);
            if (group == null) return ErrorResponse(Message.NotFound);

            if (!string.IsNullOrWhiteSpace(dto.Name))
                group.Name = dto.Name;

            if (await _businessEntityRepository.CnpjExists(dto.BusinessEntity.Cnpj))
                return SuccessResponse("Já existe um cadastro com este CNPJ.");

            if (group.BusinessEntity != null && dto.BusinessEntity != null)
                UpdateBusinessEntityFieldsIfPresent(group.BusinessEntity, dto.BusinessEntity);

            await _groupRepository.Update(group);

            return SuccessResponse(Message.Success);
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

            var result = groups.Select(MapToGroupDto).ToList();

            return SuccessResponse(result);
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
            if (group == null) return ErrorResponse(Message.NotFound);

            return SuccessResponse(MapToGroupDto(group));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetGroupsWithCompaniesByUserId(int userId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null) return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var groups = await _groupRepository.GetGroupsByUserId(userId);
            if (groups == null || !groups.Any()) return ErrorResponse(Message.NotFound);

            var result = groups.Select(g => MapToGroupWithCompaniesDto(g, userId, user.Name)).ToList();

            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetGroupDetailsById(int groupId, int userId)
    {
        try
        {
            var group = await _groupRepository.GetGroupWithCompaniesById(groupId, userId);
            if (group == null) return ErrorResponse(Message.NotFound);

            if (!group.CompanyUsers.Any(cu => cu.UserId == userId))
                return ErrorResponse(UserLoginMessage.Error);

            var userName = group.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.User?.Name ?? string.Empty;

            return SuccessResponse(MapToGroupWithCompaniesDto(group, userId, userName));
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> Delete(int userId, int groupId)
    {
        try
        {
            if (!await _groupRepository.UserHasManagerPermissionInGroup(userId, groupId))
                return ErrorResponse("Você não tem permissão para excluir este grupo.");

            var group = await _groupRepository.GetById(groupId);
            if (group == null) return ErrorResponse(Message.NotFound);

            if (group.BusinessEntityId != 0)
            {
                var entity = await _businessEntityRepository.GetById(group.BusinessEntityId);
                if (entity != null)
                    await _businessEntityRepository.Delete(entity.Id);
            }

            await _groupRepository.Delete(group.Id);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    #endregion

    #region Helpers

    private static BusinessEntity MapToBusinessEntity(InsertBusinessEntityDto dto) => new()
    {
        NomeFantasia = dto.NomeFantasia,
        RazaoSocial = dto.RazaoSocial,
        Cnpj = dto.Cnpj,
        Logradouro = dto.Logradouro,
        Numero = dto.Numero,
        Bairro = dto.Bairro,
        Municipio = dto.Municipio,
        Uf = dto.Uf,
        Cep = dto.Cep,
        Telefone = dto.Telefone,
        Email = dto.Email
    };

    private static GroupDto MapToGroupDto(GroupModel group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        DateCreate = group.DateCreate,
        BusinessEntity = group.BusinessEntity == null ? null : MapToBusinessEntityDto(group.BusinessEntity)
    };

    private static GroupWithCompaniesDto MapToGroupWithCompaniesDto(GroupModel group, int userId, string userName) => new()
    {
        GroupId = group.Id,
        GroupName = group.Name,
        DateCreate = group.DateCreate,
        UserId = userId,
        UserName = userName,
        GroupPermission = MapPermission(group.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission),
        Companies = group.Companies?
            .Where(c => c.CompanyUsers.Any(cu => cu.UserId == userId))
            .Select(c => MapToCompanyUserSimpleDto(c, userId))
            .ToList() ?? new List<CompanyUsersimpleDto>(),
        BusinessEntity = group.BusinessEntity == null ? null : MapToBusinessEntityDto(group.BusinessEntity)
    };

    private static CompanyUsersimpleDto MapToCompanyUserSimpleDto(CompanyModel company, int userId) => new()
    {
        CompanyId = company.Id,
        CompanyName = company.Name,
        DateCreate = company.DateCreate,
        Permission = MapPermission(company.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission),
        BusinessEntity = company.BusinessEntity == null ? null : MapToBusinessEntityDto(company.BusinessEntity),
        SubCompanies = company.SubCompanies?
            .Where(sc => sc.CompanyUsers.Any(cu => cu.UserId == userId))
            .Select(sc => new SubCompanyUsersimpleDto
            {
                SubCompanyId = sc.Id,
                SubCompanyName = sc.Name,
                CompanyId = company.Id,
                DateCreate = sc.DateCreate,
                Permission = MapPermission(sc.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission)
            }).ToList()
    };

    private static BusinessEntityDto MapToBusinessEntityDto(BusinessEntity entity) => new()
    {
        Id = entity.Id,
        NomeFantasia = entity.NomeFantasia,
        RazaoSocial = entity.RazaoSocial,
        Cnpj = entity.Cnpj,
        Logradouro = entity.Logradouro,
        Numero = entity.Numero,
        Bairro = entity.Bairro,
        Municipio = entity.Municipio,
        Uf = entity.Uf,
        Cep = entity.Cep,
        Telefone = entity.Telefone,
        Email = entity.Email
    };

    private static PermissionResponse MapPermission(PermissionModel permission)
    {
        if (permission == null) return null;
        return new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name
        };
    }

    private void UpdateBusinessEntityFieldsIfPresent(BusinessEntity entity, BusinessEntityDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.NomeFantasia)) entity.NomeFantasia = dto.NomeFantasia;
        if (!string.IsNullOrWhiteSpace(dto.RazaoSocial)) entity.RazaoSocial = dto.RazaoSocial;
        if (!string.IsNullOrWhiteSpace(dto.Cnpj)) entity.Cnpj = dto.Cnpj;
        if (!string.IsNullOrWhiteSpace(dto.Logradouro)) entity.Logradouro = dto.Logradouro;
        if (!string.IsNullOrWhiteSpace(dto.Numero)) entity.Numero = dto.Numero;
        if (!string.IsNullOrWhiteSpace(dto.Bairro)) entity.Bairro = dto.Bairro;
        if (!string.IsNullOrWhiteSpace(dto.Municipio)) entity.Municipio = dto.Municipio;
        if (!string.IsNullOrWhiteSpace(dto.Uf)) entity.Uf = dto.Uf;
        if (!string.IsNullOrWhiteSpace(dto.Cep)) entity.Cep = dto.Cep;
        if (!string.IsNullOrWhiteSpace(dto.Telefone)) entity.Telefone = dto.Telefone;
        if (!string.IsNullOrWhiteSpace(dto.Email)) entity.Email = dto.Email;
    }

    #endregion
}
