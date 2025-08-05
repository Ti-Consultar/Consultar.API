using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Painel
{
    public class PainelDto
    {
        public List<MonthDto> Months { get; set; }
    }

    public class MonthDto
    {
        public int Month { get; set; }
        public List<GroupDto> Groups { get; set; }
    }

    public class GroupDto
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
    }

}
