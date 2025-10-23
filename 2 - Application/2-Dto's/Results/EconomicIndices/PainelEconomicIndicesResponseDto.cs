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

    // 📦 DTO principal do painel
    public class PainelRentabilityComparativoResponseDto
    {
        public List<RentabilityComparativoMesDto> Months { get; set; } = new();
    }

    // 📦 DTO de cada mês
    public class RentabilityComparativoMesDto
    {
        public string Name { get; set; } = string.Empty;
        public int DateMonth { get; set; }

        public RentabilityItemDto Realizado { get; set; } = new();
        public RentabilityItemDto Orcado { get; set; } = new();
        public RentabilityVariacaoDto Variacao { get; set; } = new();
    }

    // 📦 DTO dos valores realizados ou orçados
    public class RentabilityItemDto
    {
        public decimal ROI { get; set; }
        public decimal LiquidoMensalROE { get; set; }
        public decimal LiquidoInicioROE { get; set; }
    }

    // 📦 DTO das variações entre realizado e orçado
    public class RentabilityVariacaoDto
    {
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
    // DTO do item Realizado ou Orçado
    public class ReturnExpectationItemDto
    {
        public decimal ROIC { get; set; }
        public decimal KE { get; set; }
        public decimal CriacaoValor { get; set; }
    }

    // DTO da variação
    public class ReturnExpectationVariacaoDto
    {
        public decimal ROIC { get; set; }
        public decimal CriacaoValor { get; set; }
        public decimal VariacaoPercentualCriacaoValor { get; set; }
    }

    // DTO por mês
    public class ReturnExpectationComparativoMesDto
    {
        public string Name { get; set; } = string.Empty;
        public int DateMonth { get; set; }

        public ReturnExpectationItemDto Realizado { get; set; } = new ReturnExpectationItemDto();
        public ReturnExpectationItemDto Orcado { get; set; } = new ReturnExpectationItemDto();
        public ReturnExpectationVariacaoDto Variacao { get; set; } = new ReturnExpectationVariacaoDto();
    }

    // DTO do painel completo
    public class PainelReturnExpectationComparativoResponseDto
    {
        public List<ReturnExpectationComparativoMesDto> Months { get; set; } = new List<ReturnExpectationComparativoMesDto>();
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


    public class PainelEBITDAComparativoResponseDto
    {
        public List<EBITDAComparativoMesDto> Months { get; set; } = new();
    }

    public class EBITDAComparativoMesDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }

        public EBITDAItemDto Realizado { get; set; } = new();
        public EBITDAItemDto Orcado { get; set; } = new();
        public EBITDAItemDto Variacao { get; set; } = new();
    }

    public class EBITDAItemDto
    {
        public decimal LucroAntesFinanceiro { get; set; }
        public decimal Depreciacao { get; set; }
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

    public class PainelNOPATComparativoResponseDto
    {
        public List<NOPATComparativoMesDto> Months { get; set; } = new();
    }

    public class NOPATComparativoMesDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }

        public NOPATItemDto Realizado { get; set; } = new();
        public NOPATItemDto Orcado { get; set; } = new();
        public NOPATItemDto Variacao { get; set; } = new();
    }

    public class NOPATItemDto
    {
        // Mantém mesmos campos do JSON que você mostrou
        public decimal LucroOperacionalAntes { get; set; }          // lucro antes do resultado financeiro
        public decimal MargemOperacionalDRE { get; set; }           // margem operacional do DRE (em %)
        public decimal ProvisaoIRPJCSLL { get; set; }               // provisões (IRPJ + CSLL)
        public decimal MargemNOPAT { get; set; }                    // margem NOPAT (em %)
        public decimal NOPAT { get; set; }                          // valor do NOPAT

        // campo útil para variação percentual (aplica-se normalmente à seção Variacao)
        public decimal VariacaoPercentual { get; set; }
    }


}
