using _2___Application._2_Dto_s.Breadcrumb;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BreadcrumbService : BaseService
{
    private readonly GroupRepository _groupRepository;
    private readonly CompanyRepository _companyRepository;


    public BreadcrumbService(
        GroupRepository groupRepository,
        CompanyRepository companyRepository,

        IAppSettings appSettings) : base(appSettings)
    {
        _groupRepository = groupRepository;
        _companyRepository = companyRepository;
 
    }

    public async Task<List<BreadcrumbDto>> GetBreadcrumbAsync(int id, string type)
    {
        var breadcrumb = new List<BreadcrumbDto>();

        while (true)
        {
            BreadcrumbDto currentItem = null;

            switch (type)
            {
                case "Group":
                    var group = await _groupRepository.GetById(id);
                    if (group != null)
                    {
                        currentItem = new BreadcrumbDto
                        {
                            Id = group.Id,
                            Name = group.Name,
                            Link = $"/groups/{group.Id}",
                            Type = "Group"
                        };
                    }
                    break;

                case "Company":
                    var company = await _companyRepository.GetById(id);
                    if (company != null)
                    {
                        currentItem = new BreadcrumbDto
                        {
                            Id = company.Id,
                            Name = company.Name,
                            Link = $"/companies/{company.Id}",
                            ParentId = company.GroupId,
                            Type = "Company"
                        };

                        // prepara para buscar o Group na próxima iteração
                        id = company.GroupId;
                        type = "Group";
                    }
                    break;

                case "SubCompany":
                    var subCompany = await _companyRepository.GetSubCompanyById(id);
                    if (subCompany != null)
                    {
                        currentItem = new BreadcrumbDto
                        {
                            Id = subCompany.Id,
                            Name = subCompany.Name,
                            Link = $"/subcompanies/{subCompany.Id}",
                            ParentId = subCompany.CompanyId,
                            Type = "SubCompany"
                        };

                        // prepara para buscar a Company na próxima iteração
                        id = subCompany.CompanyId;
                        type = "Company";
                    }
                    break;
            }

            if (currentItem == null)
                break;

            // insere sempre à frente para manter ordem hierárquica
            breadcrumb.Insert(0, currentItem);

            // agora sim: se adicionamos um Group, paramos
            if (currentItem.Type == "Group")
                break;
        }

        return breadcrumb;
    }





}
