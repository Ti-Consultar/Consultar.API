using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.AccountPlan
{
    public class AccountPlanResponse
    {

        public int Id { get; set; }
        public GroupSimpleDto Group { get; set; }
        public CompanySimpleDto? Company { get; set; }
        public SubCompanySimpleDto? SubCompany { get; set; }


    }


}
