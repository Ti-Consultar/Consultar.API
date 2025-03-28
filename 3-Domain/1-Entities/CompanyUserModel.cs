using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace _3_Domain._1_Entities
{
    public class CompanyUserModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public UserModel User { get; set; }

        public int CompanyId { get; set; }
        public CompanyModel Company { get; set; }

        public int? SubCompanyId { get; set; }
        public SubCompanyModel? SubCompany { get; set; }
        public int PermissionId { get; set; }
        public PermissionModel Permission { get; set; }
    }
}
