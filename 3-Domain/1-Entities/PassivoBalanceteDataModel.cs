using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class PassivoBalanceteDataModel
    {
        public PassivoBalanceteDataModel()
        {
            
        }

        public int Id { get; set; }
        public int AccountPlansId { get; set; }
        public int BalanceteId { get; set; }
        public int PassivoId { get; set; }
        public int BalanceteDataId { get; set; }
        public AccountPlansModel AccountPlans { get; set; }
        public BalanceteModel Balancete { get; set; }
        public ClassificationPassivoModel Passivo { get; set; }
        public BalanceteDataModel BalanceteData { get; set; }
    }
}
