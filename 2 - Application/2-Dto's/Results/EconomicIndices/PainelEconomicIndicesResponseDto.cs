using _2___Application._2_Dto_s.TotalizerClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Results.EconomicIndices
{

    public class PainelProfitabilityResponseDto
    {
        public ProfitabilityGroupedDto Profitability { get; set; }
    }
    public class ProfitabilityGroupedDto
    {
        public List<ProfitabilityResponseDto> Months { get; set; }

    }

    public class ProfitabilityResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal MargemBruta { get; set; }
        public decimal MargemEBITDA { get; set; }
        public decimal MargemOperacional { get; set; }
        public decimal MargemNOPAT { get; set; }
        public decimal MargemLiquida { get; set; }

    }

    public class PainelRentabilityResponseDto
    {
        public RentabilityGroupedDto Rentability { get; set; }
    }
    public class RentabilityGroupedDto
    {
        public List<RentabilityResponseDto> Months { get; set; }

    }


    public class RentabilityResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal ROI { get; set; }
        public decimal LiquidoMensalROE { get; set; }
        public decimal LiquidoInicioROE { get; set; }

    }

    public class PainelReturnExpectationResponseDto
    {
        public ReturnExpectationGroupedDto ReturnExpectation { get; set; }
    }
    public class ReturnExpectationGroupedDto
    {
        public List<ReturnExpectationResponseDto> Months { get; set; }

    }


    public class ReturnExpectationResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal ROIC { get; set; }
        public decimal KE { get; set; }
        public decimal CriacaoValor { get; set; }

    }
    public class PainelEBITDAResponseDto
    {
        public EBITDAGroupedDto EBITDA { get; set; }
    }
    public class EBITDAGroupedDto
    {
        public List<EBITDAResponseDto> Months { get; set; }

    }
    public class EBITDAResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal LucroOperacionalAntesDoResultadoFinanceiro { get; set; }
        public decimal DespesasDepreciacao { get; set; }
        public decimal EBITDA { get; set; }
    }
    public class PainelNOPATResponseDto
    {
        public NOPATGroupedDto NOPAT { get; set; }
    }
    public class NOPATGroupedDto
    {
        public List<NOPATResponseDto> Months { get; set; }

    }
    public class NOPATResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal LucroOperacionalAntes { get; set; }
        public decimal MargemOperacionalDRE { get; set; }
        public decimal ProvisaoIRPJCSLL { get; set; }
        public decimal MargemNOPAT { get; set; }
        public decimal NOPAT { get; set; }
 

    }
}
