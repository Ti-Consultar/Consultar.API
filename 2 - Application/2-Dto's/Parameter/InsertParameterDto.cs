using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Parameter
{
    public class InsertParameterDto
    {
        public int AccountPlanId { get; set; }
        public string Name { get; set; }
        public decimal ParameterValue { get; set; }
        public int ParameterYear { get; set; }
    }

    public class ParameterResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParameterYear { get; set; }
        public decimal ParameterValue { get; set; }
    }
    public class UpdateParameterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParameterYear { get; set; }
        public decimal ParameterValue { get; set; }
    }

}


