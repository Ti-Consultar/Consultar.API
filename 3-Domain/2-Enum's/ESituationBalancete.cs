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
        [Description("Pendente")]
        Pending =1,

        [Description("Aceito")]
        Accepted = 2,

        [Description("Rejeitado")]
        Rejected = 3,

    }
}
