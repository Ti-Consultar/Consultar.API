using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._2_Enum_s
{
    public enum ESituationBalancete
    {
        [Description("Pending")]
        Pending = 0,

        [Description("Accepted")]
        Accepted = 1,

        [Description("Rejected")]
        Rejected = 2,

    }
}
