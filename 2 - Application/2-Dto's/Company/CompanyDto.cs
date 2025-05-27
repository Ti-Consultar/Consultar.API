

using _2___Application._2_Dto_s.BusinesEntity;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Permissions;

namespace _2___Application._2_Dto_s.Company
{
    public class CompanyDto
    {
        public int Id { get; set; }  
        public string Name { get; set; } 
        public DateTime DateCreate { get; set; }
        public BusinessEntityDto BusinessEntity { get; set; }
        public List<SubCompanyDto> ?SubCompanies { get; set; }
        public PermissionResponse Permission { get; set; }


    }
    public class CompanySimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
       
    }
}
