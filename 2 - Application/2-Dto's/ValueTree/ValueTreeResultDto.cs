using System;

namespace _2___Application._2_Dto_s.ValueTree
{
    public class ValueTreeResultDto
    {
        public ValueTreeYearMonthDto ValueTreeYearMonth { get; set; }
        public EconomicViewDto EconomicView { get; set; }
        public FinancialViewDto FinancialView { get; set; }
        public ReturnIndicatorsDto Indicators { get; set; }
    }
    public class ValueTreeComparativoResponse
    {
        public ValueTreeResultDto Realizado { get; set; }
        public ValueTreeResultDto Orcado { get; set; }
    }

    public class ValueTreeYearMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class EconomicViewDto
    {
        public decimal ReceitaLiquida { get; set; }
        public decimal ReceitaLiquidaAcumulado { get; set; }

        public decimal CustoDespesaVariavel { get; set; }
        public decimal CustoDespesaVariavelAcumulado { get; set; }

        public decimal MargemContribuicao { get; set; }
        public decimal MargemContribuicaoAcumulado { get; set; }

        public decimal DespesasOperacionais { get; set; }
        public decimal DespesasOperacionaisAcumulado { get; set; }

        public decimal OutrosResultadosOperacionais { get; set; }
        public decimal OutrosResultadosOperacionaisAcumulado { get; set; }

        public decimal LAJIR { get; set; }
        public decimal LAJIRAcumulado { get; set; }

        public decimal Impostos { get; set; }
        public decimal ImpostosAcumulado { get; set; }

        public decimal NOPAT { get; set; }
        public decimal NOPATAcumulado { get; set; }
    }

    public class FinancialViewDto
    {
        public decimal Disponivel { get; set; }
        public decimal DisponivelAcumulado { get; set; }

        public decimal Clientes { get; set; }
        public decimal ClientesAcumulado { get; set; }

        public decimal Estoques { get; set; }
        public decimal EstoquesAcumulado { get; set; }

        public decimal OutrosAtivosOperacionais { get; set; }
        public decimal OutrosAtivosOperacionaisAcumulado { get; set; }

        public decimal Fornecedores { get; set; }
        public decimal FornecedoresAcumulado { get; set; }

        public decimal OutrosPassivosOperacionais { get; set; }
        public decimal OutrosPassivosOperacionaisAcumulado { get; set; }

        public decimal RealizavelLongoPrazo { get; set; }
        public decimal RealizavelLongoPrazoAcumulado { get; set; }

        public decimal ExigivelLongoPrazo { get; set; }
        public decimal ExigivelLongoPrazoAcumulado { get; set; }

        public decimal AtivosFixos { get; set; }
        public decimal AtivosFixosAcumulado { get; set; }

        public decimal CapitalDeGiro { get; set; }
        public decimal CapitalDeGiroAcumulado { get; set; }

        public decimal CapitalInvestido { get; set; }
        public decimal CapitalInvestidoAcumulado { get; set; }
    }

    public class ReturnIndicatorsDto
    {
        public decimal NOPAT { get; set; }
        public decimal NOPATAcumulado { get; set; }

        public decimal CapitalInvestido { get; set; }
        public decimal CapitalInvestidoAcumulado { get; set; }

        public decimal ROIC { get; set; }
        public decimal ROICAcumulado { get; set; }

        public decimal WACC { get; set; }
        public decimal WACCAcumulado { get; set; }

        public decimal SPREAD { get; set; }
        public decimal SPREADAcumulado { get; set; }

        public decimal EVA { get; set; }
        public decimal EVA_Acumulado { get; set; }
    }
}
