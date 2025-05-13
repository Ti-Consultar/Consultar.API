using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class BalanceteDataModel
    {
        public BalanceteDataModel()
        {
            
        }
        public int Id { get; set; }
        public int BalanceteId { get; set; }
        public BalanceteModel Balancete { get; set; }
        public string CostCenter { get; set; }
        public string Name { get; set; }
        public decimal InitialValue { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal FinalValue { get; set; }
        public bool BudgetAmount  { get; set; }
    }
}
