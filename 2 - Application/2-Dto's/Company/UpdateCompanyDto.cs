using _2___Application._2_Dto_s.BusinesEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company
{
    public class UpdateCompanyDto
    {
        public string Name { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public BusinessEntityDto BusinessEntity { get; set; }

    }

    public class UpdateSubCompanyDto
    {
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
    }
}
