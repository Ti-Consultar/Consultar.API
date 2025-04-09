using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class CompanyModel
    {
        public CompanyModel()
        {

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; }

        public int GroupId { get; set; }
        public GroupModel Group { get; set; } // Navegação opcional, mas recomendada

        // Relacionamento Muitos-para-Muitos com Usuários
        public List<CompanyUserModel> CompanyUsers { get; set; }

        // Relacionamento 1:N com SubCompanies
        public List<SubCompanyModel> SubCompanies { get; set; }
    }

}
