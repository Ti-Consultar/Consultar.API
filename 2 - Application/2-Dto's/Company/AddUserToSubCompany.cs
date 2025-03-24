using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Company
{
    public class AddUserToSubCompanyDto
    {
        public int UserId { get; set; }  // ID do usuário
        public int SubCompanyId { get; set; }  // ID da subempresa
    }

}
