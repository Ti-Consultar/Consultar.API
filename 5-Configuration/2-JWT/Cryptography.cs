using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace _5_Configuration._2_JWT
{
    public static class Cryptography
    {
        public static string EncryptPassword(this string password)
        {
            var hash = SHA384.Create();
            var encoding = new ASCIIEncoding();
            var array = encoding.GetBytes(password);

            array = hash.ComputeHash(array);
            var strHexa = new StringBuilder();

            foreach (var item in array)
                strHexa.Append(item.ToString("X2"));

            return strHexa.ToString();
        }
    }
}
