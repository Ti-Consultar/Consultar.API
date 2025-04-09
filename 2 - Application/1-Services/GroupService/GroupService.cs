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
using _2___Application._2_Dto_s.Permissions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
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
    public async Task<ResultValue> CreateGroup(InsertGroupDto createGroupDto)
    {
        try
        {
            var user = await _userRepository.GetById(createGroupDto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Cria a entidade empresarial
            var businessEntity = new BusinessEntity
            {
                NomeFantasia = createGroupDto.BusinessEntity.NomeFantasia,
                RazaoSocial = createGroupDto.BusinessEntity.RazaoSocial,
                Cnpj = createGroupDto.BusinessEntity.Cnpj,
                Logradouro = createGroupDto.BusinessEntity.Logradouro,
                Numero = createGroupDto.BusinessEntity.Numero,
                Bairro = createGroupDto.BusinessEntity.Bairro,
                Municipio = createGroupDto.BusinessEntity.Municipio,
                Uf = createGroupDto.BusinessEntity.Uf,
                Cep = createGroupDto.BusinessEntity.Cep,
                Telefone = createGroupDto.BusinessEntity.Telefone,
                Email = createGroupDto.BusinessEntity.Email
            };

            await _businessEntityRepository.AddAsync(businessEntity);


            var group = new GroupModel
            {
                Name = createGroupDto.Name,
                DateCreate = DateTime.Now,
                BusinessEntityId = businessEntity.Id
            };

            await _groupRepository.Add(group);


            var companyUser = new CompanyUserModel
            {
                UserId = createGroupDto.UserId,
                GroupId = group.Id
            };

            await _companyRepository.AddUserToGroup(companyUser.UserId, companyUser.GroupId);

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
                DateCreate = group.DateCreate,
                BusinessEntity = group.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = group.BusinessEntity.Id,
                    NomeFantasia = group.BusinessEntity.NomeFantasia,
                    RazaoSocial = group.BusinessEntity.RazaoSocial,
                    Cnpj = group.BusinessEntity.Cnpj,
                    Logradouro = group.BusinessEntity.Logradouro,
                    Numero = group.BusinessEntity.Numero,
                    Bairro = group.BusinessEntity.Bairro,
                    Municipio = group.BusinessEntity.Municipio,
                    Uf = group.BusinessEntity.Uf,
                    Cep = group.BusinessEntity.Cep,
                    Telefone = group.BusinessEntity.Telefone,
                    Email = group.BusinessEntity.Email
                }
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
                DateCreate = g.DateCreate,
                BusinessEntity = g.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = g.BusinessEntity.Id,
                    NomeFantasia = g.BusinessEntity.NomeFantasia,
                    RazaoSocial = g.BusinessEntity.RazaoSocial,
                    Cnpj = g.BusinessEntity.Cnpj,
                    Logradouro = g.BusinessEntity.Logradouro,
                    Numero = g.BusinessEntity.Numero,
                    Bairro = g.BusinessEntity.Bairro,
                    Municipio = g.BusinessEntity.Municipio,
                    Uf = g.BusinessEntity.Uf,
                    Cep = g.BusinessEntity.Cep,
                    Telefone = g.BusinessEntity.Telefone,
                    Email = g.BusinessEntity.Email
                }
            }).ToList();

            return SuccessResponse(groupDtos);
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
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var groups = await _groupRepository.GetGroupsByUserId(userId);
            if (groups == null || !groups.Any())
                return ErrorResponse(Message.NotFound);

            var groupDtos = groups.Select(group =>
            {
                var companies = group.Companies?
                    .Where(c => c.CompanyUsers.Any(cu => cu.UserId == userId))
                    .ToList();

                var companyDtos = companies?.Select(company => new CompanyUsersimpleDto
                {
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    DateCreate = company.DateCreate,
                    Permission = company.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission != null
                        ? new PermissionResponse
                        {
                            Id = company.CompanyUsers.First(cu => cu.UserId == userId).Permission.Id,
                            Name = company.CompanyUsers.First(cu => cu.UserId == userId).Permission.Name
                        }
                        : null,

                    SubCompanies = company.SubCompanies?
                        .Where(sc => sc.CompanyUsers.Any(cu => cu.UserId == userId))
                        .Select(subCompany => new SubCompanyUsersimpleDto
                        {
                            SubCompanyId = subCompany.Id,
                            SubCompanyName = subCompany.Name,
                            CompanyId = company.Id,
                            DateCreate = subCompany.DateCreate,
                            Permission = subCompany.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission != null
                                ? new PermissionResponse
                                {
                                    Id = subCompany.CompanyUsers.First(cu => cu.UserId == userId).Permission.Id,
                                    Name = subCompany.CompanyUsers.First(cu => cu.UserId == userId).Permission.Name
                                }
                                : null
                        }).ToList()
                }).ToList();

                return new GroupWithCompaniesDto
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    DateCreate = group.DateCreate,
                    UserId = userId,
                    UserName = user.Name,
                    Companies = companyDtos ?? new List<CompanyUsersimpleDto>(),

                    BusinessEntity = group.BusinessEntity == null ? null : new BusinessEntityDto
                    {
                        Id = group.BusinessEntity.Id,
                        NomeFantasia = group.BusinessEntity.NomeFantasia,
                        RazaoSocial = group.BusinessEntity.RazaoSocial,
                        Cnpj = group.BusinessEntity.Cnpj,
                        Logradouro = group.BusinessEntity.Logradouro,
                        Numero = group.BusinessEntity.Numero,
                        Bairro = group.BusinessEntity.Bairro,
                        Municipio = group.BusinessEntity.Municipio,
                        Uf = group.BusinessEntity.Uf,
                        Cep = group.BusinessEntity.Cep,
                        Telefone = group.BusinessEntity.Telefone,
                        Email = group.BusinessEntity.Email
                    }
                };
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
