using _2___Application._2_Dto_s.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company.SubCompany
{
   public class SubCompanyUserResponse
    {
        public SubCompanyDto SubCompany { get; set; }
        public PermissionResponse Permission { get; set; }
    }
}
