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


public class CompanyService : BaseService
{
    private readonly CompanyRepository _companyRepository;
    private readonly UserRepository _userRepository;
    private readonly BusinessEntityRepository _businessEntityRepository;
    private readonly GroupRepository _groupRepository;

    public CompanyService(CompanyRepository companyRepository, UserRepository userRepository, BusinessEntityRepository businessEntityRepository, GroupRepository groupRepository, IAppSettings appSettings)
        : base(appSettings)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _businessEntityRepository = businessEntityRepository;
        _groupRepository = groupRepository;
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
            await _companyRepository.UpdateCompany(company);

            return SuccessResponse(Message.Success);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }
    public async Task<ResultValue> DeleteCompany(int id, int userId, int groupId)
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
                return ErrorResponse(Message.NotFound);

            await _companyRepository.DeleteCompany(company.Id);

            return SuccessResponse(Message.DeleteSuccess);
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
    public async Task<ResultValue> CreateSubCompany(InsertSubCompanyDto createsubCompanyDto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(createsubCompanyDto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var company = await _companyRepository.GetById(createsubCompanyDto.CompanyId);
            if (company == null)
                return ErrorResponse(Message.NotFound);

            var subcompany = new SubCompanyModel
            {
                Name = createsubCompanyDto.Name,
                CompanyId = createsubCompanyDto.CompanyId,
                DateCreate = DateTime.Now
            };
            await _companyRepository.AddSubCompany(subcompany.CompanyId, subcompany);
            var companyUser = new CompanyUserModel
            {
                UserId = createsubCompanyDto.UserId,
                CompanyId = company.Id,
                GroupId = company.GroupId,
                PermissionId = 1
            };

            await _companyRepository.AddUserToCompanyOrSubCompany(companyUser.UserId, company.GroupId, companyUser?.CompanyId, subcompany.Id, companyUser.PermissionId);

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

            // Obtém a subempresa relacionada ao usuário
            var subCompanies = await _companyRepository.GetSubCompanieByUserId(dto.UserId);
            var subCompany = subCompanies.SubCompanies.FirstOrDefault(a => a.Id == id);

            if (subCompany == null)
                return ErrorResponse(Message.NotFound);

            subCompany.Name = dto.Name;

            await _companyRepository.UpdateSubCompany(subCompany);

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
                return ErrorResponse(Message.NotFound);

            await _companyRepository.DeleteSubCompany(company.Id, subCompany.Id);

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
            var subCompanies = await _companyRepository.GetSubCompaniesByUserId(userId);
            var subCompanyDtos = subCompanies.Select(sc => new SubCompanyDto
            {
                Id = sc.Id,
                Name = sc.Name,
                DateCreate = sc.DateCreate
            }).ToList();

            return SuccessResponse(subCompanyDtos);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
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








    public async Task<ResultValue> GetCompaniesByUserIdPaginated(int userId, int skip, int take)
    {
        try
        {
            // Verifica se o usuário existe
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Obtém as empresas paginadas
            var companies = await _companyRepository.GetCompaniesByUserIdPaginated(userId, skip, take);
            if (companies == null || !companies.Any())
                return ErrorResponse(Message.NotFound);

            // Mapeia as empresas e permissões para o response
            var companyResponses = companies.Select(company => new CompanyUsersimpleDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                DateCreate = company.DateCreate,
                Permission = company.CompanyUsers?.FirstOrDefault()?.Permission != null ? new PermissionResponse
                {
                    Id = company.CompanyUsers.FirstOrDefault().Permission.Id,
                    Name = company.CompanyUsers.FirstOrDefault().Permission.Name
                } : null,
                SubCompanies = company.SubCompanies?
                    .Where(subCompany => subCompany.CompanyUsers.Any(cu => cu.UserId == userId)) // Filtra apenas as vinculadas ao usuário
                    .Select(subCompany => new SubCompanyUsersimpleDto
                    {
                        SubCompanyId = subCompany.Id,
                        SubCompanyName = subCompany.Name,
                        CompanyId = company.Id,
                        DateCreate = subCompany.DateCreate,
                        Permission = subCompany.CompanyUsers?
                            .FirstOrDefault(cu => cu.UserId == userId)?.Permission != null ? new PermissionResponse
                            {
                                Id = subCompany.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId).Permission.Id,
                                Name = subCompany.CompanyUsers.FirstOrDefault(cu => cu.UserId == userId).Permission.Name
                            } : null
                    }).ToList() ?? new List<SubCompanyUsersimpleDto>()
            }).ToList();

            // Resposta final com paginação
            var response = new CompanyUserDto
            {
                UserId = userId,
                Name = user.Name,
                companies = companyResponses ?? new List<CompanyUsersimpleDto>()
            };

            return SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    //public async Task SaveCompanyFromCnpj(CnpjResponseDto dto)
    //{
    //    var company = new CompanyModel
    //    {
    //        Name = dto.Nome,
    //        FantasyName = dto.Fantasia,
    //        Cnpj = dto.Cnpj,
    //        Address = $"{dto.Logradouro}, {dto.Numero}, {dto.Bairro} - {dto.Municipio}/{dto.Uf}",
    //        DateCreate = DateTime.UtcNow
    //    };

    //    await _companyRepository.AddAsync(company);
    //    await _unitOfWork.SaveChangesAsync();
    //}

    #endregion
}