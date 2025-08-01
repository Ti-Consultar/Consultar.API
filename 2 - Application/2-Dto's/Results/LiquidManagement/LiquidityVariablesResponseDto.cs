using _2___Application._2_Dto_s.TotalizerClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Results.LiquidManagement
{
# region PainelLiquidityManagementResponseDto

    public class PainelLiquidityManagementResponseDto
    {
        public LiquidityVariablesGroupedDto LiquidityVariables { get; set; }
    }


    public class LiquidityVariablesGroupedDto
    {
        public List<LiquidityMonthlyDto> Months { get; set; }
    }

    public class LiquidityMonthlyDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal SaldoTesouraria { get; set; }
        public decimal NCG { get; set; }
        public decimal CDG { get; set; }
        public decimal? IndiceDeLiquidez { get; set; }
    }
    #endregion

    public class PainelCapitalDynamicsResponseDto
    {
        public CapitalDynamicsGroupedDto CapitalDynamics { get; set; }
    }
    public class CapitalDynamicsGroupedDto
    {
        public List<CapitalDynamicsResponseDto> Months { get; set; }

    }

    public class CapitalDynamicsResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal PME { get; set; }
        public decimal PMR { get; set; }
        public decimal PMP { get; set; }
        public decimal CicloFinanceiroDasOperacoesPrincipais { get; set; }
        public decimal CicloFinanceiroNCG { get; set; }

    }

    public class PainelGrossCashFlowResponseDto
    {
        public GrossCashFlowGroupedDto GrossCashFlows { get; set; }
    }
    public class GrossCashFlowGroupedDto
    {
        public List<GrossCashFlowResponseDto> Months { get; set; }

    }

    public class GrossCashFlowResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal EBITIDA { get; set; }
        public decimal MargemEBITIDA { get; set; }
        public decimal VariacaoNCG { get; set; }
        public decimal FluxoCaixaOperacional { get; set; }
        public decimal GeracaoCaixa { get; set; }
        public decimal AumentoReducaoFluxoCaixa { get; set; }

    }


    public class PainelTurnoverResponseDto
    {
        public TurnoverGroupedDto Turnovers { get; set; }
    }
    public class TurnoverGroupedDto
    {
        public List<TurnoverResponseDto> Months { get; set; }

    }
    public class TurnoverResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal GiroPME { get; set; }
        public decimal GiroPMR { get; set; }
        public decimal GiroPMP { get; set; }
        public decimal GiroCaixa { get; set; }

    }

    public class PainelLiquidityResponseDto
    {
        public LiquidityGroupedDto Liquiditys { get; set; }
    }
    public class LiquidityGroupedDto
    {
        public List<LiquidityResponseDto> Months { get; set; }

    }

    public class LiquidityResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal LiquidezCorrente { get; set; }
        public decimal LiquidezSeca { get; set; }
        public decimal LiquidezImediata { get; set; }

    }

    public class PainelCapitalStructureResponseDto
    {
        public CapitalStructureGroupedDto CapitalStructures { get; set; }
    }
    public class CapitalStructureGroupedDto
    {
        public List<CapitalStructureResponseDto> Months { get; set; }

    }


    public class CapitalStructureResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal? TerceirosCurtoPrazo { get; set; }
        public decimal? TerceirosLongoPrazo { get; set; }
        public decimal? ParticipacaoCapitalTerceiros { get; set; }
        public decimal? ParticipacaoCapitalProprio { get; set; }

    }
}
