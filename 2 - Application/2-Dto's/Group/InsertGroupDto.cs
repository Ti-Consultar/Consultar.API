using _2___Application._2_Dto_s.BusinesEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Group
{
    public class InsertGroupDto
    {
        public string Name { get; set; }
        public int UserId { get; set; } 
        public InsertBusinessEntityDto BusinessEntity { get; set; } 
    }
}
