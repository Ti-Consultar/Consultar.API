using _2___Application._2_Dto_s.Results.CILeEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Results.OperationalEfficiency
{
    public class PainelOperationalEfficiencyResponseDto
    {
        public OperationalEfficiencyGroupedDto OperationalEfficiency { get; set; }
    }


    public class OperationalEfficiencyGroupedDto
    {
        public List<OperationalEfficiencyResponseDto> Months { get; set; }
    }


    public class OperationalEfficiencyResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal ReceitasLiquidas { get; set; }
        public decimal CustosDespesas { get; set; }
        public decimal EBITDA { get; set; }
        public decimal MargemEBITDA { get; set; }
        public decimal LucroOperacionalAntesJurosImpostos { get; set; }
        public decimal ResultadoFinanceiro { get; set; }
        public decimal Impostos { get; set; }
        public decimal LucroLiquido { get; set; }
        public decimal NOPAT { get; set; }
        public decimal MargemNOPAT { get; set; }
        public decimal Disponivel { get; set; }
        public decimal Clientes { get; set; }
        public decimal Estoques { get; set; }
        public decimal Fornecedores { get; set; }
        public decimal NCGTotal { get; set; }
        public decimal NCGCEF { get; set; }//NCG Clientes + estoques - fornecedores
        public decimal InvestimentosAtivosFixos { get; set; }
        public decimal CapitalInvestidoLiquido { get; set; }
        public decimal CapitalTurnover { get; set; }
        public decimal ROIC { get; set; }
        public decimal WACC { get; set; }
        public decimal EVASPREAD { get; set; }
        public decimal EVA { get; set; }

    }
}
