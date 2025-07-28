using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class BalanceteDataAccountPlanClassification
    {
        public BalanceteDataAccountPlanClassification()
        {
            
        }

        public int Id { get; set; }
        public int AccountPlanClassificationId { get; set; }
        public AccountPlanClassification AccountPlanClassification { get; set; }
        public string CostCenter { get; set; }
    }
}
