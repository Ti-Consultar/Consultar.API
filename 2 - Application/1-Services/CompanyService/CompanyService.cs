﻿using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using _2___Application._2_Dto_s.Permissions;
using _2___Application._2_Dto_s.Company.CompanyUser;
using Microsoft.EntityFrameworkCore;
using _2___Application._2_Dto_s.Group;
using _2___Application._2_Dto_s.CNPJ;
using _2___Application._2_Dto_s.BusinesEntity;
using _4_InfraData._3_Utils.Email;
using _2___Application._2_Dto_s.Invitation;
using _2___Application._2_Dto_s.Users;


public class CompanyService : BaseService
{
    private readonly CompanyRepository _companyRepository;
    private readonly UserRepository _userRepository;
    private readonly BusinessEntityRepository _businessEntityRepository;
    private readonly GroupRepository _groupRepository;
    private readonly EmailService _emailService;

    public CompanyService(CompanyRepository companyRepository, UserRepository userRepository, BusinessEntityRepository businessEntityRepository, GroupRepository groupRepository, EmailService emailService,IAppSettings appSettings)
        : base(appSettings)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _businessEntityRepository = businessEntityRepository;
        _groupRepository = groupRepository;
        _emailService = emailService;
    }

    #region Companies
    public async Task<ResultValue> CreateCompany(InsertCompanyDto createCompanyDto)
    {
        try
        {
            var user = await _userRepository.GetById(createCompanyDto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Verifica se o CNPJ já existe
            var cnpjExists = await _businessEntityRepository.CnpjExists(createCompanyDto.BusinessEntity.Cnpj);
            if (cnpjExists)
            {
                return SuccessResponse("Já existe um cadastro com este CNPJ.");
            }

            // Cria a entidade empresarial
            var businessEntity = new BusinessEntity
            {
                NomeFantasia = createCompanyDto.BusinessEntity.NomeFantasia,
                RazaoSocial = createCompanyDto.BusinessEntity.RazaoSocial,
                Cnpj = createCompanyDto.BusinessEntity.Cnpj,
                Logradouro = createCompanyDto.BusinessEntity.Logradouro,
                Numero = createCompanyDto.BusinessEntity.Numero,
                Bairro = createCompanyDto.BusinessEntity.Bairro,
                Municipio = createCompanyDto.BusinessEntity.Municipio,
                Uf = createCompanyDto.BusinessEntity.Uf,
                Cep = createCompanyDto.BusinessEntity.Cep,
                Telefone = createCompanyDto.BusinessEntity.Telefone,
                Email = createCompanyDto.BusinessEntity.Email
            };

            await _businessEntityRepository.AddAsync(businessEntity);

            // Cria a empresa
            var company = new CompanyModel
            {
                Name = createCompanyDto.Name,
                DateCreate = DateTime.Now,
                GroupId = createCompanyDto.GroupId,
                BusinessEntityId = businessEntity.Id
            };

            await _companyRepository.AddCompany(company);

            // Vincula o usuário à empresa
            var companyUser = new CompanyUserModel
            {
                UserId = createCompanyDto.UserId,
                CompanyId = company.Id,
                GroupId = createCompanyDto.GroupId
            };

            await _companyRepository.AddUserToCompany(companyUser.UserId, companyUser.CompanyId, companyUser.GroupId);
            await _emailService.SendWelcomeAsync(user.Email, company.Name, user.Name);
            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> UpdateCompany(int id, UpdateCompanyDto dto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(dto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var hasPermission = await _companyRepository.ExistsEditCompanyUser(dto.UserId, id, dto.GroupId);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            var company = await _companyRepository.GetCompanyByUserId(id, dto.UserId, dto.GroupId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            // Atualiza os dados da empresa
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                company.Name = dto.Name;
                company.BusinessEntity.NomeFantasia = dto.Name; // Ou mantenha separado se quiser lógica diferente
            }

            if (company.BusinessEntity != null && dto.BusinessEntity != null)
                UpdateBusinessEntityFieldsIfPresent(company.BusinessEntity, dto.BusinessEntity);

            await _companyRepository.UpdateCompany(company);
            await _businessEntityRepository.Update(company.BusinessEntity);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
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
    public async Task<ResultValue> DeleteCompany(int userId, int id, int groupId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Verifica se o usuário tem permissão para excluir a empresa
            var hasPermission = await _companyRepository.ExistsCompanyUser(userId, id, groupId);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            var company = await _companyRepository.GetCompanyByUserId(id, userId, groupId);
            if (company == null)
                return SuccessResponse(new List<ResultValue>());

            await _companyRepository.DeleteCompany(company.Id);

            return SuccessResponse(Message.DeletedSuccess);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> RestoreCompanies(int userId, List<int> companyIds, int groupId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var notAuthorizedIds = new List<int>();
            var notFoundIds = new List<int>();
            var restoredIds = new List<int>();

            foreach (var companyId in companyIds)
            {
                var hasPermission = await _companyRepository.ExistsCompanyUser(userId, companyId, groupId);
                if (!hasPermission)
                {
                    notAuthorizedIds.Add(companyId);
                    continue;
                }

                var company = await _companyRepository.GetById(companyId);
                if (company == null)
                {
                    notFoundIds.Add(companyId);
                    continue;
                }

                await _companyRepository.RestoreCompany(company.Id);
                restoredIds.Add(company.Id);
            }

            return SuccessResponse(new
            {
                Restored = restoredIds,
                NotFound = notFoundIds,
                Unauthorized = notAuthorizedIds,
                Message = "Empresas restauradas com sucesso"
            });
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetCompanyById(int companyId, int userId, int groupId)
    {
        try
        {
            var company = await _companyRepository.GetCompanyById(userId, groupId, companyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            // Buscar a CompanyUser para pegar a Permission
            var companyUser = company.CompanyUsers?
                .FirstOrDefault(cu => cu.UserId == userId && cu.GroupId == groupId);

            var response = new _2___Application._2_Dto_s.Company.CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                DateCreate = company.DateCreate,
                BusinessEntity = company.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = company.BusinessEntity.Id,
                    NomeFantasia = company.BusinessEntity.NomeFantasia,
                    RazaoSocial = company.BusinessEntity.RazaoSocial,
                    Cnpj = company.BusinessEntity.Cnpj,
                    Logradouro = company.BusinessEntity.Logradouro,
                    Numero = company.BusinessEntity.Numero,
                    Bairro = company.BusinessEntity.Bairro,
                    Municipio = company.BusinessEntity.Municipio,
                    Uf = company.BusinessEntity.Uf,
                    Cep = company.BusinessEntity.Cep,
                    Telefone = company.BusinessEntity.Telefone,
                    Email = company.BusinessEntity.Email
                },
                Permission = companyUser?.Permission != null
                    ? new PermissionResponse
                    {
                        Id = companyUser.Permission.Id,
                        Name = companyUser.Permission.Name
                    }
                    : null
            };

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> CreateUserCompany(CreateCompanyUserDto dto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(dto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var company = await _companyRepository.GetById(dto.CompanyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            var model = new CompanyUserModel();

            model.CompanyId = dto.CompanyId;
            model.UserId = user.Id;
            model.PermissionId = dto.PermissionId;
            model.GroupId = dto.GroupId;

            await _companyRepository.AddUserToCompany(model.UserId, model.CompanyId, model.PermissionId);



            return SuccessResponse(Message.Success);
        }
        catch (DbUpdateException ex)
        {
            var innerException = ex.InnerException?.Message ?? "No inner exception";
            throw new Exception($"Erro ao salvar no banco. Detalhes: {innerException}");
        }
    }

    public async Task<ResultValue> GetUsersByCompanyId(int groupId, int companyId)
    {
        try
        {
            var users = await _groupRepository.GetUsersByCompanyId(groupId, companyId);
            if (users == null) return ErrorResponse(Message.NotFound);

            var response = new List<UserListDto>();
            response.AddRange(users.Select(u => new UserListDto
            {
                Id = u.Id,
                Name = u.User.Name,
                Email = u.User.Email,
                Contact = u.User.Name,
                Permission = new PermissionDto
                {
                    Id = u.Permission.Id,
                    Name = u.Permission.Name
                }
            }));

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }


    #endregion

    #region SubCompanies
    public async Task<ResultValue> CreateSubCompany(InsertSubCompanyDto createSubCompanyDto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(createSubCompanyDto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var company = await _companyRepository.GetById(createSubCompanyDto.CompanyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            // Verifica se o CNPJ já está cadastrado
            var cnpjExists = await _businessEntityRepository.CnpjExists(createSubCompanyDto.BusinessEntity.Cnpj);
            if (cnpjExists)
                return SuccessResponse("Já existe um cadastro com este CNPJ.");

            // Cria a entidade empresarial
            var businessEntity = new BusinessEntity
            {
                NomeFantasia = createSubCompanyDto.BusinessEntity.NomeFantasia,
                RazaoSocial = createSubCompanyDto.BusinessEntity.RazaoSocial,
                Cnpj = createSubCompanyDto.BusinessEntity.Cnpj,
                Logradouro = createSubCompanyDto.BusinessEntity.Logradouro,
                Numero = createSubCompanyDto.BusinessEntity.Numero,
                Bairro = createSubCompanyDto.BusinessEntity.Bairro,
                Municipio = createSubCompanyDto.BusinessEntity.Municipio,
                Uf = createSubCompanyDto.BusinessEntity.Uf,
                Cep = createSubCompanyDto.BusinessEntity.Cep,
                Telefone = createSubCompanyDto.BusinessEntity.Telefone,
                Email = createSubCompanyDto.BusinessEntity.Email
            };

            await _businessEntityRepository.AddAsync(businessEntity);

            // Cria a SubCompany com a BusinessEntity associada
            var subCompany = new SubCompanyModel
            {
                Name = createSubCompanyDto.Name,
                CompanyId = createSubCompanyDto.CompanyId,
                DateCreate = DateTime.Now,
                BusinessEntityId = businessEntity.Id
            };

            await _companyRepository.AddSubCompany(subCompany.CompanyId, subCompany);

            // Vincula o usuário à SubCompany
            var companyUser = new CompanyUserModel
            {
                UserId = createSubCompanyDto.UserId,
                CompanyId = company.Id,
                GroupId = company.GroupId,
                PermissionId = 1 // gestor
            };

            await _companyRepository.AddUserToCompanyOrSubCompany(
                companyUser.UserId,
                company.GroupId,
                companyUser.CompanyId,
                subCompany.Id,
                companyUser.PermissionId
            );

            await _emailService.SendWelcomeSubCompanyAsync(user.Email,company.Name ,subCompany.Name, user.Name);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> UpdateSubCompany(int id, UpdateSubCompanyDto dto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(dto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Verifica se o usuário tem permissão para editar a subempresa
            var hasPermission = await _companyRepository.ExistsEditSubCompanyUser(dto.UserId, dto.CompanyId, id);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            // Obtém a subempresa diretamente pelo ID e pelo usuário
            var subCompany = await _companyRepository.GetSubCompanyByUserId(id, dto.UserId);
            if (subCompany == null)
                return ErrorResponse(Message.NotFound);

            // Atualiza os dados da SubCompany
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                subCompany.Name = dto.Name;
                if (subCompany.BusinessEntity != null)
                    subCompany.BusinessEntity.NomeFantasia = dto.Name;
            }

            // Atualiza os campos da BusinessEntity, se houver
            if (subCompany.BusinessEntity != null && dto.BusinessEntity != null)
                UpdateBusinessEntityFieldsIfPresent(subCompany.BusinessEntity, dto.BusinessEntity);

            await _companyRepository.UpdateSubCompany(subCompany);
            await _businessEntityRepository.Update(subCompany.BusinessEntity);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> DeleteSubCompany(int userId, int companyId, int subCompanyId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Verifica se o usuário tem permissão para excluir a subempresa
            var hasPermission = await _companyRepository.ExistsEditSubCompanyUser(userId, companyId, subCompanyId);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            var company = await _companyRepository.GetById(companyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            var subCompany = company.SubCompanies.FirstOrDefault(a => a.Id == subCompanyId);
            if (subCompany == null)
                return SuccessResponse(new List<ResultValue>());

            await _companyRepository.DeleteSubCompany(company.Id, subCompany.Id);

            return SuccessResponse(Message.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> RestoreSubCompanies(int userId, int companyId, List<int> subCompanyIds)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var company = await _companyRepository.GetById(companyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            var restoredIds = new List<int>();
            var notFoundIds = new List<int>();
            var unauthorizedIds = new List<int>();

            foreach (var subCompanyId in subCompanyIds)
            {
                var hasPermission = await _companyRepository.ExistsEditSubCompanyUser(userId, companyId, subCompanyId);
                if (!hasPermission)
                {
                    unauthorizedIds.Add(subCompanyId);
                    continue;
                }

                var subCompany = company.SubCompanies.FirstOrDefault(a => a.Id == subCompanyId);
                if (subCompany == null)
                {
                    notFoundIds.Add(subCompanyId);
                    continue;
                }

                await _companyRepository.RestoreSubCompany(company.Id, subCompany.Id);
                restoredIds.Add(subCompany.Id);
            }

            return SuccessResponse(new
            {
                Restored = restoredIds,
                NotFound = notFoundIds,
                Unauthorized = unauthorizedIds,
                Message = "Subempresas restauradas com sucesso"
            });
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> CreateUserSubCompany(CreateSubCompanyUserDto dto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(dto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var company = await _companyRepository.GetById(dto.CompanyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            var model = new CompanyUserModel();

            model.CompanyId = dto.CompanyId;
            model.SubCompanyId = dto.SubCompanyId;
            model.UserId = user.Id;
            model.PermissionId = dto.PermissionId;
            model.GroupId = company.GroupId;
            await _companyRepository.AddUserToCompanyOrSubCompany(model.UserId, company.GroupId, model.CompanyId, model.SubCompanyId, model.PermissionId);


            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetSubCompaniesByUserId(int userId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var subCompanies = await _companyRepository.GetSubCompaniesByUserId(userId);

            var subCompanyDtos = subCompanies.Select(sc => new SubCompanyUsersimpleDto
            {
                SubCompanyId = sc.Id,
                SubCompanyName = sc.Name,
                DateCreate = sc.DateCreate,
                CompanyId = sc.Company.Id,


                BusinessEntity = sc.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = sc.BusinessEntity.Id,
                    NomeFantasia = sc.BusinessEntity.NomeFantasia,
                    RazaoSocial = sc.BusinessEntity.RazaoSocial,
                    Cnpj = sc.BusinessEntity.Cnpj,
                    Logradouro = sc.BusinessEntity.Logradouro,
                    Numero = sc.BusinessEntity.Numero,
                    Bairro = sc.BusinessEntity.Bairro,
                    Municipio = sc.BusinessEntity.Municipio,
                    Uf = sc.BusinessEntity.Uf,
                    Cep = sc.BusinessEntity.Cep,
                    Telefone = sc.BusinessEntity.Telefone,
                    Email = sc.BusinessEntity.Email
                },

                Permission = sc.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission != null
                    ? new PermissionResponse
                    {
                        Id = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Id,
                        Name = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Name
                    }
                    : null
            }).ToList();

            return SuccessResponse(subCompanyDtos);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> GetSubCompaniesById(int userId, int companyId, int subcompanyId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var sc = await _companyRepository.GetSubCompanyId(userId, companyId, subcompanyId);

            if (sc == null)
                return SuccessResponse(Message.NotFound);

            var subCompanyDtos = new SubCompanyUsersimpleDto
            {
                SubCompanyId = sc.Id,
                SubCompanyName = sc.Name,
                DateCreate = sc.DateCreate,
                CompanyId = sc.Company.Id,

                BusinessEntity = sc.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = sc.BusinessEntity.Id,
                    NomeFantasia = sc.BusinessEntity.NomeFantasia,
                    RazaoSocial = sc.BusinessEntity.RazaoSocial,
                    Cnpj = sc.BusinessEntity.Cnpj,
                    Logradouro = sc.BusinessEntity.Logradouro,
                    Numero = sc.BusinessEntity.Numero,
                    Bairro = sc.BusinessEntity.Bairro,
                    Municipio = sc.BusinessEntity.Municipio,
                    Uf = sc.BusinessEntity.Uf,
                    Cep = sc.BusinessEntity.Cep,
                    Telefone = sc.BusinessEntity.Telefone,
                    Email = sc.BusinessEntity.Email
                },

                Permission = sc.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission != null
                    ? new PermissionResponse
                    {
                        Id = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Id,
                        Name = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Name
                    }
                    : null
            };

            return SuccessResponse(subCompanyDtos);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetSubCompaniesDeletedByUserId(int userId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var subCompanies = await _companyRepository.GetSubCompaniesDeletedByUserId(userId);

            var subCompanyDtos = subCompanies.Select(sc => new SubCompanyUsersimpleDto
            {
                SubCompanyId = sc.Id,
                SubCompanyName = sc.Name,
                DateCreate = sc.DateCreate,
                CompanyId = sc.Company.Id,


                BusinessEntity = sc.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = sc.BusinessEntity.Id,
                    NomeFantasia = sc.BusinessEntity.NomeFantasia,
                    RazaoSocial = sc.BusinessEntity.RazaoSocial,
                    Cnpj = sc.BusinessEntity.Cnpj,
                    Logradouro = sc.BusinessEntity.Logradouro,
                    Numero = sc.BusinessEntity.Numero,
                    Bairro = sc.BusinessEntity.Bairro,
                    Municipio = sc.BusinessEntity.Municipio,
                    Uf = sc.BusinessEntity.Uf,
                    Cep = sc.BusinessEntity.Cep,
                    Telefone = sc.BusinessEntity.Telefone,
                    Email = sc.BusinessEntity.Email
                },

                Permission = sc.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission != null
                    ? new PermissionResponse
                    {
                        Id = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Id,
                        Name = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Name
                    }
                    : null
            }).ToList();

            return SuccessResponse(subCompanyDtos);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetUsersBySubCompanyId(int groupId, int companyId, int subCompanyId)
    {
        try
        {
            var users = await _groupRepository.GetUsersBySubCompanyId(groupId, companyId, subCompanyId);
            if (users == null) return ErrorResponse(Message.NotFound);

            var response = new List<UserListDto>();
            response.AddRange(users.Select(u => new UserListDto
            {
                Id = u.Id,
                Name = u.User.Name,
                Email = u.User.Email,
                Contact = u.User.Name,
                Permission = new PermissionDto
                {
                    Id = u.Permission.Id,
                    Name = u.Permission.Name
                }
            }));

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    #endregion

    #region Get By User
    public async Task<ResultValue> GetCompaniesByUserId(int userId, int groupId)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var group = await _groupRepository.GetByIdByCompanies(groupId);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            var companies = group.Companies?
                .Where(c => c.CompanyUsers.Any(cu => cu.UserId == userId))
                .ToList();

            var companyDtos = companies?.Select(company => new CompanyUsersimpleDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                DateCreate = company.DateCreate,

                BusinessEntity = company.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = company.BusinessEntity.Id,
                    NomeFantasia = company.BusinessEntity.NomeFantasia,
                    RazaoSocial = company.BusinessEntity.RazaoSocial,
                    Cnpj = company.BusinessEntity.Cnpj,
                    Logradouro = company.BusinessEntity.Logradouro,
                    Numero = company.BusinessEntity.Numero,
                    Bairro = company.BusinessEntity.Bairro,
                    Municipio = company.BusinessEntity.Municipio,
                    Uf = company.BusinessEntity.Uf,
                    Cep = company.BusinessEntity.Cep,
                    Telefone = company.BusinessEntity.Telefone,
                    Email = company.BusinessEntity.Email
                },

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

            var groupDto = new GroupWithCompaniesDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                DateCreate = group.DateCreate,
                UserId = userId,
                UserName = user.Name,
                Companies = companyDtos ?? new List<CompanyUsersimpleDto>()

            };

            return SuccessResponse(groupDto);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
   public async Task<ResultValue> GetByIdByCompaniesDeleted(int userId, int groupId)
{
    try
    {
        var user = await _userRepository.GetByUserId(userId);
        if (user == null)
            return ErrorResponse(UserLoginMessage.InvalidCredentials);

        var dtos = await _groupRepository.GetByIdByCompaniesDeleted(groupId);
        if (dtos == null || !dtos.Any())
            return ErrorResponse(Message.NotFound);

        var companies = dtos
            .Where(c => c.UserId == userId) // filtra as empresas em que o user aparece
            .GroupBy(c => c.CompanyId)
            .Select(g => new CompanyUsersimpleDto
            {
                CompanyId = g.Key,
                CompanyName = g.First().CompanyName,
                BusinessEntity = new BusinessEntityDto
                {
                    Cnpj = g.First().CompanyCnpj,
                    NomeFantasia = g.First().CompanyName
                },
                Permission = g.First().PermissionId != null
                    ? new PermissionResponse
                    {
                        Id = g.First().PermissionId.Value,
                        Name = g.First().PermissionName
                    }
                    : null
            })
            .ToList();

        var groupDto = new GroupWithCompaniesDto
        {
            GroupId = dtos.First().GroupId,
            GroupName = dtos.First().GroupName,
            DateCreate = DateTime.Now, // não vem na query, defina conforme necessidade
            UserId = userId,
            UserName = user.Name,
            Companies = companies
        };

        return SuccessResponse(groupDto);
    }
    catch (Exception ex)
    {
        return ErrorResponse(ex);
    }
}
    public async Task<ResultValue> GetCompaniesByUserIdPaginated(int userId, int groupId, int skip = 0, int take = 10)
    {
        try
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var group = await _groupRepository.GetByIdByCompanies(groupId);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            // Obtém as empresas vinculadas ao usuário no grupo específico
            var companies = await _groupRepository.GetCompaniesByUserIdAndGroupId(userId, groupId);

            // Paginação manual para evitar sobrecarga no banco
            var paginatedCompanies = companies
                .Skip(skip)
                .Take(take)
                .ToList();

            var companyDtos = paginatedCompanies.Select(company =>
            {
                var userCompany = company.CompanyUsers?.FirstOrDefault(cu => cu.UserId == userId);

                return new CompanyUsersimpleDto
                {
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    DateCreate = company.DateCreate,
                    BusinessEntity = company.BusinessEntity != null ? new BusinessEntityDto
                    {
                        Id = company.BusinessEntity.Id,
                        NomeFantasia = company.BusinessEntity.NomeFantasia,
                        RazaoSocial = company.BusinessEntity.RazaoSocial,
                        Cnpj = company.BusinessEntity.Cnpj,
                        Logradouro = company.BusinessEntity.Logradouro,
                        Numero = company.BusinessEntity.Numero,
                        Bairro = company.BusinessEntity.Bairro,
                        Municipio = company.BusinessEntity.Municipio,
                        Uf = company.BusinessEntity.Uf,
                        Cep = company.BusinessEntity.Cep,
                        Telefone = company.BusinessEntity.Telefone,
                        Email = company.BusinessEntity.Email
                    } : null,
                    Permission = userCompany?.Permission != null ? new PermissionResponse
                    {
                        Id = userCompany.Permission.Id,
                        Name = userCompany.Permission.Name
                    } : null
                };
            }).ToList();

            var result = new
            {
                TotalCount = companies.Count,
                Skip = skip,
                Take = take,
                Companies = companyDtos
            };

            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetByIdByCompaniesDeletedPaginated(int userId, int groupId, int skip = 0, int take = 10)
    {
        try
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var group = await _groupRepository.GetByIdByCompaniesDeleted(groupId);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            // Busca todas as empresas deletadas vinculadas ao usuário e grupo
            var allDeletedCompanies = await _groupRepository.GetDeletedCompaniesByUserIdAndGroupId(userId, groupId);

            var totalCount = allDeletedCompanies.Count;

            // Realiza a paginação manualmente
            var paginatedCompanies = allDeletedCompanies
                .Skip(skip)
                .Take(take)
                .ToList();

            var companyDtos = paginatedCompanies.Select(company =>
            {
                var userCompany = company.CompanyUsers?.FirstOrDefault(cu => cu.UserId == userId);

                return new CompanyUsersimpleDto
                {
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    DateCreate = company.DateCreate,
                    BusinessEntity = company.BusinessEntity == null ? null : new BusinessEntityDto
                    {
                        Id = company.BusinessEntity.Id,
                        NomeFantasia = company.BusinessEntity.NomeFantasia,
                        RazaoSocial = company.BusinessEntity.RazaoSocial,
                        Cnpj = company.BusinessEntity.Cnpj,
                        Logradouro = company.BusinessEntity.Logradouro,
                        Numero = company.BusinessEntity.Numero,
                        Bairro = company.BusinessEntity.Bairro,
                        Municipio = company.BusinessEntity.Municipio,
                        Uf = company.BusinessEntity.Uf,
                        Cep = company.BusinessEntity.Cep,
                        Telefone = company.BusinessEntity.Telefone,
                        Email = company.BusinessEntity.Email
                    },
                    Permission = userCompany?.Permission != null
                        ? new PermissionResponse
                        {
                            Id = userCompany.Permission.Id,
                            Name = userCompany.Permission.Name
                        }
                        : null
                };
            }).ToList();

            var result = new
            {
                TotalCount = totalCount,
                Skip = skip,
                Take = take,
                Companies = companyDtos
            };

            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetSubCompaniesByUserIdPaginated(int userId, int companyId, int skip = 0, int take = 10)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Obtém as subempresas filtradas diretamente do repositório
            var allSubCompanies = await _companyRepository.GetSubCompaniesByUserId(userId);

            // Filtra pelo ID da empresa
            var filteredSubCompanies = allSubCompanies
                .Where(sc => sc.CompanyId == companyId)
                .ToList();

            var totalCount = filteredSubCompanies.Count;

            var paginatedSubCompanies = filteredSubCompanies
                .Skip(skip)
                .Take(take)
                .ToList();

            var subCompanyDtos = paginatedSubCompanies.Select(sc => new _2___Application._2_Dto_s.Company.SubCompany.SubCompanyDto
            {
                Id = sc.Id,
                Name = sc.Name,
                DateCreate = sc.DateCreate,
                CompanyId = sc.CompanyId,
                BusinessEntity = sc.BusinessEntity == null ? null : new BusinessEntityDto
                {
                    Id = sc.BusinessEntity.Id,
                    NomeFantasia = sc.BusinessEntity.NomeFantasia,
                    RazaoSocial = sc.BusinessEntity.RazaoSocial,
                    Cnpj = sc.BusinessEntity.Cnpj,
                    Logradouro = sc.BusinessEntity.Logradouro,
                    Numero = sc.BusinessEntity.Numero,
                    Bairro = sc.BusinessEntity.Bairro,
                    Municipio = sc.BusinessEntity.Municipio,
                    Uf = sc.BusinessEntity.Uf,
                    Cep = sc.BusinessEntity.Cep,
                    Telefone = sc.BusinessEntity.Telefone,
                    Email = sc.BusinessEntity.Email
                },
                Permission = sc.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId)?.Permission != null
                    ? new PermissionResponse
                    {
                        Id = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Id,
                        Name = sc.CompanyUsers.First(cu => cu.UserId == userId).Permission.Name
                    }
                    : null
            }).ToList();

            var result = new
            {
                TotalCount = totalCount,
                Skip = skip,
                Take = take,
                SubCompanies = subCompanyDtos
            };

            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    public async Task<ResultValue> GetSubCompaniesDeletedByUserIdPaginated(int userId, int companyId, int skip = 0, int take = 10)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Busca todas as subempresas deletadas da company que o usuário tem vínculo
            var allSubCompanies = await _companyRepository.GetSubCompaniesDeletedByUserId(userId);

            var filteredSubCompanies = allSubCompanies
                .Where(sc => sc.CompanyId == companyId && sc.Deleted) // Garante que estejam deletadas
                .ToList();

            var totalCount = filteredSubCompanies.Count;

            var paginatedSubCompanies = filteredSubCompanies
                .Skip(skip)
                .Take(take)
                .ToList();

            var subCompanyDtos = paginatedSubCompanies.Select(sc =>
            {
                var userCompany = sc.CompanyUsers?.FirstOrDefault(cu => cu.UserId == userId);

                return new _2___Application._2_Dto_s.Company.SubCompany.SubCompanyDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    DateCreate = sc.DateCreate,
                    CompanyId = sc.CompanyId,

                    BusinessEntity = sc.BusinessEntity == null ? null : new BusinessEntityDto
                    {
                        Id = sc.BusinessEntity.Id,
                        NomeFantasia = sc.BusinessEntity.NomeFantasia,
                        RazaoSocial = sc.BusinessEntity.RazaoSocial,
                        Cnpj = sc.BusinessEntity.Cnpj,
                        Logradouro = sc.BusinessEntity.Logradouro,
                        Numero = sc.BusinessEntity.Numero,
                        Bairro = sc.BusinessEntity.Bairro,
                        Municipio = sc.BusinessEntity.Municipio,
                        Uf = sc.BusinessEntity.Uf,
                        Cep = sc.BusinessEntity.Cep,
                        Telefone = sc.BusinessEntity.Telefone,
                        Email = sc.BusinessEntity.Email
                    },

                    Permission = userCompany?.Permission != null
                        ? new PermissionResponse
                        {
                            Id = userCompany.Permission.Id,
                            Name = userCompany.Permission.Name
                        }
                        : null
                };
            }).ToList();

            var result = new
            {
                TotalCount = totalCount,
                Skip = skip,
                Take = take,
                SubCompanies = subCompanyDtos
            };

            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    #endregion
}