using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class SubCompanyModel
    {
        public SubCompanyModel()
        {
            CompanyUsers = new List<CompanyUserModel>(); // Inicializando a lista
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; }
        public bool Deleted { get; set; } = false;
        public int BusinessEntityId { get; set; }
        public BusinessEntity BusinessEntity { get; set; }

        // Chave estrangeira para Company
        public int CompanyId { get; set; }
        public CompanyModel Company { get; set; }

        // Relacionamento com CompanyUserModel
        public List<CompanyUserModel> CompanyUsers { get; set; }
    }

}
