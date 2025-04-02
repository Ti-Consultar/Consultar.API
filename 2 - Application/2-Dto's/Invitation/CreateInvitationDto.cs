using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Invitation
{
    public class CreateInvitationDto
    {
        public int CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
        public int UserId { get; set; }
        public int InvitedByUserId { get; set; }
        public int PermissionId { get; set; }
    }
}
