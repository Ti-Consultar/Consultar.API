using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class DREModel
    {
        public DREModel()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequential { get; set; }
        public int ClassificationId { get; set; }
        public ClassificationModel Classification { get; set; }
        public AccountPlansModel AccountPlan { get; set; }
        public int AccountPlanId { get; set; }
    }
}
