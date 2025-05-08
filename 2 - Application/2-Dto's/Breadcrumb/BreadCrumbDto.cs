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

}
