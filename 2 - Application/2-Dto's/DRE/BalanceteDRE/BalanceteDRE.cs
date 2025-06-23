using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.DRE.BalanceteDRE
{
   public class BalanceteDRE
    {
        public  int DREId { get; set; }
        public  int BalanceteId { get; set; }
        public  List<BalanceteDataListIds> Items { get; set; }
    }

    public class BalanceteDataListIds
    {
        public int BalanceteDataId { get; set; }
       
    }
}
