using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification.AccountPlanClassification
{
    public class AccountPlanClassificationResponse
    {
        public int Id { get; set; }
        public int AccountPlanId { get; set; }
        public string Name { get; set; }
        public string TypeClassification { get; set; }
        public int TypeOrder { get; set; }
    }
}
