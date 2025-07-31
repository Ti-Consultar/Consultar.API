using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class ParameterModel
    {
        public ParameterModel()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int ParameterYear { get; set; }
        public decimal ParameterValue { get; set; }
        public int AccountPlansId { get; set; }
        public AccountPlansModel AccountPlans { get; set; }
    }
}
