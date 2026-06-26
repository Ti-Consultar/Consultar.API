using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Breadcrumb
{
    public class BreadcrumbDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public int? ParentId { get; set; } // ID do pai (Group, Company ou SubCompany)
        public string Type { get; set; }   // Tipo do item (Group, Company, SubCompany)
    }

    public class BreadcrumbResolveQueryDto
    {
        public string RouteKey { get; set; }
        public int? GroupId { get; set; }
        public int? CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
        public int? BalanceteId { get; set; }
    }

    public class BreadcrumbResolveItemDto
    {
        public string Label { get; set; }
        public string Path { get; set; }
        public string RouteKey { get; set; }
        public string Type { get; set; }
    }
}
