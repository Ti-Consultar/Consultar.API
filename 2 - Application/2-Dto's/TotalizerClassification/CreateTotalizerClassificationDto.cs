using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.TotalizerClassification
{
   public class CreateTotalizerClassificationDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeOrder { get; set; }
        public int accountPlanId { get; set; }
    }
}
