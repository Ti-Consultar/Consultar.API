using _2___Application._2_Dto_s.BusinesEntity;
using _2___Application._2_Dto_s.Permissions;
using _3_Domain._1_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Group
{
    public class GroupWithCompaniesDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime DateCreate { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public PermissionResponse GroupPermission { get; set; }
        public List<CompanyUsersimpleDto> Companies { get; set; }
        public BusinessEntityDto BusinessEntity { get; set; }
    }
}
