using _2___Application._2_Dto_s.Results.EconomicIndices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.Results.CILeEC
{
    public class PainelCILeECResponseDto
    {
        public CILeECGroupedDto CILeEC { get; set; }
    }


    public class CILeECGroupedDto
    {
        public List<CILeECResponseDto> Months { get; set; }
    }





    public class CILeECResponseDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
       
        public CILResponseDto Cil { get; set; }
        public ECResponseDto EstruturaDeCapital { get; set; }

    }

    public class CILResponseDto
    {
        public string Name { get; set; }
        public decimal Disponibilidades { get; set; }
        public decimal Clientes { get; set; }
        public decimal Estoques { get; set; }
        public decimal OutrosAtivosOperacionais { get; set; }
        public decimal Fornecedores { get; set; }
        public decimal ObrigacoesTributariasTrabalhistas { get; set; }
        public decimal OutrosPassivosOperacionais { get; set; }
        public decimal NCG { get; set; }
        public decimal RealizavelLongoPrazo { get; set; }
        public decimal ExigivelALongoPrazoOperacional { get; set; }
        public decimal AtivosFixos { get; set; }
        public decimal CapitalInvestidoLiquido { get; set; }

    }
    public class ECResponseDto
    {
        public string Name { get; set; }
        public decimal Emprestimos { get; set; }
        public decimal PosicaoFinanceiraCurtoPrazo { get; set; }
        public decimal ExigivelaLongoPrazoFinanceiro { get; set; }
        public decimal PosicaoFinanceiraTerceiros { get; set; }
        public decimal PatrimonioLiquido { get; set; }
        public decimal EstruturaDeCapital { get; set; }

    }
    public class PainelCILeECComparativoResponseDto
    {
        public List<CILeECResponseComparativoDto> Months { get; set; } = new();
    }

    public class CILeECResponseComparativoDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }

        public CILResponseDto RealizadoCIL { get; set; } = new();
        public CILResponseDto OrcadoCIL { get; set; } = new();
        public CILResponseDto VariacaoCIL { get; set; } = new();

        public ECResponseDto RealizadoEC { get; set; } = new();
        public ECResponseDto OrcadoEC { get; set; } = new();
        public ECResponseDto VariacaoEC { get; set; } = new();
    }




}
