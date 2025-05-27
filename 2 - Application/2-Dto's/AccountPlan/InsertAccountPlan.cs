using _3_Domain._1_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.AccountPlan
{
   public class InsertAccountPlan
    {
        public int GroupId { get; set; }
        public int? CompanyId { get; set; }
        public int? SubCompanyId { get; set; }
    }
}
