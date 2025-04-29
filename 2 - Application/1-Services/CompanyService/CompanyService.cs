using _2___Application._2_Dto_s.Company;
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

            // Verifica se o usuário tem permissão para atualizar a empresa
            var hasPermission = await _companyRepository.ExistsEditCompanyUser(dto.UserId, id, dto.GroupId);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            // Obtém a empresa relacionada ao usuários
            var company = await _companyRepository.GetCompanyByUserId(id, dto.UserId, dto.GroupId);

            if (company == null)
                return ErrorResponse(Message.NotFound);

            // Atualiza os dados da empresa
            company.Name = dto.Name;
            company.BusinessEntity.NomeFantasia = dto.Name;
            await _companyRepository.UpdateCompany(company);
            await _businessEntityRepository.Update(company.BusinessEntity);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
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

    public async Task<ResultValue> RestoreCompany(int userId, int id, int groupId)
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

            await _companyRepository.RestoreCompany(company.Id);

            return SuccessResponse(Message.DeletedSuccess);
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

            var response = new CompanyDto
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

            // Obtém a subempresa
            var subCompanies = await _companyRepository.GetSubCompanieByUserId(dto.UserId);
            var subCompany = subCompanies.SubCompanies.FirstOrDefault(a => a.Id == id);

            if (subCompany == null)
                return ErrorResponse(Message.NotFound);

            // Atualiza o nome da SubCompany
            subCompany.Name = dto.Name;
            subCompany.BusinessEntity.NomeFantasia = dto.Name;

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

    public async Task<ResultValue> RestoreSubCompany(int userId, int companyId, int subCompanyId)
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

            await _companyRepository.RestoreSubCompany(company.Id, subCompany.Id);

            return SuccessResponse(Message.DeleteSuccess);
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

            var group = await _groupRepository.GetByIdByCompaniesDeleted(groupId);
            if (group == null)
                return ErrorResponse(Message.NotFound);

            var companies = group.Companies?
                .Where(c => c.CompanyUsers.Any(cu => cu.UserId == userId))
                .Where(a => a.Deleted == true)
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

            var paginated = await _groupRepository.GetCompaniesByUserIdPaginatedAsync(userId, groupId, skip, take);

            var companyDtos = paginated.Items.Select(company =>
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
                TotalCount = paginated.TotalCount,
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

            var paginated = await _groupRepository.GetCompaniesByUserIdPaginatedAsync(userId, groupId, skip, take);

            var companyDtos = paginated.Items.Select(company =>
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
                TotalCount = paginated.TotalCount,
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

            // Busca todas as subempresas da company que o usuário tem vínculo
            var allSubCompanies = await _companyRepository.GetSubCompaniesByUserId(userId);

            var filteredSubCompanies = allSubCompanies
                .Where(sc => sc.CompanyId == companyId)
                .ToList();

            var totalCount = filteredSubCompanies.Count;

            var paginatedSubCompanies = filteredSubCompanies
                .Skip(skip)
                .Take(take)
                .ToList();

            var subCompanyDtos = paginatedSubCompanies.Select(sc => new SubCompanyDto
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

            // Busca todas as subempresas da company que o usuário tem vínculo
            var allSubCompanies = await _companyRepository.GetSubCompaniesDeletedByUserId(userId);

            var filteredSubCompanies = allSubCompanies
                .Where(sc => sc.CompanyId == companyId)
                .ToList();

            var totalCount = filteredSubCompanies.Count;

            var paginatedSubCompanies = filteredSubCompanies
                .Skip(skip)
                .Take(take)
                .ToList();

            var subCompanyDtos = paginatedSubCompanies.Select(sc => new SubCompanyDto
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
    #endregion
}