using _2___Application._2_Dto_s.BusinesEntity;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company.CompanyUser
{
   public class CompanyUserDto
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public List<CompanyUsersimpleDto> companies { get; set; }

    }
}
public class CompanyUsersimpleDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public DateTime DateCreate { get; set; }
    public BusinessEntityDto BusinessEntity { get; set; }
    public List<SubCompanyUsersimpleDto>? SubCompanies { get; set; }
    public PermissionResponse Permission { get; set; }


}

public class SubCompanyUsersimpleDto
{
    public int SubCompanyId { get; set; }
    public string SubCompanyName { get; set; }
    public int CompanyId { get; set; }
    public DateTime DateCreate { get; set; }
    public BusinessEntityDto BusinessEntity { get; set; }
    public PermissionResponse Permission { get; set; }



}
public class SubCompanyUsersimpleDeleteDto
{
    public int SubCompanyId { get; set; }
    public string SubCompanyName { get; set; }
    public BusinessEntitySimpleDto BusinessEntity { get; set; }
    public PermissionResponse Permission { get; set; }


}