using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._2_Enum_s
{
    public enum EAction
    {
        [Description("Create")]
        Create = 1,

        [Description("Edit")]
        Edit = 2,

        [Description("Delete")]
        Delete = 3,

        [Description("View")]
        View = 4,

        [Description("List")]
        List = 5
    }

}
