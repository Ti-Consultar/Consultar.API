using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Classification
{
    public class ListAtivoPainel
    {
        public TotalAtivoCirculante TotalAtivoCirculante { get; set; }
        public TotalLongoPrazo TotalLongoPrazo { get; set; }
        public TotalPermanente TotalPermanente { get; set; }
        public TotalAtivoNaoCirculante TotalAtivoNaoCirculante { get; set; }
        public TotalGeralDoAtivo TotalGeralDoAtivo { get; set; }
    }

    public class TotalAtivoCirculante
    {
        public decimal value { get; set; }
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
    }

    public class TotalLongoPrazo
    {
        public decimal value { get; set; }
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
    }

    public class TotalPermanente
    {
        public decimal value { get; set; }
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
    }

    public class TotalAtivoNaoCirculante
    {
        public decimal value { get; set; }
        public List<BalanceteDataAccountPlanClassificationResponseteste> Classifications { get; set; }
    }

    public class TotalGeralDoAtivo
    {
        public decimal value { get; set; }

    }



    public class BalanceteDataAccountPlanClassificationResponseteste
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string ValueFormatted => Value.ToString("N2", new CultureInfo("pt-BR"));

    }

    public class MonthBalanceteDataAccountPlanClassificationResponseteste
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public List<BalanceteDataAccountPlanClassificationResponseteste>? Classifications { get; set; }

    }

    public class PainelMensalAtivoResponse
    {
        public int Year { get; set; }
        public List<MesAtivoPainel> Meses { get; set; }
    }

    public class MesAtivoPainel
    {
        public string Month { get; set; }
        public TotalAtivoCirculante TotalAtivoCirculante { get; set; }
        public TotalLongoPrazo TotalLongoPrazo { get; set; }
        public TotalPermanente TotalPermanente { get; set; }
        public TotalAtivoNaoCirculante TotalAtivoNaoCirculante { get; set; }
        public TotalGeralDoAtivo TotalGeralDoAtivo { get; set; }
    }
}
