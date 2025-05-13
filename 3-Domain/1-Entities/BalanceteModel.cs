using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class BalanceteModel
    {
        public BalanceteModel()
        {
            
        }

        public int Id { get; set; }
        public int AccountPlansId { get; set; }
        public AccountPlansModel AccountPlans { get; set; }
        public int DateMonth { get; set; }
        public int YearMonth { get; set; }
        public ESituationBalancete Status { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;

        public List<BalanceteDataModel> BalancetesData { get; set; }

    }
}
