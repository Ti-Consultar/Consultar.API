using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._2_Enum_s
{
    public enum InvitationStatus
    {
        [Description("Pending")]
        Pending = 1,

        [Description("Accepted")]
        Accepted = 2,

        [Description("Rejected")]
        Rejected = 3,

    }
}
