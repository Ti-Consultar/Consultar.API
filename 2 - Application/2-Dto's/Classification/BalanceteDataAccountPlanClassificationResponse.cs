using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification
{
  public  class BalanceteDataAccountPlanClassificationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
         public string ValueFormatted => Value.ToString("N2", new CultureInfo("pt-BR"));

    }

    public class MonthBalanceteDataAccountPlanClassificationResponse
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public List<BalanceteDataAccountPlanClassificationResponse>? Classifications { get; set; }

    }
}
