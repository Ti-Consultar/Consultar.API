using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company.SubCompany
{
    public class SubCompanyForUserDto
    {
        public int SubCompanyId { get; set; }  // ID da subempresa
        public string SubCompanyName { get; set; }  // Nome da subempresa
        public int CompanyId { get; set; }  // ID da empresa
        public string CompanyName { get; set; }  // Nome da empresa
    }

}
