using _2___Application._2_Dto_s.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.UserDto.Response
{
   public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<CompanyDto> Companies { get; set; }  // Lista de empresas associadas
    }
}
