using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class BudgetDataModel
    {
        public BudgetDataModel()
        {
            
        }
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public BudgetModel Budget { get; set; }
        public string CostCenter { get; set; }
        public string Name { get; set; }
        public decimal InitialValue { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal FinalValue { get; set; }
        public bool BudgetedAmount  { get; set; }
    }
}
