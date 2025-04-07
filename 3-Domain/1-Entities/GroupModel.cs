using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;

       // public ICollection<CompanyModel> Companies { get; set; } = new List<CompanyModel>();
    }
}
