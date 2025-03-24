using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company.SubCompany
{
   public class InsertSubCompanyDto
    {
        public string Name { get; set; }  // Nome da subempresa
        public DateTime DateCreate { get; set; }  // Data de criação
        public int CompanyId { get; set; }  // ID da empresa à qual a subempresa pertence
        public int UserId { get; set; }  // ID do usuário que será associado a essa subempresa

    }
}
