using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.ValueTree
{
    public class ValueTreeResultDto
    {
        public EconomicViewDto EconomicView { get; set; }
        public FinancialViewDto FinancialView { get; set; }
        public ReturnIndicatorsDto Indicators { get; set; }
    }

    public class EconomicViewDto
    {
        public decimal ReceitaLiquida { get; set; }
        public decimal CustoDespesaVariavel { get; set; }
        public decimal MargemContribuicao { get; set; }

        public decimal DespesasOperacionais { get; set; }
        public decimal OutrosResultadosOperacionais { get; set; }
        public decimal LAJIR { get; set; }

        public decimal Impostos { get; set; }
        public decimal NOPAT { get; set; }
    }

    public class FinancialViewDto
    {
        public decimal Disponivel { get; set; }
        public decimal Clientes { get; set; }
        public decimal Estoques { get; set; }
        public decimal OutrosAtivosOperacionais { get; set; }

        public decimal Fornecedores { get; set; }
        public decimal OutrosPassivosOperacionais { get; set; }

        public decimal RealizavelLongoPrazo { get; set; }
        public decimal ExigivelLongoPrazo { get; set; }
        public decimal AtivosFixos { get; set; }

        public decimal CapitalDeGiro { get; set; }

        public decimal CapitalInvestido { get; set; }
    }

    public class ReturnIndicatorsDto
    {
        public decimal NOPAT { get; set; }
        public decimal CapitalInvestido { get; set; }
        public decimal ROIC { get; set; }

        public decimal WACC { get; set; }
        public decimal SPREAD { get; set; }

        public decimal EVA { get; set; }
    }

}
