using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.ConfigPrincipal
{
    public class MenuResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<MenuSonResponse> Sons { get; set; }
    }
    public class MenuSonResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
