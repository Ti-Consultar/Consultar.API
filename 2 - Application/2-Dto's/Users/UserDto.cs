using _2___Application._2_Dto_s.Invitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Users
{
   public class UserListDto
    {
        public int Id { get; set; }
        public bool UserLogado { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public PermissionDto Permission { get; set; }
    }
}
