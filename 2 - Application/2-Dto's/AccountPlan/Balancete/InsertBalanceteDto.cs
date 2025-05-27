using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.AccountPlan.Balancete
{
    public class InsertBalanceteDto
    {

        public int AccountPlansId { get; set; }
        public int DateMonth { get; set; }
        public int DateYear { get; set; }
    }
}
