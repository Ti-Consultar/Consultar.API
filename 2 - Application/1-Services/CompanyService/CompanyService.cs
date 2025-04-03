using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using _2___Application._2_Dto_s.Permissions;
using _2___Application._2_Dto_s.Company.CompanyUser;
using Microsoft.EntityFrameworkCore;


public class CompanyService : BaseService
{
    private readonly CompanyRepository _companyRepository;
    private readonly UserRepository _userRepository;

    public CompanyService(CompanyRepository companyRepository, UserRepository userRepository, IAppSettings appSettings)
        : base(appSettings)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
    }


    #region Companies
    public async Task<ResultValue> CreateCompany(InsertCompanyDto createCompanyDto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(createCompanyDto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            var company = new CompanyModel
            {
                Name = createCompanyDto.Name,
                DateCreate = DateTime.Now
            };

            await _companyRepository.AddCompany(company);

            var companyUser = new CompanyUserModel
            {
                UserId = createCompanyDto.UserId,
                CompanyId = company.Id
            };

            await _companyRepository.AddUserToCompany(companyUser.UserId, companyUser.CompanyId);

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
            var hasPermission = await _companyRepository.ExistsEditCompanyUser(dto.UserId, id);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            // Obtém a empresa relacionada ao usuário
            var companies = await _companyRepository.GetCompaniesByUserId(dto.UserId);
            var company = companies.FirstOrDefault(a => a.Id == id);

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


    public async Task<ResultValue> DeleteCompany(int userId, int id)
    {
        try
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Verifica se o usuário tem permissão para excluir a empresa
            var hasPermission = await _companyRepository.ExistsCompanyUser(userId, id);
            if (!hasPermission)
                return ErrorResponse(Message.Unauthorized);

            var company = await _companyRepository.GetById(id);
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

    public async Task<ResultValue> GetCompaniesById(int id)
    {
        try
        {
            var model = await _companyRepository.GetCompanyById(id);
            if (model == null)
                return ErrorResponse(Message.NotFound);

            var response = new CompanyDto
            {
                Id = model.Id,
                Name = model.Name,
                DateCreate = model.DateCreate
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
            await _companyRepository.AddUserToCompanyOrSubCompany(model.UserId, model.CompanyId, model.SubCompanyId, model.PermissionId);


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
    public async Task<ResultValue> GetCompaniesByUserId(int userId)
    {
        try
        {
            // Verifica se o usuário existe
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Obtém as empresas relacionadas ao usuário
            var companies = await _companyRepository.GetCompaniesByUserId(userId);
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

            // Resposta final
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


    #endregion
}