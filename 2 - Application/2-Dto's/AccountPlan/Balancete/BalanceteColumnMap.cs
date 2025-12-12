using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.AccountPlan.Balancete
{
    public class BalanceteColumnMap
    {
        public IFormFile File { get; set; }
        public int StartRow { get; set; }

        public int CostCenter { get; set; }
        public int Name { get; set; }
        public int InitialValue { get; set; }
        public int Debit { get; set; }
        public int Credit { get; set; }
        public int FinalValue { get; set; }
    }

}
