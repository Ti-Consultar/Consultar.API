using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.AccountPlan.Balancete
{
    public class BalanceteDataDto
    {
        public BalanceteDto Balancete { get; set; }
        public List<DataDto> DataDto { get; set; }

    }

    public class DataDto
    {
        public int Id { get; set; }
        public string CostCenter { get; set; }
        public string Name { get; set; }
        public decimal InitialValue { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal FinalValue { get; set; }
        public bool BudgetedAmount { get; set; }
    }

    // Orçamento

    public class BudgetDataDto
    {
        public BudgetDto Budget { get; set; }
        public List<DataDto> DataDto { get; set; }

    }

   
}
