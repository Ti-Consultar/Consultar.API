using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
   public class BalancoReclassificadoTemplate
    {
        public BalancoReclassificadoTemplate()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeOrder { get; set; }
    }
}
