using _2___Application._2_Dto_s.BusinesEntity;
using _2___Application._2_Dto_s.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company.SubCompany
{
    public class SubCompanyDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public DateTime DateCreate { get; set; }  
        public int CompanyId { get; set; } 
        public BusinessEntityDto BusinessEntity { get; set; }
        public PermissionResponse Permission { get; set; }
    }
    public class SubCompanySimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    }
