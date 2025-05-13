using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class AccountPlansModel
    {

        public AccountPlansModel() { }
        public int Id { get; set; }
        public int GroupId { get; set; }
        public GroupModel Group { get; set; }
        public int? CompanyId { get; set; }
        public CompanyModel? Company { get; set; }
        public int? SubCompanyId { get; set; }
        public SubCompanyModel? SubCompany { get; set; }

        public List<BalanceteModel> Balancetes { get; set; }
    }
}
