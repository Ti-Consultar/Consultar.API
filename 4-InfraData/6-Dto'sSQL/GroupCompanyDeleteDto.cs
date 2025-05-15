using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._6_Dto_sSQL
{
    [Keyless]
    public class GroupCompanyDeletedDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCnpj { get; set; }
        public int? UserId { get; set; }
        public int? PermissionId { get; set; }
        public string PermissionName { get; set; }
    }


}
[Keyless]
public class GroupSubCompanyDeletedDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public int SubCompanyId { get; set; }
    public string SubCompanyName { get; set; }
    public string SubCompanyCnpj { get; set; }
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }
}