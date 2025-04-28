using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class 
        GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;
        public bool Deleted { get; set; } = false;
        public int BusinessEntityId { get; set; }
        public List<CompanyUserModel> CompanyUsers { get; set; }
        public BusinessEntity BusinessEntity { get; set; }

        public ICollection<CompanyModel> Companies { get; set; } = new List<CompanyModel>();
    }
}
