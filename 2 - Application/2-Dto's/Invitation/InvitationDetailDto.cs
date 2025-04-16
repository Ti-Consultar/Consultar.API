using _2___Application._2_Dto_s.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Invitation
{
    public class InvitationDetailDto
    {
        public int Id { get; set; }
        public GroupSimpleDto Group { get; set; }
        public CompanyDto Company { get; set; }
        public SubCompanyDto SubCompany { get; set; }
        public UserDto User { get; set; }
        public UserDto InvitedByUser { get; set; }
        public PermissionDto Permission { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SubCompanyDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
