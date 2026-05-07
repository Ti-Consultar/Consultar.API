using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.ConfigPrincipal
{
    public class InsertViewConfigDto
    {
        public int AccountPlanId { get; set; }
        public List<ConfigItemDto> Configs { get; set; }
    }

    public class ConfigItemDto
    {
        public int ConfigPrincipalId { get; set; }
        public List<int> SonConfigIds { get; set; }
    }
}
