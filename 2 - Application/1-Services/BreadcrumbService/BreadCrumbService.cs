using _2___Application._2_Dto_s.Breadcrumb;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

public class BreadcrumbService : BaseService
{
    private readonly GroupRepository _groupRepository;
    private readonly CompanyRepository _companyRepository;
    private static readonly IReadOnlyDictionary<string, BreadcrumbScreenDefinition> ScreenRegistry =
        new Dictionary<string, BreadcrumbScreenDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["dashboard"] = new("Dashboard", "empresas"),
            ["upload-balance-sheet"] = new("Upload de Balancete", "arquivos/upload/balancete", "Uploads"),
            ["balance-column-mapping"] = new("Mapeamento de Colunas do Balancete", "arquivos/upload/balancete/colunas", "Uploads"),
            ["upload-budget-sheet"] = new("Upload de Orçamento", "arquivos/upload/orcamento", "Uploads"),
            ["upload-account-plan"] = new("Upload de Plano de Contas", "arquivos/upload/plano-contas", "Uploads"),
            ["balance-sheets"] = new("Balancetes", "balancetes"),
            ["balance-sheet-data"] = new("Dados do Balancete", "balancetes/{balanceteId}", "Balancetes"),
            ["balance-sheet-detailed"] = new("Balancete Detalhado", "balancetes/{balanceteId}/detalhado", "Balancetes"),
            ["balance-assets-liabilities"] = new("Balanço Contábil do Balancete", "balancetes/{balanceteId}/balanco-contabil", "Balancetes"),
            ["classification"] = new("Classificação", "classificacao", "Administração"),
            ["parameters"] = new("Parametrização", "parametros", "Administração"),
            ["accounting-balance"] = new("Balanço Contábil", "contabil", "Demonstrações Financeiras"),
            ["financial-statements"] = new("Demonstrações Contábeis", "demonstracoes-contabeis", "Demonstrações Financeiras"),
            ["brand-statements"] = new("Demonstrações por Marca", "demonstracoes-marcas", "Demonstrações Financeiras"),
            ["liquidity-management"] = new("Gestão de Liquidez", "resultados/gestao-liquidez", "Resultados"),
            ["economic-indices"] = new("Índices Econômicos", "resultados/indices-economicos", "Resultados"),
            ["cil-ec"] = new("CIL e PFL", "resultados/cil-ec", "Resultados"),
            ["operational-efficiency"] = new("Eficiência Operacional", "resultados/eficiencia-operacional", "Resultados"),
            ["cash-flow"] = new("Fluxo de Caixa", "fluxo-caixa"),
            ["eva"] = new("Árvore de Valor", "eva"),
            ["home"] = new("Início", "dashboard", null, false),
            ["profile-info"] = new("Informações", "perfil/informacoes", "Perfil", false),
            ["profile-security"] = new("Segurança", "perfil/seguranca", "Perfil", false),
            ["profile-customizing"] = new("Personalização", "perfil/personalizacao", "Perfil", false),
            ["users"] = new("Usuários", "users", null, false),
            ["groups"] = new("Grupos", "grupos", null, false)
        };


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
                case "group":
                    var group = await _groupRepository.GetById(id);
                    if (group != null)
                    {
                        currentItem = new BreadcrumbDto
                        {
                            Id = group.Id,
                            Name = group.Name,
                            Link = $"/groups/{group.Id}",
                            Type = "group"
                        };
                    }
                    break;

                case "company":
                    var company = await _companyRepository.GetById(id);
                    if (company != null)
                    {
                        currentItem = new BreadcrumbDto
                        {
                            Id = company.Id,
                            Name = company.Name,
                            Link = $"/companies/{company.Id}",
                            ParentId = company.GroupId,
                            Type = "company"
                        };

                        // prepara para buscar o Group na próxima iteração
                        id = company.GroupId;
                        type = "group";
                    }
                    break;

                case "subcompany":
                    var subCompany = await _companyRepository.GetSubCompanyById(id);
                    if (subCompany != null)
                    {
                        currentItem = new BreadcrumbDto
                        {
                            Id = subCompany.Id,
                            Name = subCompany.Name,
                            Link = $"/subcompanies/{subCompany.Id}",
                            ParentId = subCompany.CompanyId,
                            Type = "subcompany"
                        };

                        // prepara para buscar a Company na próxima iteração
                        id = subCompany.CompanyId;
                        type = "company";
                    }
                    break;
            }

            if (currentItem == null)
                break;

            // insere sempre à frente para manter ordem hierárquica
            breadcrumb.Insert(0, currentItem);

            // agora sim: se adicionamos um Group, paramos
            if (currentItem.Type == "group")
                break;
        }

        return breadcrumb;
    }

    public async Task<List<BreadcrumbResolveItemDto>> ResolveBreadcrumbAsync(BreadcrumbResolveQueryDto query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        if (string.IsNullOrWhiteSpace(query.RouteKey))
            throw new ArgumentException("RouteKey é obrigatório.");

        if (!ScreenRegistry.TryGetValue(query.RouteKey, out var screen))
            throw new ArgumentException($"RouteKey '{query.RouteKey}' não encontrado.");

        var breadcrumb = new List<BreadcrumbResolveItemDto>();

        if (screen.IncludeScope)
            breadcrumb.AddRange(await ResolveScopeAsync(query));

        if (!string.IsNullOrWhiteSpace(screen.ParentGroup))
        {
            breadcrumb.Add(new BreadcrumbResolveItemDto
            {
                Label = screen.ParentGroup,
                Path = null,
                RouteKey = null,
                Type = "section"
            });
        }

        breadcrumb.Add(new BreadcrumbResolveItemDto
        {
            Label = screen.Label,
            Path = BuildScreenPath(query, screen),
            RouteKey = query.RouteKey,
            Type = "screen"
        });

        return breadcrumb;
    }

    private async Task<List<BreadcrumbResolveItemDto>> ResolveScopeAsync(BreadcrumbResolveQueryDto query)
    {
        var items = new List<BreadcrumbResolveItemDto>
        {
            new()
            {
                Label = "Grupos",
                Path = "/grupos",
                Type = "root"
            }
        };

        if (query.GroupId == null)
            return items;

        var group = await _groupRepository.GetById(query.GroupId.Value);
        if (group == null)
            return items;

        items.Add(new BreadcrumbResolveItemDto
        {
            Label = group.Name,
            Path = $"/grupos/{group.Id}/empresas",
            Type = "group"
        });

        if (query.CompanyId == null)
            return items;

        var company = await _companyRepository.GetById(query.CompanyId.Value);
        if (company == null)
            return items;

        items.Add(new BreadcrumbResolveItemDto
        {
            Label = company.Name,
            Path = $"/grupos/{group.Id}/empresas/{company.Id}/filiais",
            Type = "company"
        });

        if (query.SubCompanyId == null)
            return items;

        var subCompany = await _companyRepository.GetSubCompanyById(query.SubCompanyId.Value);
        if (subCompany == null)
            return items;

        items.Add(new BreadcrumbResolveItemDto
        {
            Label = subCompany.Name,
            Path = $"/grupos/{group.Id}/empresas/{company.Id}/filiais/{subCompany.Id}",
            Type = "subcompany"
        });

        return items;
    }

    private static string BuildScreenPath(BreadcrumbResolveQueryDto query, BreadcrumbScreenDefinition screen)
    {
        var segment = FormatPathSegment(screen.PathSegment, query);

        if (!screen.IncludeScope)
            return "/" + segment.Trim('/');

        if (query.GroupId == null)
            return "/" + segment.Trim('/');

        if (query.RouteKey.Equals("dashboard", StringComparison.OrdinalIgnoreCase))
        {
            if (query.CompanyId != null && query.SubCompanyId != null)
                return $"/grupos/{query.GroupId}/empresas/{query.CompanyId}/filiais/{query.SubCompanyId}";

            if (query.CompanyId != null)
                return $"/grupos/{query.GroupId}/empresas/{query.CompanyId}/filiais";

            return $"/grupos/{query.GroupId}/empresas";
        }

        var basePath = $"/grupos/{query.GroupId}";

        if (query.CompanyId != null && query.SubCompanyId != null)
            basePath += $"/empresas/{query.CompanyId}/filiais/{query.SubCompanyId}";
        else if (query.CompanyId != null)
            basePath += $"/empresas/{query.CompanyId}";

        return $"{basePath}/{segment.Trim('/')}";
    }

    private static string FormatPathSegment(string pathSegment, BreadcrumbResolveQueryDto query)
    {
        return pathSegment
            .Replace("{balanceteId}", query.BalanceteId?.ToString() ?? string.Empty)
            .Replace("{groupId}", query.GroupId?.ToString() ?? string.Empty)
            .Replace("{companyId}", query.CompanyId?.ToString() ?? string.Empty)
            .Replace("{subCompanyId}", query.SubCompanyId?.ToString() ?? string.Empty)
            .Trim('/');
    }

    private sealed record BreadcrumbScreenDefinition(
        string Label,
        string PathSegment,
        string ParentGroup = null,
        bool IncludeScope = true);
}
