using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._3_Utils.Email
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string emailAddress, string newPassword);
    }
}
