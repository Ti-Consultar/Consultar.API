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

    public class BalanceteDataAccountPlanClassificationCreateList
    {
        public List<BondCreateDto> BondList { get; set; }
    }

    public class BondCreateDto
    {
        public int AccountPlanClassificationId { get; set; }
        public List<CostCenterDto> CostCenters { get; set; }
    }

    public class CostCenterDto
    {
        public string CostCenter { get; set; }
    }
}
