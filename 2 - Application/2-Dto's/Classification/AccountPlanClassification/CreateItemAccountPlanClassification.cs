using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification.AccountPlanClassification
{
    public class CreateItemClassification
    {
        public string Name { get; set; }
        public int TypeOrder { get; set; }
        public int TypeClassification { get; set; }
    }

    public class UpdateItemClassification
    {
        public string Name { get; set; }
        public int TypeClassification { get; set; }
    }
}
