using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class InvitationToCompany
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int? CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
        public int UserId { get; set; }
        public int InvitedById { get; set; }
        public int PermissionId { get; set; }
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relações
        public GroupModel Group { get; set; }
        public CompanyModel Company { get; set; }
        public SubCompanyModel SubCompany { get; set; }
        public UserModel User { get; set; }
        public UserModel InvitedBy { get; set; }
        public PermissionModel Permission { get; set; }
    }
  

}
