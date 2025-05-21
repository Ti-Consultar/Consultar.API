using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Invitation
{
    public class CreateInvitationDto
    {
        public int GroupId { get; set; }
        public int? CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
        public string EmailInvitedByUser { get; set; }
        public int PermissionId { get; set; }
    }
    public class CreateInvitationBatchDto
    {
        public List<CreateInvitationDto> Invitations { get; set; }
    }

}
