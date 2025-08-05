using _2___Application._2_Dto_s.Results.OperationalEfficiency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.CashFlow
{


    public class PainelCashFlowResponseDto
    {
        public CashFlowGroupedDto CashFlow { get; set; }
    }


    public class CashFlowGroupedDto
    {
        public List<CashFlowResponseDto> Months { get; set; }
    }


    public class CashFlowResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal  ReceitaLiquida { get; set; }
        public decimal  CustosOperacionais { get; set; }
        public decimal  DespesasVariaveis { get; set; }
        public decimal  DespesasOperacionais { get; set; }
        public decimal  OutrosResultados { get; set; }
        public decimal  ResultadosFinanceiros { get; set; }
        public decimal  Provisoes { get; set; }
        public decimal  LucroOperacionalLiquido { get; set; }
        public decimal  DepreciacaoAmortizacao { get; set; }
        public decimal  VariacaoNCG { get; set; }
        public decimal  Clientes { get; set; }
        public decimal  Estoques { get; set; }
        public decimal  OutrosAtivosOperacionais { get; set; }
        public decimal  Fornecedores { get; set; }
        public decimal  ObrigacoesTributariasTrabalhistas { get; set; }
        public decimal  OutrosPassivosOperacionais { get; set; }
        public decimal  FluxoDeCaixaOperacional { get; set; }
        public decimal  AtivoNaoCirculante { get; set; }
        public decimal  VariacaoInvetimento { get; set; }
        public decimal  VariacaoImobilizado { get; set; }
        public decimal  FluxoDeCaixaLivre { get; set; }
        public decimal  CaptacoesAmortizacoesFinanceira { get; set; }
        public decimal  PassivoNaoCirculante { get; set; }
        public decimal  VariacaoPatrimonioLiquido { get; set; }
        public decimal  FluxoDeCaixaDaEmpresa { get; set; }
        public decimal  DisponibilidadeInicioDoPeriodo { get; set; }
        public decimal  DisponibilidadeFinalDoPeriodo { get; set; }
    }
}
