using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification
{
   public class BalanceteDataAccountPlanClassificationCreate
    {

        public List<CostCenterBond> CostCenters { get; set; }
    }

    public class CostCenterBond
    {

        public string CostCenter { get; set; }
    }
}
