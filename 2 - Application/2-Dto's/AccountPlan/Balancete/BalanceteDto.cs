using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.AccountPlan.Balancete
{
    public class BalanceteDto
    {
        public int Id { get; set; }
        public AccountPlanResponse AccountPlans { get; set; }
        public EMonth DateMonth { get; set; }
        public int DateYear { get; set; }
        public ESituationBalancete Status { get; set; }
        public DateTime DateCreate { get; set; }
    }

    // Orçamento 

    public class BudgetDto
    {
        public int Id { get; set; }
        public AccountPlanResponse AccountPlans { get; set; }
        public EMonth DateMonth { get; set; }
        public int DateYear { get; set; }
        public ESituationBalancete Status { get; set; }
        public DateTime DateCreate { get; set; }
    }
}
