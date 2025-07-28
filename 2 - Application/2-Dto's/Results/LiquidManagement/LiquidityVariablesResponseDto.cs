using _2___Application._2_Dto_s.TotalizerClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Results.LiquidManagement
{
    public class PainelLiquidityManagementResponseDto
    {
        public List<MonthLiquidtyManagementResponse> Months { get; set; }
    }
    public class MonthLiquidtyManagementResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public List<LiquidityVariablesResponseDto>? LiquidityVariables { get; set; }
        public List<CapitalDynamicsResponseDto>? CapitalDynamics { get; set; }
        public List<GrossCashFlowResponseDto>? grossCashFlows { get; set; }
        public List<TurnoverResponseDto>? Turnovers { get; set; }
        public List<LiquidityResponseDto>? Liquiditys { get; set; }
        public List<CapitalStructureResponseDto>? capitalStructures { get; set; }

    }

    public class LiquidityVariablesResponseDto
    {
        public decimal SaldoTesouraria { get; set; }
        public decimal NCG { get; set; }
        public decimal CDG { get; set; }

    }

    public class CapitalDynamicsResponseDto
    {
        public decimal PME { get; set; }
        public decimal PMR { get; set; }
        public decimal PMP { get; set; }
        public decimal CicloFinanceiroOperacional { get; set; }
        public decimal CicloFinanceiroNCG { get; set; }

    }

    public class GrossCashFlowResponseDto
    {
        public decimal EBITIDA { get; set; }
        public decimal MargemEBITIDA { get; set; }
        public decimal VariacaoNCG { get; set; }
        public decimal FluxoCaixaOperacional { get; set; }
        public decimal GeracaoCaixa { get; set; }
        public decimal AumentoReducaoFluxoCaixa { get; set; }

    }

    public class TurnoverResponseDto
    {
        public decimal GiroPME { get; set; }
        public decimal GiroPMR { get; set; }
        public decimal GiroPMP { get; set; }
        public decimal GiroCaixa { get; set; }

    }

    public class LiquidityResponseDto
    {
        public decimal LiquidezCorrente { get; set; }
        public decimal LiquidezSeca { get; set; }
        public decimal LiquidezImediata { get; set; }

    }
    public class CapitalStructureResponseDto
    {
        public decimal TerceirosCurtoPrazo { get; set; }
        public decimal TerceirosLongoPrazo { get; set; }
        public decimal ParticipacaoCapitalTerceiros { get; set; }
        public decimal ParticipacaoCapitalProprio { get; set; }

    }
}
