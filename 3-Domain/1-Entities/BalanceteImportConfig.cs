using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class BalanceteImportConfig
    {
        public BalanceteImportConfig()
        {
            
        }

        public int Id { get; set; }
        public int AccountPlanId { get; set; }

        public int StartRow { get; set; }

        public int CostCenterCol { get; set; }
        public int NameCol { get; set; }
        public int InitialValueCol { get; set; }
        public int DebitCol { get; set; }
        public int CreditCol { get; set; }
        public int FinalValueCol { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
