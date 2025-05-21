using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.UserDto.Request
{
    public class UpdateUser
    {
        public string Name { get; set; }
        public string? Contact { get; set; }
        public string Email { get; set; }
    }
    public class UpdateUserByGestor
    {
        public string? Role { get; set; }
    }
}
