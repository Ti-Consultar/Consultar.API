using _3_Domain._2_Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class InteractionModel
    {
        public InteractionModel()
        {
            
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public UserModel User { get; set; }
        public EAction Action { get; set; }
        public DateTime DateAction { get; set; }
        public int BalanceteId { get; set; }
        public BalanceteModel Balancete { get; set; }
        public int? BalanceteDataId { get; set; }
        public BalanceteDataModel? BalanceteData { get; set; }
    }
}
