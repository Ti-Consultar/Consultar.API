using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._2_Enum_s
{
   public enum ERole
    {
        [Description("Admin")]
        Admin = 1,

        [Description("Gestor")]
        Gestor = 2,

        [Description("Usuário")]
        Usuario = 3,
        [Description("Consultor")]
        Consultor = 4,

        [Description("Comercial")]
        Comercial = 5,

        [Description("Desenvolvedor")]
        Desenvolvedor = 6,
        [Description("Designer")]
        Designer = 7,
    }
}
