using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._3_Utils.Base;
using SubCompanyDto = _2___Application._2_Dto_s.Company.SubCompany.SubCompanyDto;

public class CompanyService
{
    private readonly CompanyRepository _companyRepository;
    private readonly UserRepository _userRepository;  // Repositório para buscar o usuário

    public CompanyService(CompanyRepository companyRepository, UserRepository userRepository)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
    }

    // Criação de uma empresa, associação de usuário à empresa, e criação de subempresas
    public async Task<string> CreateCompany(InsertCompanyDto createCompanyDto)
    {
        // Verificar se o usuário existe
        var user = await _userRepository.GetByUserId(createCompanyDto.UserId);
        if (user == null)
        {
            return UserLoginMessage.InvalidCredentials; // Usar a mensagem de erro adequada
        }

        // Criar a empresa
        var company = new CompanyModel
        {
            Name = createCompanyDto.Name,
            DateCreate = DateTime.Now
        };

        // Adicionar a empresa
        await _companyRepository.AddCompany(company);

        // Criar o relacionamento entre o usuário e a empresa
        var companyUser = new CompanyUserModel
        {
            UserId = createCompanyDto.UserId,
            CompanyId = company.Id
        };

        await _companyRepository.AddUserToCompany(companyUser.UserId, companyUser.CompanyId);

        // Retornar mensagem de sucesso
        return Message.Success;
    }

    public async Task<string> CreateSubCompany(InsertSubCompanyDto createsubCompanyDto)
    {
        // Verificar se o usuário existe
        var user = await _userRepository.GetByUserId(createsubCompanyDto.UserId);
        if (user == null)
        {
            return UserLoginMessage.InvalidCredentials; 
        }
        var company = await _companyRepository.GetById(createsubCompanyDto.CompanyId);

        if (company == null)
        {
            return Message.NotFound; 
        }

        // Criar a empresa
        var subcompany = new SubCompanyModel
        {
            Name = createsubCompanyDto.Name,
            CompanyId = createsubCompanyDto.CompanyId,
            DateCreate = DateTime.Now
        };

        // Adicionar a empresa
        await _companyRepository.AddSubCompany(subcompany.CompanyId,subcompany);


        // Retornar mensagem de sucesso
        return Message.Success;
    }

    // Método para obter subempresas associadas a um usuário
    public async Task<List<SubCompanyDto>> GetSubCompaniesByUserId(int userId)
    {
        var subCompanies = await _companyRepository.GetSubCompaniesByUserId(userId);

        var subCompanyDtos = subCompanies.Select(sc => new SubCompanyDto
        {
            Id = sc.Id,
            Name = sc.Name,
            DateCreate = sc.DateCreate
        }).ToList();

        return subCompanyDtos;
    }

    // Método para obter empresas associadas a um usuário
    public async Task<List<CompanyDto>> GetCompaniesByUserId(int userId)
    {
        var companies = await _companyRepository.GetCompaniesByUserId(userId);

        var companyDtos = companies.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            DateCreate = c.DateCreate
        }).ToList();

        return companyDtos;
    }
}
