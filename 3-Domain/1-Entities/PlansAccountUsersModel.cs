using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class PlansAccountUsersModel
    {
        public PlansAccountUsersModel()
        {
            
        }

        public int Id { get; set; }
        public int AccountPlansId { get; set; }
        public AccountPlansModel AccountPlans { get; set; }
        public int BalanceteId { get; set; }
        public BalanceteModel Balancete { get; set; }
    }
}
