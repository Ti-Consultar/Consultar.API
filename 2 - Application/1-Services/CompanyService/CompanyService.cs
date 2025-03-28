using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using _2___Application._2_Dto_s.Permissions;


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
    public async Task<ResultValue> UpdateCompany( int id, UpdateCompanyDto dto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(dto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Obtém as empresas relacionadas ao usuário
            var companies = await _companyRepository.GetCompaniesByUserId(dto.UserId);

            var company = companies.Where(a => a.Id == id).FirstOrDefault();
            if (company == null)
            {
                return ErrorResponse(Message.NotFound);
            }
            else
            {
                company.Name = dto.Name;
            }

            await _companyRepository.UpdateCompany(company);

            return SuccessResponse(Message.Success);
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
    public async Task<ResultValue> CreateUserCompanyorSubCompany(CreateCompanyUserDto dto)
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
        catch (Exception ex)
        {
            return ErrorResponse(ex);
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

    public async Task<ResultValue> UpdateSubCompany(int id, UpdateCompanyDto dto)
    {
        try
        {
            var user = await _userRepository.GetByUserId(dto.UserId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);

            // Obtém as empresas relacionadas ao usuário
            var companies = await _companyRepository.GetSubCompanieByUserId(dto.UserId);
            
            var subCompany = companies.SubCompanies.Where(a => a.Id == id).FirstOrDefault();

         
            if (subCompany == null)
            {
                return ErrorResponse(Message.NotFound);
            }
            else
            {
                subCompany.Name = dto.Name;
            }

            await _companyRepository.UpdateSubCompany(subCompany);

            return SuccessResponse(Message.Success);
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

            // Se não houver empresas, retorna um erro
            if (companies == null || !companies.Any())
                return ErrorResponse(Message.NotFound);

            // Mapeia as empresas e permissões para o response
            var companyResponses = companies.Select(company => new CompanyUserResponse
            {
                // Mapeia informações da empresa
                Company = new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    DateCreate = company.DateCreate,
                    Permission = company.CompanyUsers
                        .Select(cu => new PermissionResponse
                        {
                            Id = cu.Permission.Id,
                            Name = cu.Permission.Name
                        }).FirstOrDefault() // Inclui a permissão diretamente dentro de CompanyDto
                }
            }).ToList();

            // Para tratar as subempresas, se houver
            var subCompanyResponses = companies.SelectMany(company => company.SubCompanies)
                .Select(subCompany => new SubCompanyUserResponse
                {
                    // Mapeia informações da subempresa
                    SubCompany = new SubCompanyDto
                    {
                        Id = subCompany.Id,
                        Name = subCompany.Name,
                        DateCreate = subCompany.DateCreate
                    },

                    // Mapeia permissões associadas à subempresa (uma permissão por vínculo)
                    Permission = subCompany.CompanyUsers.Select(cu => new PermissionResponse
                    {
                        Id = cu.Permission.Id,
                        Name = cu.Permission.Name
                    }).FirstOrDefault() // Um único vínculo de permissão, então pegamos o primeiro ou null
                }).ToList();

            // Resposta final
            var response = new
            {
                Companies = companyResponses,
                SubCompanies = subCompanyResponses
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

            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
                return ErrorResponse(UserLoginMessage.InvalidCredentials);


            var companies = await _companyRepository.GetCompaniesByUserIdPaginated(userId, skip, take);

            if (companies == null || !companies.Any())
                return ErrorResponse(Message.NotFound);


            var companyDtos = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                DateCreate = c.DateCreate,
                SubCompanies = c.SubCompanies?.Select(sub => new SubCompanyDto
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    DateCreate = sub.DateCreate,
                    CompanyId = c.Id,
                    CompanyName = c.Name
                }).ToList() ?? new List<SubCompanyDto>()
            }).ToList();

            return SuccessResponse(companyDtos);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex);
        }
    }

    #endregion
}