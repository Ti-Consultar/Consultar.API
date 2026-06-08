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

    public class InsertBalanceteImportConfig
    {

        public int AccountPlanId { get; set; }

        public int StartRow { get; set; }

        public int CostCenterCol { get; set; }
        public int NameCol { get; set; }
        public int InitialValueCol { get; set; }
        public int DebitCol { get; set; }
        public int CreditCol { get; set; }
        public int FinalValueCol { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public class UpdateBalanceteImportConfig
    {
        public int AccountPlanId { get; set; }
        public int StartRow { get; set; }
        public int CostCenterCol { get; set; }
        public int NameCol { get; set; }
        public int InitialValueCol { get; set; }
        public int DebitCol { get; set; }
        public int CreditCol { get; set; }
        public int FinalValueCol { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
