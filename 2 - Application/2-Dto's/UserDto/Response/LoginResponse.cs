using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.UserDto.Response
{
    public class LoginResponse
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string Message { get; set; }


    }
}
