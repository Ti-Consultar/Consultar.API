using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._2_Enum_s
{
    public enum ETypeClassification
    {
        [Description("Ativo")]
        Ativo = 1,

        [Description("Passivo")]
        Passivo = 2,

        [Description("DRE")]
        DRE = 3,
    }
}
