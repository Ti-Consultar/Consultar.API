using _2___Application._2_Dto_s.TotalizerClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Results.EconomicIndices
{
   public class PainelEconomicIndicesResponseDto
    {
        public List<MonthEconomicIndicesResponse> Months { get; set; }
    }
    public class MonthEconomicIndicesResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public List<ProfitabilityResponseDto>? Profitabilities { get; set; }
        public List<RentabilityResponseDto>?rentabilities { get; set; }
        public List<ReturnExpectationResponseDto>? returnExpectations { get; set; }
        public List<EBITDAResponseDto>? EBITDA { get; set; }
        public List<NOPATResponseDto>? NOPAT { get; set; }

    }

    public class ProfitabilityResponseDto
    {
        public decimal MargemBruta { get; set; }
        public decimal MargemEBITDA { get; set; }
        public decimal MargemOperacional { get; set; }
        public decimal MargemNOPAT { get; set; }
        public decimal MargemLiquida { get; set; }

    }

    public class RentabilityResponseDto
    {
        public decimal ROI { get; set; }
        public decimal LiquidoMensalROE { get; set; }
        public decimal LiquidoInicioROE { get; set; }

    }

    public class ReturnExpectationResponseDto
    {
        public decimal ROIC { get; set; }
        public decimal KE { get; set; }
        public decimal CriacaoValor { get; set; }

    }

    public class EBITDAResponseDto
    {
        public decimal LucroOperacional { get; set; }
        public decimal DespesasDepreciacao { get; set; }
        public decimal CustoDepreciacao { get; set; }
        public decimal EBITDA { get; set; }
        public decimal MargemEBITDA { get; set; }

    }

    public class NOPATResponseDto
    {
        public decimal LucroOperacionalAntes { get; set; }
        public decimal MargemOperacional { get; set; }
        public decimal ProvisaoIRPJCSLL { get; set; }
        public decimal MargemNOPAT { get; set; }

    }
}
