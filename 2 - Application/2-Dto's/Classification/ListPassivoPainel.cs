using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification
{
    public class PainelMensalPassivoResponse
    {
        public int Year { get; set; }
        public List<MesPassivoPainel> Meses { get; set; }
    }

    public class MesPassivoPainel
    {
        public string Month { get; set; }
        public TotalPassivoCirculante TotalPassivoCirculante { get; set; }
        public TotalPassivoNaoCirculante TotalPassivoNaoCirculante { get; set; }
        public TotalPatrimonioLiquido TotalPatrimonioLiquido { get; set; }
        public TotalPassivoCompensado TotalPassivoCompensado { get; set; }
        public TotalGeralDoPassivo TotalGeralDoPassivo { get; set; }
    }

    public class TotalPassivoCirculante
    {
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
        public decimal Value { get; set; }
    }

    public class TotalPassivoNaoCirculante
    {
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
        public decimal Value { get; set; }
    }

    public class TotalPatrimonioLiquido
    {
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
        public decimal Value { get; set; }
    }

    public class TotalPassivoCompensado
    {
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
        public decimal Value { get; set; }
    }

    public class TotalGeralDoPassivo
    {
        public decimal Value { get; set; }
    }

}
