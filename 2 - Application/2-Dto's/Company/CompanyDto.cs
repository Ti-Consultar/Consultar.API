

using _2___Application._2_Dto_s.Company.SubCompany;

namespace _2___Application._2_Dto_s.Company
{
    public class CompanyDto
    {
        public int Id { get; set; }  
        public string Name { get; set; } 
        public DateTime DateCreate { get; set; }  

        public List<SubCompanyDto> SubCompanies { get; set; } 


    }

}
