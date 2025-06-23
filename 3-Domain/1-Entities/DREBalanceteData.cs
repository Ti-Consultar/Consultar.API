using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class DREBalanceteData
    {
        public DREBalanceteData()
        {
            
        }
        public int Id { get; set; }
        public int DREId { get; set; }
        public int BalanceteDataId { get; set; }
        public int BalanceteId { get; set; }
        public double TotalValue { get; set; }

        // Relacionamentos, se existirem
        public virtual DREModel Dre { get; set; }
        public virtual BalanceteDataModel BalanceteData { get; set; }
        public virtual BalanceteModel Balancete { get; set; }
    }
}
