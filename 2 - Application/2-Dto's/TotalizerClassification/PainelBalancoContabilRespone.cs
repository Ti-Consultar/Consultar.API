using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.TotalizerClassification
{
  public  class PainelBalancoContabilRespone
    {
        public List<MonthPainelContabilRespone> Months { get; set; }
        public List<TotalizerParentRespone> Totalizador { get; set; }

    }
    public class MonthPainelContabilRespone
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public MonthPainelContabilTotalizerRespone MonthPainelContabilTotalizer { get; set; }
        public List<TotalizerParentRespone> Totalizer { get; set; }

    }

    public class MonthPainelContabilTotalizerRespone
    {
        public string Name { get; set; }
        public decimal TotalValue { get; set; }
    

    }
    public class TotalizerParentRespone
    {
        public int Id { get; set; }
        public int TypeOrder { get; set; }
        public string Name { get; set; }
        public decimal TotalValue { get; set; }
        public List<ClassificationRespone> Classifications { get; set; }
    }


    public class ClassificationRespone
    {
        public int Id { get; set; }
        public int TypeOrder { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public List<BalanceteDataResponse> Datas { get; set; }
    }

    public class BalanceteDataResponse
    {
        public int Id { get; set; }
        public int TypeOrder { get; set; }
        public string Name { get; set; }
        public string CostCenter { get; set; }
        public decimal InitialValue { get; set; }
        public decimal CreditValue { get; set; }
        public decimal DebitValue { get; set; }
        public decimal Value { get; set; }
    }

    public class PainelBalancoComparativoResponse
    {
        public PainelBalancoContabilRespone Realizado { get; set; }
        public PainelBalancoContabilRespone Orcado { get; set; }
        public PainelBalancoContabilRespone Variacao { get; set; }
    }

}
