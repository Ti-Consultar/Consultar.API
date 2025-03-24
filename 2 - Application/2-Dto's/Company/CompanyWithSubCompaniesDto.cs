using _2___Application._2_Dto_s.Company.SubCompany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company
{
    public class CompanyWithSubCompaniesDto
    {
        public int CompanyId { get; set; }  // ID da empresa
        public string CompanyName { get; set; }  // Nome da empresa
        public List<SubCompanyDto> SubCompanies { get; set; }  // Lista de subempresas associadas
    }

}
