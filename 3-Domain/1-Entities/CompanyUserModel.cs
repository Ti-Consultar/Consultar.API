using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class CompanyUserModel
    {
        public int UserId { get; set; }
        public UserModel User { get; set; }

        public int CompanyId { get; set; }
        public CompanyModel Company { get; set; }
    }
}
