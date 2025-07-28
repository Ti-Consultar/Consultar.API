using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Passivo
{
   public class BondPassivoBalanceteData
    {
        public int AccountPlansId { get; set; }
        public int BalanceteId { get; set; }
        public int PassivoId { get; set; }
        public List<BalanceteDataSimpleDto> ItensBalanceteData { get; set; }
    }
    public  class BalanceteDataSimpleDto
    {
        public int BalanceteDataId { get; set; }
    }

}
