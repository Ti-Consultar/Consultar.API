using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._2_Dto_s.DashBoards
{
    public class DashBoardDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal ReceitaLiquida { get; set; }
        public decimal VariacaoReceitaLiquida { get; set; }
        public decimal MargemBruta { get; set; }
        public decimal VariacaoMargemBruta { get; set; }
        public decimal MargemLiquida { get; set; }
        public decimal VariacaoMargemLiquida { get; set; }
    }

    public class DashBoardGestaoPrazoMedioDto
    {
        public string Name { get; set; }
        public int DateMonth { get; set; }
        public decimal Clientes { get; set; }
        public decimal Estoques { get; set; }
        public decimal Fornecedores { get; set; }
  
        public decimal GiroPME { get; set; }
        public decimal GiroPMR { get; set; }
        public decimal GiroPMP { get; set; }
        public decimal GiroCaixa { get; set; }

    }
}
