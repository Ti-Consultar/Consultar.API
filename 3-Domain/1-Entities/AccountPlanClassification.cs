using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class AccountPlanClassification
    {
        public AccountPlanClassification()
        {
            
        }

        public int Id { get; set; }
        public int AccountPlanId { get; set; }
        public AccountPlansModel AccountPlan { get; set; }
        public string Name { get; set; }
        public int TypeOrder { get; set; }
        public ETypeClassification TypeClassification { get; set; }

        public int TotalizerClassificationId { get; set; }
        public TotalizerClassificationModel TotalizerClassificationTemplate { get; set; }
    }
}
