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
        public decimal MargemBruta { get; set; }
        public decimal VariacaoMargemBruta { get; set; }
        public decimal MargemLiquida { get; set; }
        public decimal VariacaoMargemLiquida { get; set; }
    }
}
