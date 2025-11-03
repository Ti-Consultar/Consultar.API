using _2___Application._2_Dto_s.Painel;
using _2___Application._2_Dto_s.Results.CILeEC;
using _2___Application._2_Dto_s.Results.LiquidManagement;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application.Base;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.Results
{
    public class LiquidManagementService : BaseService
    {
        private readonly ClassificationRepository _repository;
        private readonly AccountPlanClassificationRepository _accountClassificationRepository;
        private readonly BalanceteDataRepository _balanceteDataRepository;
        private readonly BalanceteRepository _balanceteRepository;
        private readonly BudgetRepository _budgetRepository;
        private readonly BudgetDataRepository _budgetDataRepository;
        private readonly TotalizerClassificationRepository _totalizerClassificationRepository;
        private readonly TotalizerClassificationTemplateRepository _totalizerClassificationTemplateRepository;
        private readonly BalancoReclassificadoTemplateRepository _balancoReclassificadoTemplateRepository;
        private readonly BalancoReclassificadoRepository _balancoReclassificadoRepository;
        private readonly AccountPlansRepository _accountPlansRepository;

        public LiquidManagementService(
            ClassificationRepository repository,
            AccountPlanClassificationRepository accountClassificationRepository,
            BalanceteDataRepository balanceteDataRepository,
            BalanceteRepository balanceteRepository,
            BudgetRepository budgetRepository,
            BudgetDataRepository budgetDataRepository,
            TotalizerClassificationRepository _talizerClassificationRepository,
            TotalizerClassificationTemplateRepository totalizerClassificationTemplateRepository,
            BalancoReclassificadoTemplateRepository balancoReclassificadoTemplateRepository,
            BalancoReclassificadoRepository balancoReclassificadoRepository,
            AccountPlansRepository accountPlansRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository;
            _accountClassificationRepository = accountClassificationRepository;
            _balanceteDataRepository = balanceteDataRepository;
            _balanceteRepository = balanceteRepository;
            _budgetRepository = budgetRepository;
            _budgetDataRepository = budgetDataRepository;
            _totalizerClassificationRepository = _talizerClassificationRepository;
            _totalizerClassificationTemplateRepository = totalizerClassificationTemplateRepository;
            _balancoReclassificadoTemplateRepository = balancoReclassificadoTemplateRepository;
            _balancoReclassificadoRepository = balancoReclassificadoRepository;
            _accountPlansRepository = accountPlansRepository;
        }

        #region Variáveis da Liquidez

        public async Task<PainelLiquidityManagementResponseDto> GetLiquidityManagement(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);

            if(painelAtivo is null || painelPassivo is null)
            {
                return new PainelLiquidityManagementResponseDto();
            }
            
          

            var liquidityMonths = new List<LiquidityMonthlyDto>();

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var valorAtivoFinanceiro = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                var valorPassivoFinanceiro = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0;

                var valorAtivoOperacional = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

                var passivoNaoCirculante = monthPassivo?.Totalizer
    .FirstOrDefault(t => t.Name == "Passivo Não Circulante")?.TotalValue ?? 0;

                var patrimonioLiquido = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                var ativoNaoCirculante = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                var ativoFixo = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                var cdg = (passivoNaoCirculante + patrimonioLiquido) - (ativoNaoCirculante + ativoFixo);

                var saldoTesouraria = valorAtivoFinanceiro - valorPassivoFinanceiro;

                var ncg = valorAtivoOperacional  - valorPassivoOperacional;

                decimal? indiceDeLiquidez = ncg != 0 ? (saldoTesouraria / ncg) * 100 : (decimal?)null;





                liquidityMonths.Add(new LiquidityMonthlyDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    SaldoTesouraria = saldoTesouraria,
                    NCG = ncg,
                    CDG = cdg,
                    IndiceDeLiquidez = indiceDeLiquidez
                });
            }

            return new PainelLiquidityManagementResponseDto
            {
                LiquidityVariables = new LiquidityVariablesGroupedDto
                {
                    Months = liquidityMonths
                }
            };
        }

        public async Task<PainelLiquidityManagementComparativoResponseDto> GetLiquidityManagementComparativo(int accountPlanId, int year)
        {
            var painelAtivoRealizado = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);

            var painelPassivoRealizado = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);

            if (painelAtivoRealizado is null || painelAtivoOrcado is null || painelPassivoRealizado is null || painelPassivoOrcado is null)
                return new PainelLiquidityManagementComparativoResponseDto();

            var lista = new List<LiquidityMonthlyComparativoDto>();
            var meses = Enumerable.Range(1, 12).ToList();

            foreach (var mes in meses)
            {
                var ativoR = painelAtivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var ativoO = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var passivoR = painelPassivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var passivoO = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                // ------------ REALIZADO ------------
                var saldoTesourariaR = (ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0) -
                                       (passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0);

                var ncgR = (ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0) -
                           (passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0);

                var cdgR = ((passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante")?.TotalValue ?? 0) +
                            (passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0)) -
                           ((ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0) +
                            (ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0));

                decimal? indiceLiquidezR = ncgR != 0 ? (saldoTesourariaR / ncgR) * 100 : (decimal?)null;

                var realizado = new LiquidityMonthlyDto
                {
                    Name = ativoR?.Name ?? new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    SaldoTesouraria = saldoTesourariaR,
                    NCG = ncgR,
                    CDG = cdgR,
                    IndiceDeLiquidez = indiceLiquidezR
                };

                // ------------ ORÇADO ------------
                var saldoTesourariaO = (ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0) -
                                       (passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0);

                var ncgO = (ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0) -
                           (passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0);

                var cdgO = ((passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante")?.TotalValue ?? 0) +
                            (passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0)) -
                           ((ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0) +
                            (ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0));

                decimal? indiceLiquidezO = ncgO != 0 ? (saldoTesourariaO / ncgO) * 100 : (decimal?)null;

                var orcado = new LiquidityMonthlyDto
                {
                    Name = ativoO?.Name ?? new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    SaldoTesouraria = saldoTesourariaO,
                    NCG = ncgO,
                    CDG = cdgO,
                    IndiceDeLiquidez = indiceLiquidezO
                };

                // ------------ VARIAÇÃO ------------
                var variacao = new LiquidityMonthlyDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    SaldoTesouraria = realizado.SaldoTesouraria - orcado.SaldoTesouraria,
                    NCG = realizado.NCG - orcado.NCG,
                    CDG = realizado.CDG - orcado.CDG,
                    IndiceDeLiquidez = realizado.IndiceDeLiquidez - orcado.IndiceDeLiquidez
                };

                lista.Add(new LiquidityMonthlyComparativoDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            //// ------------ ACUMULADO ------------
            //var acumuladoRealizado = new LiquidityMonthlyDto
            //{
            //    Name = "ACUMULADO",
            //    DateMonth = 13,
            //    SaldoTesouraria = lista.Sum(x => x.Realizado.SaldoTesouraria),
            //    NCG = lista.Sum(x => x.Realizado.NCG),
            //    CDG = lista.Sum(x => x.Realizado.CDG),
            //    IndiceDeLiquidez = lista.Average(x => x.Realizado.IndiceDeLiquidez ?? 0)
            //};

            //var acumuladoOrcado = new LiquidityMonthlyDto
            //{
            //    Name = "ACUMULADO",
            //    DateMonth = 13,
            //    SaldoTesouraria = lista.Sum(x => x.Orcado.SaldoTesouraria),
            //    NCG = lista.Sum(x => x.Orcado.NCG),
            //    CDG = lista.Sum(x => x.Orcado.CDG),
            //    IndiceDeLiquidez = lista.Average(x => x.Orcado.IndiceDeLiquidez ?? 0)
            //};

            //var acumuladoVariacao = new LiquidityMonthlyDto
            //{
            //    Name = "ACUMULADO",
            //    DateMonth = 13,
            //    SaldoTesouraria = acumuladoRealizado.SaldoTesouraria - acumuladoOrcado.SaldoTesouraria,
            //    NCG = acumuladoRealizado.NCG - acumuladoOrcado.NCG,
            //    CDG = acumuladoRealizado.CDG - acumuladoOrcado.CDG,
            //    IndiceDeLiquidez = acumuladoRealizado.IndiceDeLiquidez - acumuladoOrcado.IndiceDeLiquidez
            //};

            //lista.Add(new LiquidityMonthlyComparativoDto
            //{
            //    Name = "ACUMULADO",
            //    DateMonth = 13,
            //    Realizado = acumuladoRealizado,
            //    Orcado = acumuladoOrcado,
            //    Variacao = acumuladoVariacao
            //});

            return new PainelLiquidityManagementComparativoResponseDto
            {
                Months = lista
            };
        }


        public async Task<PainelLiquidityManagementResponseDto> GetLiquidityManagementMonth(int accountPlanId, int year, int month)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);

            if (painelAtivo is null || painelPassivo is null)
            {
                return new PainelLiquidityManagementResponseDto();
            }


            var monthAtivo = painelAtivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == month);

            if (monthAtivo == null || monthPassivo == null)
            {
                return new PainelLiquidityManagementResponseDto
                {
                    LiquidityVariables = new LiquidityVariablesGroupedDto
                    {
                        Months = new List<LiquidityMonthlyDto>() // retorna vazio se mês não encontrado
                    }
                };
            }

            var valorAtivoFinanceiro = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
            var valorPassivoFinanceiro = monthPassivo.Totalizer
                .FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0;

            var valorAtivoOperacional = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
            var valorPassivoOperacional = monthPassivo.Totalizer
                .FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

            var passivoNaoCirculante = monthPassivo.Totalizer
                .FirstOrDefault(t => t.Name == "Passivo Não Circulante")?.TotalValue ?? 0;
            var patrimonioLiquido = monthPassivo.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var ativoNaoCirculante = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
            var ativoFixo = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;



            var cdg = (passivoNaoCirculante + patrimonioLiquido) - (ativoNaoCirculante + ativoFixo);

            var saldoTesouraria = valorAtivoFinanceiro - valorPassivoFinanceiro;

            var ncg = valorAtivoOperacional - valorPassivoOperacional;

            decimal? indiceDeLiquidez = ncg != 0 ? saldoTesouraria / ncg : (decimal?)null;
            var liquidityMonth = new LiquidityMonthlyDto
            {
                Name = monthAtivo.Name,
                DateMonth = month,
                SaldoTesouraria = saldoTesouraria,
                NCG = ncg,
                CDG = cdg,
                IndiceDeLiquidez = indiceDeLiquidez
            };

            return new PainelLiquidityManagementResponseDto
            {
                LiquidityVariables = new LiquidityVariablesGroupedDto
                {
                    Months = new List<LiquidityMonthlyDto> { liquidityMonth }
                }
            };
        }

        #endregion

        #region Dinâmica do Capital de Giro

        public async Task<PainelCapitalDynamicsResponseDto> GetCapitalDynamics(int accountPlanId, int year)
        {

            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            if (painelAtivo is null || painelPassivo is null || painelDRE is null)
            {
                return new PainelCapitalDynamicsResponseDto();
            }



            // Cálculo acumulado da receita líquida (Lucro Líquido do Periodo)
            var lucroLiquidoAcumuladoPorMes = new Dictionary<int, decimal>();
            decimal acumulado = 0;

            foreach (var mes in painelDRE.Months.OrderBy(m => m.DateMonth))
            {
                var lucroMes = mes.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                acumulado += lucroMes;
                lucroLiquidoAcumuladoPorMes[mes.DateMonth] = acumulado;
            }

            var capitalDinamics = new List<CapitalDynamicsResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var dateMonth = monthAtivo.DateMonth;
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var receitaLiquidaAcumulada = lucroLiquidoAcumuladoPorMes.ContainsKey(dateMonth)
                    ? lucroLiquidoAcumuladoPorMes[dateMonth]
                    : 0;

                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var cliente = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedor = monthPassivo.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional - valorPassivoOperacional;

                decimal pMR = 0;
                decimal pME = 0;
                decimal pMP = 0;
                decimal cicloNCG = 0;

                if (receitaLiquidaAcumulada > 0)
                {
                    int multiplicadorDias = dateMonth * 30;
                    pMR = (cliente / receitaLiquidaAcumulada) * multiplicadorDias;
                    pME = (estoque / receitaLiquidaAcumulada) * multiplicadorDias;
                    pMP = (fornecedor / receitaLiquidaAcumulada) * multiplicadorDias;
                    cicloNCG = (ncg / receitaLiquidaAcumulada) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCG = pME + pMR - pMP;

                capitalDinamics.Add(new CapitalDynamicsResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    PME = pME,
                    PMR = pMR,
                    PMP = pMP,
                    CicloFinanceiroDasOperacoesPrincipais = cicloFinanceiroOperacoesPrincipaisNCG,
                    CicloFinanceiroNCG = cicloNCG
                });
            }

            return new PainelCapitalDynamicsResponseDto
            {
                CapitalDynamics = new CapitalDynamicsGroupedDto
                {
                    Months = capitalDinamics
                }
            };
        }

        public async Task<PainelCapitalDynamicsComparativoResponseDto> GetCapitalDynamicsComparativo(int accountPlanId, int year)
        {
            // Painéis realizados
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // Painéis orçados
            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            if (painelAtivo is null || painelPassivo is null || painelDRE is null ||
                painelAtivoOrcado is null || painelPassivoOrcado is null || painelDREOrcado is null)
            {
                return new PainelCapitalDynamicsComparativoResponseDto();
            }

            // Receita Líquida acumulada - Realizado
            var receitaAcumulada = new Dictionary<int, decimal>();
            decimal acumulado = 0;
            foreach (var mes in painelDRE.Months.OrderBy(m => m.DateMonth))
            {
                var valor = mes.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                acumulado += valor;
                receitaAcumulada[mes.DateMonth] = acumulado;
            }

            // Receita Líquida acumulada - Orçado
            var receitaAcumuladaOrcado = new Dictionary<int, decimal>();
            decimal acumuladoOrcado = 0;
            foreach (var mes in painelDREOrcado.Months.OrderBy(m => m.DateMonth))
            {
                var valor = mes.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                acumuladoOrcado += valor;
                receitaAcumuladaOrcado[mes.DateMonth] = acumuladoOrcado;
            }

            var comparativo = new List<CapitalDynamicsComparativoMesDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var dateMonth = monthAtivo.DateMonth;

                // -------------------------
                // REALIZADO
                // -------------------------
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var receita = receitaAcumulada.ContainsKey(dateMonth) ? receitaAcumulada[dateMonth] : 0;

                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var cliente = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedor = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                var ativoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = ativoOperacional - passivoOperacional;

                decimal pMR = 0, pME = 0, pMP = 0, cicloNCG = 0;
                if (receita > 0)
                {
                    int dias = dateMonth * 30;
                    pMR = (cliente / receita) * dias;
                    pME = (estoque / receita) * dias;
                    pMP = (fornecedor / receita) * dias;
                    cicloNCG = (ncg / receita) * dias;
                }

                var cicloFinanceiroOperacoesPrincipais = pME + pMR - pMP;

                var realizado = new CapitalDynamicsResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    PME = pME,
                    PMR = pMR,
                    PMP = pMP,
                    CicloFinanceiroDasOperacoesPrincipais = cicloFinanceiroOperacoesPrincipais,
                    CicloFinanceiroNCG = cicloNCG
                };

                // -------------------------
                // ORÇADO
                // -------------------------
                var monthAtivoOrcado = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var monthPassivoOrcado = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var receitaOrcado = receitaAcumuladaOrcado.ContainsKey(dateMonth) ? receitaAcumuladaOrcado[dateMonth] : 0;

                var estoqueOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var clienteOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedorOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                var ativoOperacionalOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivoOperacionalOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncgOrcado = ativoOperacionalOrcado - passivoOperacionalOrcado;

                decimal pMRO = 0, pMEO = 0, pMPO = 0, cicloNCGO = 0;
                if (receitaOrcado > 0)
                {
                    int dias = dateMonth * 30;
                    pMRO = (clienteOrcado / receitaOrcado) * dias;
                    pMEO = (estoqueOrcado / receitaOrcado) * dias;
                    pMPO = (fornecedorOrcado / receitaOrcado) * dias;
                    cicloNCGO = (ncgOrcado / receitaOrcado) * dias;
                }

                var cicloFinanceiroOperacoesPrincipaisOrcado = pMEO + pMRO - pMPO;

                var orcado = new CapitalDynamicsResponseDto
                {
                    Name = monthAtivoOrcado?.Name ?? monthAtivo.Name,
                    DateMonth = dateMonth,
                    PME = pMEO,
                    PMR = pMRO,
                    PMP = pMPO,
                    CicloFinanceiroDasOperacoesPrincipais = cicloFinanceiroOperacoesPrincipaisOrcado,
                    CicloFinanceiroNCG = cicloNCGO
                };

                // -------------------------
                // VARIAÇÃO
                // -------------------------
                var variacao = new CapitalDynamicsResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    PME = realizado.PME - orcado.PME,
                    PMR = realizado.PMR - orcado.PMR,
                    PMP = realizado.PMP - orcado.PMP,
                    CicloFinanceiroDasOperacoesPrincipais = realizado.CicloFinanceiroDasOperacoesPrincipais - orcado.CicloFinanceiroDasOperacoesPrincipais,
                    CicloFinanceiroNCG = realizado.CicloFinanceiroNCG - orcado.CicloFinanceiroNCG
                };

                comparativo.Add(new CapitalDynamicsComparativoMesDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            return new PainelCapitalDynamicsComparativoResponseDto
            {
                Months = comparativo
            };
        }


        #endregion

        #region Geração de Fluxo de Caixa Bruto

        public async Task<PainelGrossCashFlowResponseDto> GetGrossCashFlow(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            if (painelAtivo is null || painelPassivo is null || painelDRE is null)
                return new PainelGrossCashFlowResponseDto();

            var grossCashFlow = new List<GrossCashFlowResponseDto>();

            // 🔹 Busca o valor da NCG de dezembro do ano anterior
            decimal? ncgMesAnterior = await GetNCGDoMesAnterior(accountPlanId, year);

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal ebitda = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "EBITDA")?.TotalValue ?? 0;
                decimal margemEbitda = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                decimal valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                decimal valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

                decimal ncg = valorAtivoOperacional - valorPassivoOperacional;

                // 🔹 Calcula variação da NCG (janeiro usa dezembro do ano anterior)
                decimal variacaoNCG = (ncgMesAnterior.HasValue)
                    ? ncg - ncgMesAnterior.Value
                    : 0;

                ncgMesAnterior = ncg;

                // 🔹 Fluxo de Caixa Operacional
                decimal fluxoDeCaixaOperacional = ebitda - variacaoNCG;

                // 🔹 Receita líquida do mês
                var receitaMensal = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

                // 🔹 Geração de caixa e variação da margem
                decimal geracaoCaixa = receitaMensal != 0 ? (fluxoDeCaixaOperacional / receitaMensal) * 100 : 0;
                decimal aumentoReducaoFluxoCaixa = margemEbitda != 0 ? geracaoCaixa - margemEbitda : 0;

                grossCashFlow.Add(new GrossCashFlowResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    EBITIDA = ebitda,
                    MargemEBITIDA = margemEbitda,
                    VariacaoNCG = variacaoNCG,
                    FluxoCaixaOperacional = fluxoDeCaixaOperacional,
                    GeracaoCaixa = geracaoCaixa,
                    AumentoReducaoFluxoCaixa = aumentoReducaoFluxoCaixa
                });
            }

            return new PainelGrossCashFlowResponseDto
            {
                GrossCashFlows = new GrossCashFlowGroupedDto
                {
                    Months = grossCashFlow
                }
            };
        }

        public async Task<PainelGrossCashFlowComparativoResponseDto> GetGrossCashFlowComparativo(int accountPlanId, int year)
        {
            // 🔹 Painéis realizados
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // 🔹 Painéis orçados
            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            if (painelAtivo is null || painelPassivo is null || painelDRE is null ||
                painelAtivoOrcado is null || painelPassivoOrcado is null || painelDREOrcado is null)
            {
                return new PainelGrossCashFlowComparativoResponseDto();
            }

            // 🔹 NCG do mês anterior (para ambos)
            decimal? ncgMesAnterior = await GetNCGDoMesAnterior(accountPlanId, year);
            decimal? ncgMesAnteriorOrcado = await GetNCGDoMesAnterior(accountPlanId, year); // pode ser ajustado conforme o fluxo orçado

            var comparativo = new List<GrossCashFlowComparativoMesDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var dateMonth = monthAtivo.DateMonth;

                // =============================
                // 🔸 REALIZADO
                // =============================
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == dateMonth);

                decimal ebitda = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "EBITDA")?.TotalValue ?? 0;
                decimal margemEbitda = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;
                decimal valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                decimal valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                decimal ncg = valorAtivoOperacional - valorPassivoOperacional;

                decimal variacaoNCG = (ncgMesAnterior.HasValue) ? ncg - ncgMesAnterior.Value : 0;
                ncgMesAnterior = ncg;

                decimal fluxoCaixaOperacional = ebitda - variacaoNCG;
                var receitaMensal = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                decimal geracaoCaixa = receitaMensal != 0 ? (fluxoCaixaOperacional / receitaMensal) * 100 : 0;
                decimal aumentoReducaoFluxoCaixa = margemEbitda != 0 ? geracaoCaixa - margemEbitda : 0;

                var realizado = new GrossCashFlowResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    EBITIDA = ebitda,
                    MargemEBITIDA = margemEbitda,
                    VariacaoNCG = variacaoNCG,
                    FluxoCaixaOperacional = fluxoCaixaOperacional,
                    GeracaoCaixa = geracaoCaixa,
                    AumentoReducaoFluxoCaixa = aumentoReducaoFluxoCaixa
                };

                // =============================
                // 🔸 ORÇADO
                // =============================
                var monthAtivoOrcado = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var monthPassivoOrcado = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var monthDREOrcado = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);

                decimal ebitdaOrcado = monthDREOrcado?.Totalizer.FirstOrDefault(t => t.Name == "EBITDA")?.TotalValue ?? 0;
                decimal margemEbitdaOrcado = monthDREOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;
                decimal valorAtivoOperacionalOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                decimal valorPassivoOperacionalOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                decimal ncgOrcado = valorAtivoOperacionalOrcado - valorPassivoOperacionalOrcado;

                decimal variacaoNCGOrcado = (ncgMesAnteriorOrcado.HasValue) ? ncgOrcado - ncgMesAnteriorOrcado.Value : 0;
                ncgMesAnteriorOrcado = ncgOrcado;

                decimal fluxoCaixaOperacionalOrcado = ebitdaOrcado - variacaoNCGOrcado;
                var receitaMensalOrcada = monthDREOrcado?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                decimal geracaoCaixaOrcada = receitaMensalOrcada != 0 ? (fluxoCaixaOperacionalOrcado / receitaMensalOrcada) * 100 : 0;
                decimal aumentoReducaoFluxoCaixaOrcado = margemEbitdaOrcado != 0 ? geracaoCaixaOrcada - margemEbitdaOrcado : 0;

                var orcado = new GrossCashFlowResponseDto
                {
                    Name = monthAtivoOrcado?.Name ?? monthAtivo.Name,
                    DateMonth = dateMonth,
                    EBITIDA = ebitdaOrcado,
                    MargemEBITIDA = margemEbitdaOrcado,
                    VariacaoNCG = variacaoNCGOrcado,
                    FluxoCaixaOperacional = fluxoCaixaOperacionalOrcado,
                    GeracaoCaixa = geracaoCaixaOrcada,
                    AumentoReducaoFluxoCaixa = aumentoReducaoFluxoCaixaOrcado
                };

                // =============================
                // 🔸 VARIAÇÃO (Realizado - Orçado)
                // =============================
                var variacao = new GrossCashFlowResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    EBITIDA = realizado.EBITIDA - orcado.EBITIDA,
                    MargemEBITIDA = realizado.MargemEBITIDA - orcado.MargemEBITIDA,
                    VariacaoNCG = realizado.VariacaoNCG - orcado.VariacaoNCG,
                    FluxoCaixaOperacional = realizado.FluxoCaixaOperacional - orcado.FluxoCaixaOperacional,
                    GeracaoCaixa = realizado.GeracaoCaixa - orcado.GeracaoCaixa,
                    AumentoReducaoFluxoCaixa = realizado.AumentoReducaoFluxoCaixa - orcado.AumentoReducaoFluxoCaixa
                };

                comparativo.Add(new GrossCashFlowComparativoMesDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            return new PainelGrossCashFlowComparativoResponseDto
            {
                Months = comparativo
            };
        }


        // 🔹 Método auxiliar para pegar a NCG de dezembro do ano anterior
        private async Task<decimal?> GetNCGDoMesAnterior(int accountPlanId, int year)
        {
            int anoAnterior = year - 1;

            var painelAtivoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, anoAnterior, 1);
            var painelPassivoAnterior = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, anoAnterior, 2);

            var dezembroAtivo = painelAtivoAnterior?.Months.FirstOrDefault(m => m.DateMonth == 12);
            var dezembroPassivo = painelPassivoAnterior?.Months.FirstOrDefault(m => m.DateMonth == 12);

            if (dezembroAtivo is null || dezembroPassivo is null)
                return null;

            decimal valorAtivoOperacional = dezembroAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
            decimal valorPassivoOperacional = dezembroPassivo.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

            return valorAtivoOperacional - valorPassivoOperacional;
        }


        #endregion

        #region Rotatividade
        public async Task<PainelTurnoverResponseDto> GetTurnover(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // Cálculo acumulado da receita líquida (Lucro Líquido do Periodo)
            var lucroLiquidoAcumuladoPorMes = new Dictionary<int, decimal>();
            decimal acumulado = 0;

            foreach (var mes in painelDRE.Months.OrderBy(m => m.DateMonth))
            {
                var lucroMes = mes.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                acumulado += lucroMes;
                lucroLiquidoAcumuladoPorMes[mes.DateMonth] = acumulado;
            }

            if (painelAtivo is null || painelPassivo is null || painelDRE is null)
            {
                return new PainelTurnoverResponseDto();
            }

            var turnover = new List<TurnoverResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var dateMonth = monthAtivo.DateMonth;

                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var cliente = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedor = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                var receitaLiquidaAcumulada = lucroLiquidoAcumuladoPorMes.ContainsKey(dateMonth)
                  ? lucroLiquidoAcumuladoPorMes[dateMonth]
                  : 0;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional + valorPassivoOperacional;



                decimal pMR = 0, pME = 0, pMP = 0, cicloNCG = 0;

                if (receitaLiquidaAcumulada > 0)
                {
                    int multiplicadorDias = monthAtivo.DateMonth * 30;
                    pMR = (cliente / receitaLiquidaAcumulada) * multiplicadorDias;
                    pME = (estoque / receitaLiquidaAcumulada) * multiplicadorDias;
                    pMP = (fornecedor / receitaLiquidaAcumulada) * multiplicadorDias;
                    cicloNCG = (ncg / receitaLiquidaAcumulada) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCG = pME + pMR - pMP;

                var giroPME = pME != 0 ? 30 / pME : 0;
                var giroPMR = pMR != 0 ? 30 / pMR : 0;
                var giroPMP = pMP != 0 ? 30 / pMP : 0;
                var giroCaixa = cicloFinanceiroOperacoesPrincipaisNCG != 0 ? 30 / cicloFinanceiroOperacoesPrincipaisNCG : 0;

                turnover.Add(new TurnoverResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    GiroPME = giroPME,
                    GiroPMP = giroPMP,
                    GiroPMR = giroPMR,
                    GiroCaixa = giroCaixa,
                });
            }

            return new PainelTurnoverResponseDto
            {
                Turnovers = new TurnoverGroupedDto
                {
                    Months = turnover
                }
            };
        }

        public async Task<PainelTurnoverComparativoResponseDto> GetTurnoverComparativo(int accountPlanId, int year)
        {
            // Painéis realizados
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // Painéis orçados
            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            if (painelAtivo is null || painelPassivo is null || painelDRE is null ||
                painelAtivoOrcado is null || painelPassivoOrcado is null || painelDREOrcado is null)
            {
                return new PainelTurnoverComparativoResponseDto();
            }

            // Receita líquida acumulada - Realizado
            var lucroLiquidoAcumuladoPorMes = new Dictionary<int, decimal>();
            decimal acumulado = 0;
            foreach (var mes in painelDRE.Months.OrderBy(m => m.DateMonth))
            {
                var lucroMes = mes.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                acumulado += lucroMes;
                lucroLiquidoAcumuladoPorMes[mes.DateMonth] = acumulado;
            }

            // Receita líquida acumulada - Orçado
            var lucroLiquidoAcumuladoPorMesOrcado = new Dictionary<int, decimal>();
            decimal acumuladoOrcado = 0;
            foreach (var mes in painelDREOrcado.Months.OrderBy(m => m.DateMonth))
            {
                var lucroMes = mes.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                acumuladoOrcado += lucroMes;
                lucroLiquidoAcumuladoPorMesOrcado[mes.DateMonth] = acumuladoOrcado;
            }

            var comparativoTurnover = new List<TurnoverComparativoMesDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var dateMonth = monthAtivo.DateMonth;

                // Realizado
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var receitaLiquidaAcumulada = lucroLiquidoAcumuladoPorMes.ContainsKey(dateMonth) ? lucroLiquidoAcumuladoPorMes[dateMonth] : 0;
                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var cliente = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedor = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                var ativoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = ativoOperacional + passivoOperacional;

                decimal pMR = 0, pME = 0, pMP = 0, cicloNCG = 0;
                if (receitaLiquidaAcumulada > 0)
                {
                    int multiplicadorDias = dateMonth * 30;
                    pMR = (cliente / receitaLiquidaAcumulada) * multiplicadorDias;
                    pME = (estoque / receitaLiquidaAcumulada) * multiplicadorDias;
                    pMP = (fornecedor / receitaLiquidaAcumulada) * multiplicadorDias;
                    cicloNCG = (ncg / receitaLiquidaAcumulada) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCG = pME + pMR - pMP;
                var realizado = new TurnoverResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    GiroPME = pME != 0 ? 30 / pME : 0,
                    GiroPMR = pMR != 0 ? 30 / pMR : 0,
                    GiroPMP = pMP != 0 ? 30 / pMP : 0,
                    GiroCaixa = cicloFinanceiroOperacoesPrincipaisNCG != 0 ? 30 / cicloFinanceiroOperacoesPrincipaisNCG : 0,
                };

                // Orçado
                var monthAtivoOrcado = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var monthPassivoOrcado = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == dateMonth);
                var receitaLiquidaAcumuladaOrcado = lucroLiquidoAcumuladoPorMesOrcado.ContainsKey(dateMonth) ? lucroLiquidoAcumuladoPorMesOrcado[dateMonth] : 0;
                var estoqueOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var clienteOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedorOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                var ativoOperacionalOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivoOperacionalOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncgOrcado = ativoOperacionalOrcado + passivoOperacionalOrcado;

                decimal pMRO = 0, pMEO = 0, pMPO = 0, cicloNCGO = 0;
                if (receitaLiquidaAcumuladaOrcado > 0)
                {
                    int multiplicadorDias = dateMonth * 30;
                    pMRO = (clienteOrcado / receitaLiquidaAcumuladaOrcado) * multiplicadorDias;
                    pMEO = (estoqueOrcado / receitaLiquidaAcumuladaOrcado) * multiplicadorDias;
                    pMPO = (fornecedorOrcado / receitaLiquidaAcumuladaOrcado) * multiplicadorDias;
                    cicloNCGO = (ncgOrcado / receitaLiquidaAcumuladaOrcado) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCGOrcado = pMEO + pMRO - pMPO;
                var orcado = new TurnoverResponseDto
                {
                    Name = monthAtivoOrcado?.Name ?? monthAtivo.Name,
                    DateMonth = dateMonth,
                    GiroPME = pMEO != 0 ? 30 / pMEO : 0,
                    GiroPMR = pMRO != 0 ? 30 / pMRO : 0,
                    GiroPMP = pMPO != 0 ? 30 / pMPO : 0,
                    GiroCaixa = cicloFinanceiroOperacoesPrincipaisNCGOrcado != 0 ? 30 / cicloFinanceiroOperacoesPrincipaisNCGOrcado : 0,
                };

                // Variação
                var variacao = new TurnoverResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    GiroPME = realizado.GiroPME - orcado.GiroPME,
                    GiroPMR = realizado.GiroPMR - orcado.GiroPMR,
                    GiroPMP = realizado.GiroPMP - orcado.GiroPMP,
                    GiroCaixa = realizado.GiroCaixa - orcado.GiroCaixa
                };

                comparativoTurnover.Add(new TurnoverComparativoMesDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = dateMonth,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            return new PainelTurnoverComparativoResponseDto
            {
                Months = comparativoTurnover
            };
        }


        #endregion

        #region Liquidez
        public async Task<PainelLiquidityResponseDto> GetLiquidity(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            if (painelAtivo is null || painelPassivo is null || painelDRE is null)
            {
                return new PainelLiquidityResponseDto();
            }

            var liquidity = new List<LiquidityResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var ativofinanceiro = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                var ativoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivofinanceiro = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0;
                var passivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                var divisor = (passivofinanceiro + passivoOperacional);
                decimal lc = divisor != 0 ? (ativofinanceiro + ativoOperacional) / divisor : 0;
                decimal ls = divisor != 0 ? ((ativofinanceiro + ativoOperacional - estoque) / divisor) : 0;
                decimal li = divisor != 0 ? (ativofinanceiro / divisor) : 0;

                liquidity.Add(new LiquidityResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    LiquidezCorrente = lc,
                    LiquidezImediata = li,
                    LiquidezSeca = ls
                });
            }

            return new PainelLiquidityResponseDto
            {
                Liquiditys = new LiquidityGroupedDto
                {
                    Months = liquidity
                }
            };
        }

        public async Task<PainelLiquidityComparativoResponseDto> GetLiquidityComparativo(int accountPlanId, int year)
        {
            var painelAtivoRealizado = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivoRealizado = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRERealizado = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            if (painelAtivoRealizado is null || painelPassivoRealizado is null || painelDRERealizado is null ||
                painelAtivoOrcado is null || painelPassivoOrcado is null || painelDREOrcado is null)
            {
                return new PainelLiquidityComparativoResponseDto
                {
                    Months = new List<LiquidityComparativoMesDto>()
                };
            }

            var meses = new List<LiquidityComparativoMesDto>();

            var todosMeses = painelAtivoRealizado.Months.Select(m => m.DateMonth)
                .Union(painelAtivoOrcado.Months.Select(m => m.DateMonth))
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            foreach (var mes in todosMeses)
            {
                // 🔹 Realizado
                var monthAtivoRealizado = painelAtivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoRealizado = painelPassivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthDRERealizado = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == mes);

                var ativoFinanceiroReal = monthAtivoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                var ativoOperacionalReal = monthAtivoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivoFinanceiroReal = monthPassivoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0;
                var passivoOperacionalReal = monthPassivoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var estoqueReal = monthAtivoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                var divisorReal = (passivoFinanceiroReal + passivoOperacionalReal);
                decimal lcReal = divisorReal != 0 ? (ativoFinanceiroReal + ativoOperacionalReal) / divisorReal : 0;
                decimal lsReal = divisorReal != 0 ? ((ativoFinanceiroReal + ativoOperacionalReal - estoqueReal) / divisorReal) : 0;
                decimal liReal = divisorReal != 0 ? (ativoFinanceiroReal / divisorReal) : 0;

                // 🔹 Orçado
                var monthAtivoOrcado = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoOrcado = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthDREOrcado = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                var ativoFinanceiroOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                var ativoOperacionalOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var passivoFinanceiroOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Financeiro")?.TotalValue ?? 0;
                var passivoOperacionalOrcado = monthPassivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var estoqueOrcado = monthAtivoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                var divisorOrcado = (passivoFinanceiroOrcado + passivoOperacionalOrcado);
                decimal lcOrcado = divisorOrcado != 0 ? (ativoFinanceiroOrcado + ativoOperacionalOrcado) / divisorOrcado : 0;
                decimal lsOrcado = divisorOrcado != 0 ? ((ativoFinanceiroOrcado + ativoOperacionalOrcado - estoqueOrcado) / divisorOrcado) : 0;
                decimal liOrcado = divisorOrcado != 0 ? (ativoFinanceiroOrcado / divisorOrcado) : 0;

                // 🔹 Monta os objetos DTO
                var realizado = new LiquidityResponseDto
                {
                    Name = monthAtivoRealizado?.Name ?? monthPassivoRealizado?.Name ?? mes.ToString(),
                    DateMonth = mes,
                    LiquidezCorrente = lcReal,
                    LiquidezSeca = lsReal,
                    LiquidezImediata = liReal
                };

                var orcado = new LiquidityResponseDto
                {
                    Name = monthAtivoOrcado?.Name ?? monthPassivoOrcado?.Name ?? mes.ToString(),
                    DateMonth = mes,
                    LiquidezCorrente = lcOrcado,
                    LiquidezSeca = lsOrcado,
                    LiquidezImediata = liOrcado
                };

                var variacao = new LiquidityResponseDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    LiquidezCorrente = realizado.LiquidezCorrente - orcado.LiquidezCorrente,
                    LiquidezSeca = realizado.LiquidezSeca - orcado.LiquidezSeca,
                    LiquidezImediata = realizado.LiquidezImediata - orcado.LiquidezImediata
                };

                meses.Add(new LiquidityComparativoMesDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            // 🔹 Linha ACUMULADO (médias anuais)
            var acumulado = new LiquidityComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new LiquidityResponseDto
                {
                    LiquidezCorrente = meses.Average(x => x.Realizado?.LiquidezCorrente ?? 0),
                    LiquidezSeca = meses.Average(x => x.Realizado?.LiquidezSeca ?? 0),
                    LiquidezImediata = meses.Average(x => x.Realizado?.LiquidezImediata ?? 0)
                },
                Orcado = new LiquidityResponseDto
                {
                    LiquidezCorrente = meses.Average(x => x.Orcado?.LiquidezCorrente ?? 0),
                    LiquidezSeca = meses.Average(x => x.Orcado?.LiquidezSeca ?? 0),
                    LiquidezImediata = meses.Average(x => x.Orcado?.LiquidezImediata ?? 0)
                }
            };

            acumulado.Variacao = new LiquidityResponseDto
            {
                LiquidezCorrente = acumulado.Realizado.LiquidezCorrente - acumulado.Orcado.LiquidezCorrente,
                LiquidezSeca = acumulado.Realizado.LiquidezSeca - acumulado.Orcado.LiquidezSeca,
                LiquidezImediata = acumulado.Realizado.LiquidezImediata - acumulado.Orcado.LiquidezImediata
            };

            meses.Add(acumulado);

            return new PainelLiquidityComparativoResponseDto
            {
                Months = meses
            };
        }



        #endregion

        #region Estrutura de Capital 
        public async Task<PainelCapitalStructureResponseDto> GetCapitalStructure(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);


            if (painelAtivo is null || painelPassivo is null )
            {
                return new PainelCapitalStructureResponseDto();
            }

            var capitalStructure = new List<CapitalStructureResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal emprestimosACurtoPrazo = monthPassivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;

                decimal passivoNaoCirculanteFinanceiro = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Financeiro")?.TotalValue ?? 0;

                decimal patrimonioLiquido = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                decimal totalTerceiros = emprestimosACurtoPrazo + passivoNaoCirculanteFinanceiro;
                decimal totalGeral = totalTerceiros + patrimonioLiquido;

                decimal? endividamentoTerceirosCurtoPrazo = totalTerceiros != 0
                    ? emprestimosACurtoPrazo / totalTerceiros
                    : (decimal?)null;

                decimal? endividamentoTerceirosLongoPrazo = totalTerceiros != 0
                    ? passivoNaoCirculanteFinanceiro / totalTerceiros
                    : (decimal?)null;

                decimal? participacaoCapitaldeTerceiros = totalGeral != 0
                    ? totalTerceiros / totalGeral
                    : (decimal?)null;

                decimal? participacaoCapitaldeProprio = totalGeral != 0
                    ? patrimonioLiquido / totalGeral
                    : (decimal?)null;

                capitalStructure.Add(new CapitalStructureResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    TerceirosCurtoPrazo = endividamentoTerceirosCurtoPrazo * 100,
                    TerceirosLongoPrazo = endividamentoTerceirosLongoPrazo * 100 ,
                    ParticipacaoCapitalTerceiros = participacaoCapitaldeTerceiros * 100,
                    ParticipacaoCapitalProprio = participacaoCapitaldeProprio * 100
                });
            }

            return new PainelCapitalStructureResponseDto
            {
                CapitalStructures = new CapitalStructureGroupedDto
                {
                    Months = capitalStructure
                }
            };
        }


        public async Task<PainelCapitalStructureComparativoResponseDto> GetCapitalStructureComparativo(int accountPlanId, int year)
        {
            var painelAtivoRealizado = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivoRealizado = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);

            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);

            if (painelAtivoRealizado is null || painelPassivoRealizado is null ||
                painelAtivoOrcado is null || painelPassivoOrcado is null)
            {
                return new PainelCapitalStructureComparativoResponseDto
                {
                    Months = new List<CapitalStructureComparativoMesDto>()
                };
            }

            var meses = new List<CapitalStructureComparativoMesDto>();

            var todosMeses = painelAtivoRealizado.Months.Select(m => m.DateMonth)
                .Union(painelAtivoOrcado.Months.Select(m => m.DateMonth))
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            foreach (var mes in todosMeses)
            {
                // 🔹 Realizado
                var monthAtivoRealizado = painelAtivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoRealizado = painelPassivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);

                decimal emprestimosACurtoPrazoReal = monthPassivoRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;

                decimal passivoNaoCirculanteFinanceiroReal = monthPassivoRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Financeiro")?.TotalValue ?? 0;

                decimal patrimonioLiquidoReal = monthPassivoRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                decimal totalTerceirosReal = emprestimosACurtoPrazoReal + passivoNaoCirculanteFinanceiroReal;
                decimal totalGeralReal = totalTerceirosReal + patrimonioLiquidoReal;

                decimal? endividamentoCurtoPrazoReal = totalTerceirosReal != 0
                    ? emprestimosACurtoPrazoReal / totalTerceirosReal
                    : (decimal?)null;

                decimal? endividamentoLongoPrazoReal = totalTerceirosReal != 0
                    ? passivoNaoCirculanteFinanceiroReal / totalTerceirosReal
                    : (decimal?)null;

                decimal? participacaoCapitalTerceirosReal = totalGeralReal != 0
                    ? totalTerceirosReal / totalGeralReal
                    : (decimal?)null;

                decimal? participacaoCapitalProprioReal = totalGeralReal != 0
                    ? patrimonioLiquidoReal / totalGeralReal
                    : (decimal?)null;

                // 🔹 Orçado
                var monthAtivoOrcado = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoOrcado = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                decimal emprestimosACurtoPrazoOrcado = monthPassivoOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;

                decimal passivoNaoCirculanteFinanceiroOrcado = monthPassivoOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Financeiro")?.TotalValue ?? 0;

                decimal patrimonioLiquidoOrcado = monthPassivoOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                decimal totalTerceirosOrcado = emprestimosACurtoPrazoOrcado + passivoNaoCirculanteFinanceiroOrcado;
                decimal totalGeralOrcado = totalTerceirosOrcado + patrimonioLiquidoOrcado;

                decimal? endividamentoCurtoPrazoOrcado = totalTerceirosOrcado != 0
                    ? emprestimosACurtoPrazoOrcado / totalTerceirosOrcado
                    : (decimal?)null;

                decimal? endividamentoLongoPrazoOrcado = totalTerceirosOrcado != 0
                    ? passivoNaoCirculanteFinanceiroOrcado / totalTerceirosOrcado
                    : (decimal?)null;

                decimal? participacaoCapitalTerceirosOrcado = totalGeralOrcado != 0
                    ? totalTerceirosOrcado / totalGeralOrcado
                    : (decimal?)null;

                decimal? participacaoCapitalProprioOrcado = totalGeralOrcado != 0
                    ? patrimonioLiquidoOrcado / totalGeralOrcado
                    : (decimal?)null;

                // 🔹 Monta os objetos
                var realizado = new CapitalStructureResponseDto
                {
                    Name = monthAtivoRealizado?.Name ?? monthPassivoRealizado?.Name ?? mes.ToString(),
                    DateMonth = mes,
                    TerceirosCurtoPrazo = endividamentoCurtoPrazoReal * 100,
                    TerceirosLongoPrazo = endividamentoLongoPrazoReal * 100,
                    ParticipacaoCapitalTerceiros = participacaoCapitalTerceirosReal * 100,
                    ParticipacaoCapitalProprio = participacaoCapitalProprioReal * 100
                };

                var orcado = new CapitalStructureResponseDto
                {
                    Name = monthAtivoOrcado?.Name ?? monthPassivoOrcado?.Name ?? mes.ToString(),
                    DateMonth = mes,
                    TerceirosCurtoPrazo = endividamentoCurtoPrazoOrcado * 100,
                    TerceirosLongoPrazo = endividamentoLongoPrazoOrcado * 100,
                    ParticipacaoCapitalTerceiros = participacaoCapitalTerceirosOrcado * 100,
                    ParticipacaoCapitalProprio = participacaoCapitalProprioOrcado * 100
                };

                var variacao = new CapitalStructureResponseDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    TerceirosCurtoPrazo = (realizado.TerceirosCurtoPrazo ?? 0) - (orcado.TerceirosCurtoPrazo ?? 0),
                    TerceirosLongoPrazo = (realizado.TerceirosLongoPrazo ?? 0) - (orcado.TerceirosLongoPrazo ?? 0),
                    ParticipacaoCapitalTerceiros = (realizado.ParticipacaoCapitalTerceiros ?? 0) - (orcado.ParticipacaoCapitalTerceiros ?? 0),
                    ParticipacaoCapitalProprio = (realizado.ParticipacaoCapitalProprio ?? 0) - (orcado.ParticipacaoCapitalProprio ?? 0)
                };

                meses.Add(new CapitalStructureComparativoMesDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            // 🔹 Linha ACUMULADO
            var acumulado = new CapitalStructureComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new CapitalStructureResponseDto
                {
                    TerceirosCurtoPrazo = meses.Average(x => x.Realizado?.TerceirosCurtoPrazo ?? 0),
                    TerceirosLongoPrazo = meses.Average(x => x.Realizado?.TerceirosLongoPrazo ?? 0),
                    ParticipacaoCapitalTerceiros = meses.Average(x => x.Realizado?.ParticipacaoCapitalTerceiros ?? 0),
                    ParticipacaoCapitalProprio = meses.Average(x => x.Realizado?.ParticipacaoCapitalProprio ?? 0)
                },
                Orcado = new CapitalStructureResponseDto
                {
                    TerceirosCurtoPrazo = meses.Average(x => x.Orcado?.TerceirosCurtoPrazo ?? 0),
                    TerceirosLongoPrazo = meses.Average(x => x.Orcado?.TerceirosLongoPrazo ?? 0),
                    ParticipacaoCapitalTerceiros = meses.Average(x => x.Orcado?.ParticipacaoCapitalTerceiros ?? 0),
                    ParticipacaoCapitalProprio = meses.Average(x => x.Orcado?.ParticipacaoCapitalProprio ?? 0)
                },
            };

            acumulado.Variacao = new CapitalStructureResponseDto
            {
                TerceirosCurtoPrazo = (acumulado.Realizado.TerceirosCurtoPrazo ?? 0) - (acumulado.Orcado.TerceirosCurtoPrazo ?? 0),
                TerceirosLongoPrazo = (acumulado.Realizado.TerceirosLongoPrazo ?? 0) - (acumulado.Orcado.TerceirosLongoPrazo ?? 0),
                ParticipacaoCapitalTerceiros = (acumulado.Realizado.ParticipacaoCapitalTerceiros ?? 0) - (acumulado.Orcado.ParticipacaoCapitalTerceiros ?? 0),
                ParticipacaoCapitalProprio = (acumulado.Realizado.ParticipacaoCapitalProprio ?? 0) - (acumulado.Orcado.ParticipacaoCapitalProprio ?? 0)
            };

            meses.Add(acumulado);

            return new PainelCapitalStructureComparativoResponseDto
            {
                Months = meses
            };
        }

        #endregion

        #region Dados
        private async Task<PainelBalancoContabilRespone> BuildPainelByTypePassivo(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _balanceteRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationAsync(accountPlanId, typeClassification);

            var classificationTotalizerIds = classifications
                .Where(c => c.TotalizerClassificationId.HasValue)
                .Select(c => c.TotalizerClassificationId.Value)
                .Distinct()
                .ToList();

            var totalizers = await _totalizerClassificationRepository.GetByAccountPlanIdList(accountPlanId, classificationTotalizerIds);
            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);

            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _balanceteDataRepository.GetByAccountPlanClassificationId(accountPlanId);

            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3); // Painel da DRE para pegar o lucro líquido



            decimal acumuladoAnterior = 0;

            var months = balancetes.OrderBy(b => b.DateMonth).Select(balancete =>
            {
                var totalizerResponses = totalizers.Select(totalizer =>
                {
                    var relatedClassifications = classifications
                        .Where(c => c.TotalizerClassificationId == totalizer.Id)
                        .ToList();

                    var classificationsResp = relatedClassifications.Select(classification =>
                    {
                        var datas = balanceteDataClassifications
                            .Where(x => x.AccountPlanClassificationId == classification.Id)
                            .SelectMany(x =>
                                balanceteData
                                    .Where(bd => bd.CostCenter == x.CostCenter && bd.BalanceteId == balancete.Id)
                                    .Select(bd => new BalanceteDataResponse
                                    {
                                        Id = bd.Id,
                                        CostCenter = bd.CostCenter,
                                        Name = bd.Name,
                                        Value = bd.FinalValue
                                    })
                            ).ToList();

                        return new ClassificationRespone
                        {
                            Id = classification.Id,
                            Name = classification.Name,
                            TypeOrder = classification.TypeOrder,
                            Value = datas.Sum(d => d.Value * -1),
                            Datas = datas
                        };
                    }).ToList();

                    return new TotalizerParentRespone
                    {
                        Id = totalizer.Id,
                        Name = totalizer.Name,
                        TypeOrder = totalizer.TypeOrder,
                        Classifications = classificationsResp,
                        TotalValue = classificationsResp.Sum(c => c.Value)
                    };

                }).ToList();

                // Aplicar a regra do resultado acumulado
                var resultadoAcumuladoClass = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Resultado do Exercício Acumulado");

                if (resultadoAcumuladoClass != null)
                {


                    resultadoAcumuladoClass.Value = 0; // Agora sim, pode setar o valor

                    var lucroLiquidoMes = painelDRE.Months
                        .Where(m => m.DateMonth == (int)balancete.DateMonth)
                        .SelectMany(m => m.Totalizer)
                        .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo");

                    var lucroLiquidoValor = lucroLiquidoMes?.TotalValue ?? 0;

                    var resultadoAcumuladoAtual = acumuladoAnterior + lucroLiquidoValor;
                    var data = new BalanceteDataResponse
                    {
                        CostCenter = "0",
                        CreditValue = 0,
                        DebitValue = 0,
                        Name = "Lucro Líquido do Periodo (DRE)",
                        InitialValue = 0,
                        Id = 1,
                        TypeOrder = 1,
                        Value = lucroLiquidoValor
                    };
                    resultadoAcumuladoClass.Datas.Add(data);

                    resultadoAcumuladoClass.Value = resultadoAcumuladoAtual;

                    acumuladoAnterior = resultadoAcumuladoAtual; // Atualiza para o próximo mês

                    // Aplicar a regra do resultado acumulado
                    var patrimonioLiquido = totalizerResponses
                        .FirstOrDefault(c => c.Name == "Patrimônio Liquido");

                    patrimonioLiquido.TotalValue = patrimonioLiquido.TotalValue + resultadoAcumuladoClass.Value;
                }
                var patrimonioLiquidos = totalizerResponses
                       .FirstOrDefault(c => c.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                var contasTransitorias = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Contas Transitórias")?.Value ?? 0;
                var totalPassivoNaoCirculante = totalizerResponses
                   .FirstOrDefault(c => c.Name == "Total Passivo Não Circulante")?.TotalValue ?? 0;

                var totalPassivoCirculante = totalizerResponses
                    .FirstOrDefault(c => c.Name == "Total Passivo Circulante")?.TotalValue ?? 0;

                decimal total = totalPassivoCirculante + totalPassivoNaoCirculante + patrimonioLiquidos;

                return new MonthPainelContabilRespone
                {
                    Id = balancete.Id,
                    Name = balancete.DateMonth.GetDescription(),
                    DateMonth = (int)balancete.DateMonth,
                    Totalizer = totalizerResponses,
                    MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                    {
                        Name = "TOTAL GERAL DO PASSIVO",
                        TotalValue = total
                    }
                };
            }).ToList();

            return new PainelBalancoContabilRespone { Months = months };
        }
        private async Task<PainelBalancoContabilRespone> BuildPainelBalancoReclassificadoByTypeAtivo(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _balanceteRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationAsync(accountPlanId, typeClassification);



            var balancoReclassificados = await _balancoReclassificadoRepository.GetByAccountPlanIdListt(accountPlanId);

            var balancoReclassificadoIds = balancoReclassificados
                 .Where(c => c.TypeOrder >= 1 && c.TypeOrder <= 17)
                 .Distinct()
                 .ToList();

            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);

            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _balanceteDataRepository.GetByAccountPlanClassificationId(accountPlanId);

            var months = balancetes
                .Select(balancete =>
                {
                    var totalizerResponses = balancoReclassificadoIds
                        .Select(totalizer =>
                        {
                            var relatedClassifications = classifications
                                .Where(c => c.BalancoReclassificadoId == totalizer.Id)
                                .ToList();

                            var classificationsResp = relatedClassifications
                                .Select(classification =>
                                {
                                    var datas = balanceteDataClassifications
                                        .Where(x => x.AccountPlanClassificationId == classification.Id)
                                        .SelectMany(x =>
                                            balanceteData
                                                .Where(bd => bd.CostCenter == x.CostCenter && bd.BalanceteId == balancete.Id)
                                                .Select(bd => new BalanceteDataResponse
                                                {
                                                    Id = bd.Id,
                                                    CostCenter = bd.CostCenter,
                                                    Name = bd.Name,
                                                    Value = bd.FinalValue
                                                })
                                        ).ToList();

                                    return new ClassificationRespone
                                    {
                                        Id = classification.Id,
                                        Name = classification.Name,
                                        TypeOrder = classification.TypeOrder,
                                        Value = datas.Sum(d => d.Value),
                                        Datas = datas
                                    };
                                }).ToList();

                            return new TotalizerParentRespone
                            {
                                Id = totalizer.Id,
                                Name = totalizer.Name,
                                TypeOrder = totalizer.TypeOrder,
                                Classifications = classificationsResp,
                                TotalValue = classificationsResp.Sum(c => c.Value)
                            };
                        }).ToList();

                    // Mapas para acesso rápido
                    var totalizerMap = totalizerResponses.ToDictionary(t => t.Name);
                    var classificationMap = totalizerResponses
                        .SelectMany(t => t.Classifications)
                        .ToDictionary(c => c.Name);

                    // Aplicar regras de valor nos totalizadores
                    for (int i = 0; i < 3; i++)
                    {
                        foreach (var totalizer in totalizerResponses.OrderBy(t => t.TypeOrder))
                        {
                            var ruleValue = ApplyBalancoReclassificadoTotalAtivoValueRules(totalizer.Name, totalizerMap, classificationMap);
                            if (ruleValue.HasValue)
                                totalizer.TotalValue = ruleValue.Value;
                        }
                    }

                    // cálculos 

                    decimal ativoFinanceiro = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                    decimal ativoOperacional = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Operacional")?.TotalValue ?? 0;
                    decimal ativoFixo = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Fixo")?.TotalValue ?? 0;
                    decimal ativoNaoCirculante = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                    decimal totalAtivo = ativoFinanceiro + ativoOperacional + ativoFixo + ativoNaoCirculante;

                    var depreciacao = totalizerResponses.FirstOrDefault(a => a.Name == "Depreciação / Amort. Acumulada");

                    if (depreciacao != null)
                    {
                        depreciacao.TotalValue = -Math.Abs(depreciacao.TotalValue);
                    }

                    return new MonthPainelContabilRespone
                    {
                        Id = balancete.Id,
                        Name = balancete.DateMonth.GetDescription(),
                        DateMonth = (int)balancete.DateMonth,
                        Totalizer = totalizerResponses,
                        MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                        {
                            Name = "TOTAL DO ATIVO",
                            TotalValue = totalAtivo
                        }
                    };
                })
                .OrderBy(m => m.DateMonth)
                .ToList();

            return new PainelBalancoContabilRespone
            {
                Months = months
            };
        }


        private async Task<PainelBalancoContabilRespone> BuildPainelBalancoReclassificadoByTypePassivo(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _balanceteRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationAsync(accountPlanId, typeClassification);

            var balancoReclassificados = await _balancoReclassificadoRepository.GetByAccountPlanIdListt(accountPlanId);
            var balancoReclassificadoIds = balancoReclassificados
                .Where(c => c.TypeOrder >= 18 && c.TypeOrder <= 34)
                .Distinct()
                .ToList();

            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _balanceteDataRepository.GetByAccountPlanClassificationId(accountPlanId);
            var painelBalancoContabilPassivo = await BuildPainelByTypePassivo(accountPlanId, year, 2);

            var months = balancetes
                .Select(balancete =>
                {
                    // Monta os totalizadores do mês (Classifications permanecem como estão)
                    var totalizerResponses = balancoReclassificadoIds
                        .Select(totalizer =>
                        {
                            var relatedClassifications = classifications
                                .Where(c => c.BalancoReclassificadoId == totalizer.Id)
                                .ToList();

                            var classificationsResp = relatedClassifications
                                .Select(classification =>
                                {
                                    var datas = balanceteDataClassifications
                                        .Where(x => x.AccountPlanClassificationId == classification.Id)
                                        .SelectMany(x =>
                                            balanceteData
                                                .Where(bd => bd.CostCenter == x.CostCenter && bd.BalanceteId == balancete.Id)
                                                .Select(bd => new BalanceteDataResponse
                                                {
                                                    Id = bd.Id,
                                                    CostCenter = bd.CostCenter,
                                                    Name = bd.Name,
                                                    Value = bd.FinalValue
                                                })
                                        ).ToList();

                                    return new ClassificationRespone
                                    {
                                        Id = classification.Id,
                                        Name = classification.Name,
                                        TypeOrder = classification.TypeOrder,
                                        Value = datas.Sum(d => d.Value),
                                        Datas = datas
                                    };
                                }).ToList();

                            return new TotalizerParentRespone
                            {
                                Id = totalizer.Id,
                                Name = totalizer.Name,
                                TypeOrder = totalizer.TypeOrder,
                                Classifications = classificationsResp,
                                TotalValue = classificationsResp.Sum(c => c.Value)
                            };
                        }).ToList();

                    // Mapas para regras
                    var totalizerMap = totalizerResponses.ToDictionary(t => t.Name);
                    var classificationMap = totalizerResponses
                        .SelectMany(t => t.Classifications)
                        .ToDictionary(c => c.Name);

                    // Regras de valor
                    for (int i = 0; i < 3; i++)
                    {
                        foreach (var totalizer in totalizerResponses.OrderBy(t => t.TypeOrder))
                        {
                            var ruleValue = ApplyBalancoReclassificadoTotalPassivoValueRules(totalizer.Name, totalizerMap, classificationMap);
                            if (ruleValue.HasValue)
                                totalizer.TotalValue = ruleValue.Value;
                        }
                    }

                    // 🔹 Ajuste de "Resultado do Exercício Acumulado" -> "Resultado Acumulado"
                    var resultadodoExercicioAcumulado = painelBalancoContabilPassivo.Months
                        .Where(m => m.DateMonth == (int)balancete.DateMonth)
                        .SelectMany(m => m.Totalizer)
                        .SelectMany(a => a.Classifications)
                        .FirstOrDefault(t => t.Name == "Resultado do Exercício Acumulado");

                    var resultadoAcumulado = totalizerResponses.FirstOrDefault(a => a.Name == "Resultado Acumulado");
                    if (resultadodoExercicioAcumulado != null && resultadoAcumulado != null)
                    {
                        resultadoAcumulado.TotalValue = resultadodoExercicioAcumulado.Value;
                    }

                    // 🔹 Cálculo do PL
                    var patrimonioLiquido = totalizerResponses.FirstOrDefault(a => a.Name == "Patrimônio Liquido");
                    var lucrosPrejuizos = totalizerResponses.FirstOrDefault(a => a.Name == "Lucros / Prejuízos Acumulados")?.TotalValue ?? 0;
                    var resultadoAcumValor = resultadoAcumulado?.TotalValue ?? 0;

                    if (patrimonioLiquido != null)
                    {
                        patrimonioLiquido.TotalValue = patrimonioLiquido.TotalValue + lucrosPrejuizos + (resultadoAcumValor * -1);
                    }

                    // 🔹 NORMALIZAÇÃO: deixa todos os totalizadores POSITIVOS (sem mexer nas Classifications).
                    // Se quiser preservar "Resultado Acumulado" com sinal original, comente a linha do IF e use a condição abaixo.
                    foreach (var t in totalizerResponses)
                    {
                        // Para preservar o sinal do "Resultado Acumulado", troque por:
                        if (!string.Equals(t.Name, "Resultado Acumulado", StringComparison.OrdinalIgnoreCase))
                            t.TotalValue = Math.Abs(t.TotalValue);
                    }

                    // 🔹 Re-leitura após normalização (para garantir que o total do mês use os valores já positivos)
                    decimal passivoFinanceiro = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Financeiro")?.TotalValue ?? 0;
                    decimal passivoOperacional = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Operacional")?.TotalValue ?? 0;
                    decimal patrimonioLiquidoPos = totalizerResponses.FirstOrDefault(a => a.Name == "Patrimônio Liquido")?.TotalValue ?? 0;
                    decimal passivoNaoCirculante = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Não Circulante")?.TotalValue ?? 0;

                    // 🔹 Total do mês (já positivo)
                    decimal totalPassivo = passivoFinanceiro + passivoOperacional + patrimonioLiquidoPos + passivoNaoCirculante;
                    totalPassivo = Math.Abs(totalPassivo);

                    return new MonthPainelContabilRespone
                    {
                        Id = balancete.Id,
                        Name = balancete.DateMonth.GetDescription(),
                        DateMonth = (int)balancete.DateMonth,
                        Totalizer = totalizerResponses,
                        MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                        {
                            Name = "TOTAL DO PASSIVO",
                            TotalValue = totalPassivo
                        }
                    };
                })
                .OrderBy(m => m.DateMonth)
                .ToList();

            return new PainelBalancoContabilRespone
            {
                Months = months
            };
        }


        private async Task<PainelBalancoContabilRespone> BuildPainelByTypeDRE(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _balanceteRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationDREAsync(accountPlanId, typeClassification);
            var totalizersBase = await _totalizerClassificationRepository.GetByAccountPlansId(accountPlanId);
            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);

            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _balanceteDataRepository.GetByAccountPlanClassificationId(accountPlanId);

            var months = new List<MonthPainelContabilRespone>();

            foreach (var balancete in balancetes.OrderBy(b => b.DateMonth))
            {
                // 1. Calcular totalizadores normais
                var totalizerResponses = totalizersBase.Select(totalizer =>
                {
                    var relatedClassifications = classifications
                        .Where(c => c.TotalizerClassificationId == totalizer.Id)
                        .ToList();

                    var classificationsResp = relatedClassifications.Select(classification =>
                    {
                        var datas = balanceteDataClassifications
                            .Where(x => x.AccountPlanClassificationId == classification.Id)
                            .SelectMany(x =>
                                balanceteData
                                    .Where(bd => bd.CostCenter == x.CostCenter && bd.BalanceteId == balancete.Id)
                                    .Select(bd => new BalanceteDataResponse
                                    {
                                        Id = bd.Id,
                                        CostCenter = bd.CostCenter,
                                        Name = bd.Name,
                                        InitialValue = bd.InitialValue,
                                        CreditValue = bd.Credit,
                                        DebitValue = bd.Debit,
                                        Value = bd.FinalValue
                                    })
                            ).ToList();

                        return new ClassificationRespone
                        {
                            Id = classification.Id,
                            Name = classification.Name,
                            TypeOrder = classification.TypeOrder,
                            Value = datas.Sum(a => a.CreditValue - a.DebitValue),
                            Datas = datas
                        };
                    }).ToList();

                    var custoMercadorias = classificationsResp
                        .FirstOrDefault(t => t.Name == "(-) Custos das Mercadorias")?.Value ?? 0;


                    return new TotalizerParentRespone
                    {
                        Id = totalizer.Id,
                        Name = totalizer.Name,
                        TypeOrder = totalizer.TypeOrder,
                        Classifications = classificationsResp,
                        TotalValue = classificationsResp.Sum(a => a.Value)
                    };
                }).ToList();

                // totalizerResponses 
                var receitaOperacionalBruta = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Receita Operacional Bruta")?.TotalValue ?? 0;

                var deducoes = totalizerResponses
                    .FirstOrDefault(t => t.Name == "(-) Deduções da Receita Bruta")?.TotalValue ?? 0;

                var receitaLiquida = totalizerResponses
                    .FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas");
                if (receitaLiquida != null) receitaLiquida.TotalValue = 0;

                var lucroBruto = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Lucro Bruto");
                if (lucroBruto != null) lucroBruto.TotalValue = 0;

                var margemContribuicao = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Margem Contribuição");

                var despesasOperacionais = totalizerResponses
                    .FirstOrDefault(t => t.Name == "(-) Despesas Operacionais");

                var lucroOperacional = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Lucro Operacional");
                if (lucroOperacional != null) lucroOperacional.TotalValue = 0;

                var lucroAntes = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro");
                if (lucroAntes != null) lucroAntes.TotalValue = 0;

                var resultadoAntes = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Resultado do Exercício Antes do Imposto");
                if (resultadoAntes != null) resultadoAntes.TotalValue = 0;

                var lucroLiquido = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo");
                if (lucroLiquido != null) lucroLiquido.TotalValue = 0;

                var ebitda = totalizerResponses
                    .FirstOrDefault(t => t.Name == "EBITDA");

                var nopat = totalizerResponses
                    .FirstOrDefault(t => t.Name == "NOPAT");

                // classificaton
                var custoMercadorias = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;


                // classificaton
                var custoDosServicosPrestados = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;

                var despesasV = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Variáveis")?.Value ?? 0;

                var outrosReceitas = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Outras Receitas não Operacionais")?.Value ?? 0;

                var ganhosEPerdas = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Ganhos e Perdas de Capital")?.Value ?? 0;

                var receitasFinanceiras = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;

                var despesasFinanceiras = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0;

                var provisaoCSLL = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;

                var provisaoIRPJ = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

                var despesasDepreciacao = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas com Depreciação");

                var outrosResultadosOperacionais = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Outros  Resultados Operacionais")?.Value ?? 0;

                if (despesasOperacionais != null)
                    despesasOperacionais.TotalValue = despesasOperacionais.TotalValue + despesasDepreciacao.Value - outrosResultadosOperacionais;
                //+ despesasDepreciacao.Value

                // cálculos 
                var receitaLiquidaValor = receitaOperacionalBruta + deducoes;
                if (receitaLiquida != null) receitaLiquida.TotalValue = receitaLiquidaValor;
                if (lucroBruto != null) lucroBruto.TotalValue = receitaLiquidaValor + custoMercadorias + custoDosServicosPrestados;
                if (margemContribuicao != null && lucroBruto != null)
                    margemContribuicao.TotalValue = lucroBruto.TotalValue + despesasV;

                decimal margemContri = totalizerResponses
                    .FirstOrDefault(t => t.Name == "Margem Contribuição")?.TotalValue ?? 0;

                if (lucroOperacional != null && lucroBruto != null && despesasOperacionais != null)
                    lucroOperacional.TotalValue = (margemContri + despesasOperacionais.TotalValue + outrosResultadosOperacionais);

                if (lucroAntes != null && lucroOperacional != null)
                    lucroAntes.TotalValue = (lucroOperacional.TotalValue + outrosReceitas + ganhosEPerdas);

                if (resultadoAntes != null && lucroAntes != null)
                    resultadoAntes.TotalValue = (lucroAntes.TotalValue + receitasFinanceiras + despesasFinanceiras);

                if (lucroLiquido != null && resultadoAntes != null)
                    lucroLiquido.TotalValue = (resultadoAntes.TotalValue + provisaoCSLL + provisaoIRPJ);

                if (ebitda != null && lucroAntes != null)
                    ebitda.TotalValue = lucroAntes.TotalValue - despesasDepreciacao.Value;

                if (nopat != null && lucroAntes != null)
                    nopat.TotalValue = (lucroAntes.TotalValue + provisaoCSLL + provisaoIRPJ);

                // margens
                var margemBruta = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Bruta %");
                if (margemBruta != null)
                    margemBruta.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round((lucroBruto?.TotalValue ?? 0) / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemContribuicaoPorcentagem = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Contribuição %");
                if (margemContribuicaoPorcentagem != null && margemContribuicao != null)
                    margemContribuicaoPorcentagem.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(margemContribuicao.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemOperacional = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Operacional %");
                if (margemOperacional != null && lucroOperacional != null)
                    margemOperacional.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(lucroOperacional.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemLajir = totalizerResponses.FirstOrDefault(t => t.Name == "Margem LAJIR %");
                if (margemLajir != null && lucroAntes != null)
                    margemLajir.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(lucroAntes.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemLAIR = totalizerResponses.FirstOrDefault(t => t.Name == "Margem LAIR %");
                if (margemLAIR != null && resultadoAntes != null)
                    margemLAIR.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(resultadoAntes.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemLiquida = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Líquida %");
                if (margemLiquida != null && lucroLiquido != null)
                    margemLiquida.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(lucroLiquido.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemEBITDA = totalizerResponses.FirstOrDefault(t => t.Name == "Margem EBITDA %");
                if (margemEBITDA != null && ebitda != null)
                    margemEBITDA.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(ebitda.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemNOPAT = totalizerResponses.FirstOrDefault(t => t.Name == "Margem NOPAT %");
                if (margemNOPAT != null && nopat != null)
                    margemNOPAT.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(nopat.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                despesasDepreciacao.Value = despesasDepreciacao.Value * -1;

                months.Add(new MonthPainelContabilRespone
                {
                    Id = balancete.Id,
                    Name = balancete.DateMonth.GetDescription(),
                    DateMonth = (int)balancete.DateMonth,
                    Totalizer = totalizerResponses.OrderBy(t => t.TypeOrder).ToList()
                });
            }

            return new PainelBalancoContabilRespone { Months = months };
        }

        private async Task<PainelBalancoContabilRespone> BuildPainelBalancoReclassificadoByTypeAtivoOrcado(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _budgetRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationAsync(accountPlanId, typeClassification);



            var balancoReclassificados = await _balancoReclassificadoRepository.GetByAccountPlanIdListt(accountPlanId);

            var balancoReclassificadoIds = balancoReclassificados
                 .Where(c => c.TypeOrder >= 1 && c.TypeOrder <= 17)
                 .Distinct()
                 .ToList();

            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);

            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _budgetDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _budgetDataRepository.GetByAccountPlanClassificationId(accountPlanId);

            var months = balancetes
                .Select(balancete =>
                {
                    var totalizerResponses = balancoReclassificadoIds
                        .Select(totalizer =>
                        {
                            var relatedClassifications = classifications
                                .Where(c => c.BalancoReclassificadoId == totalizer.Id)
                                .ToList();

                            var classificationsResp = relatedClassifications
                                .Select(classification =>
                                {
                                    var datas = balanceteDataClassifications
                                        .Where(x => x.AccountPlanClassificationId == classification.Id)
                                        .SelectMany(x =>
                                            balanceteData
                                                .Where(bd => bd.CostCenter == x.CostCenter && bd.BudgetId == balancete.Id)
                                                .Select(bd => new BalanceteDataResponse
                                                {
                                                    Id = bd.Id,
                                                    CostCenter = bd.CostCenter,
                                                    Name = bd.Name,
                                                    Value = bd.FinalValue
                                                })
                                        ).ToList();

                                    return new ClassificationRespone
                                    {
                                        Id = classification.Id,
                                        Name = classification.Name,
                                        TypeOrder = classification.TypeOrder,
                                        Value = datas.Sum(d => d.Value),
                                        Datas = datas
                                    };
                                }).ToList();

                            return new TotalizerParentRespone
                            {
                                Id = totalizer.Id,
                                Name = totalizer.Name,
                                TypeOrder = totalizer.TypeOrder,
                                Classifications = classificationsResp,
                                TotalValue = classificationsResp.Sum(c => c.Value)
                            };
                        }).ToList();

                    // Mapas para acesso rápido
                    var totalizerMap = totalizerResponses.ToDictionary(t => t.Name);
                    var classificationMap = totalizerResponses
                        .SelectMany(t => t.Classifications)
                        .ToDictionary(c => c.Name);

                    // Aplicar regras de valor nos totalizadores
                    for (int i = 0; i < 3; i++)
                    {
                        foreach (var totalizer in totalizerResponses.OrderBy(t => t.TypeOrder))
                        {
                            var ruleValue = ApplyBalancoReclassificadoTotalAtivoValueRules(totalizer.Name, totalizerMap, classificationMap);
                            if (ruleValue.HasValue)
                                totalizer.TotalValue = ruleValue.Value;
                        }
                    }

                    // cálculos 

                    decimal ativoFinanceiro = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                    decimal ativoOperacional = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Operacional")?.TotalValue ?? 0;
                    decimal ativoFixo = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Fixo")?.TotalValue ?? 0;
                    decimal ativoNaoCirculante = totalizerResponses.FirstOrDefault(a => a.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                    decimal totalAtivo = ativoFinanceiro + ativoOperacional + ativoFixo + ativoNaoCirculante;

                    var depreciacao = totalizerResponses.FirstOrDefault(a => a.Name == "Depreciação / Amort. Acumulada");

                    if (depreciacao != null)
                    {
                        depreciacao.TotalValue = -Math.Abs(depreciacao.TotalValue);
                    }

                    return new MonthPainelContabilRespone
                    {
                        Id = balancete.Id,
                        Name = balancete.DateMonth.GetDescription(),
                        DateMonth = (int)balancete.DateMonth,
                        Totalizer = totalizerResponses,
                        MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                        {
                            Name = "TOTAL DO ATIVO",
                            TotalValue = totalAtivo
                        }
                    };
                })
                .OrderBy(m => m.DateMonth)
                .ToList();

            return new PainelBalancoContabilRespone
            {
                Months = months
            };
        }

        private async Task<PainelBalancoContabilRespone> BuildPainelBalancoReclassificadoByTypePassivoOrcado(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _budgetRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationAsync(accountPlanId, typeClassification);

            var balancoReclassificados = await _balancoReclassificadoRepository.GetByAccountPlanIdListt(accountPlanId);
            var balancoReclassificadoIds = balancoReclassificados
                .Where(c => c.TypeOrder >= 18 && c.TypeOrder <= 34)
                .Distinct()
                .ToList();

            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _budgetDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _budgetDataRepository.GetByAccountPlanClassificationId(accountPlanId);
            var painelBalancoContabilPassivo = await BuildPainelByTypePassivo(accountPlanId, year, 2);

            var months = balancetes
                .Select(balancete =>
                {
                    // Monta os totalizadores do mês (Classifications permanecem como estão)
                    var totalizerResponses = balancoReclassificadoIds
                        .Select(totalizer =>
                        {
                            var relatedClassifications = classifications
                                .Where(c => c.BalancoReclassificadoId == totalizer.Id)
                                .ToList();

                            var classificationsResp = relatedClassifications
                                .Select(classification =>
                                {
                                    var datas = balanceteDataClassifications
                                        .Where(x => x.AccountPlanClassificationId == classification.Id)
                                        .SelectMany(x =>
                                            balanceteData
                                                .Where(bd => bd.CostCenter == x.CostCenter && bd.BudgetId == balancete.Id)
                                                .Select(bd => new BalanceteDataResponse
                                                {
                                                    Id = bd.Id,
                                                    CostCenter = bd.CostCenter,
                                                    Name = bd.Name,
                                                    Value = bd.FinalValue
                                                })
                                        ).ToList();

                                    return new ClassificationRespone
                                    {
                                        Id = classification.Id,
                                        Name = classification.Name,
                                        TypeOrder = classification.TypeOrder,
                                        Value = datas.Sum(d => d.Value),
                                        Datas = datas
                                    };
                                }).ToList();

                            return new TotalizerParentRespone
                            {
                                Id = totalizer.Id,
                                Name = totalizer.Name,
                                TypeOrder = totalizer.TypeOrder,
                                Classifications = classificationsResp,
                                TotalValue = classificationsResp.Sum(c => c.Value)
                            };
                        }).ToList();

                    // Mapas para regras
                    var totalizerMap = totalizerResponses.ToDictionary(t => t.Name);
                    var classificationMap = totalizerResponses
                        .SelectMany(t => t.Classifications)
                        .ToDictionary(c => c.Name);

                    // Regras de valor
                    for (int i = 0; i < 3; i++)
                    {
                        foreach (var totalizer in totalizerResponses.OrderBy(t => t.TypeOrder))
                        {
                            var ruleValue = ApplyBalancoReclassificadoTotalPassivoValueRules(totalizer.Name, totalizerMap, classificationMap);
                            if (ruleValue.HasValue)
                                totalizer.TotalValue = ruleValue.Value;
                        }
                    }

                    // 🔹 Ajuste de "Resultado do Exercício Acumulado" -> "Resultado Acumulado"
                    var resultadodoExercicioAcumulado = painelBalancoContabilPassivo.Months
                        .Where(m => m.DateMonth == (int)balancete.DateMonth)
                        .SelectMany(m => m.Totalizer)
                        .SelectMany(a => a.Classifications)
                        .FirstOrDefault(t => t.Name == "Resultado do Exercício Acumulado");

                    var resultadoAcumulado = totalizerResponses.FirstOrDefault(a => a.Name == "Resultado Acumulado");
                    if (resultadodoExercicioAcumulado != null && resultadoAcumulado != null)
                    {
                        resultadoAcumulado.TotalValue = resultadodoExercicioAcumulado.Value;
                    }

                    // 🔹 Cálculo do PL
                    var patrimonioLiquido = totalizerResponses.FirstOrDefault(a => a.Name == "Patrimônio Liquido");
                    var lucrosPrejuizos = totalizerResponses.FirstOrDefault(a => a.Name == "Lucros / Prejuízos Acumulados")?.TotalValue ?? 0;
                    var resultadoAcumValor = resultadoAcumulado?.TotalValue ?? 0;

                    if (patrimonioLiquido != null)
                    {
                        patrimonioLiquido.TotalValue = patrimonioLiquido.TotalValue + lucrosPrejuizos + (resultadoAcumValor * -1);
                    }

                    // 🔹 NORMALIZAÇÃO: deixa todos os totalizadores POSITIVOS (sem mexer nas Classifications).
                    // Se quiser preservar "Resultado Acumulado" com sinal original, comente a linha do IF e use a condição abaixo.
                    foreach (var t in totalizerResponses)
                    {
                        // Para preservar o sinal do "Resultado Acumulado", troque por:
                        if (!string.Equals(t.Name, "Resultado Acumulado", StringComparison.OrdinalIgnoreCase))
                            t.TotalValue = Math.Abs(t.TotalValue);
                    }

                    // 🔹 Re-leitura após normalização (para garantir que o total do mês use os valores já positivos)
                    decimal passivoFinanceiro = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Financeiro")?.TotalValue ?? 0;
                    decimal passivoOperacional = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Operacional")?.TotalValue ?? 0;
                    decimal patrimonioLiquidoPos = totalizerResponses.FirstOrDefault(a => a.Name == "Patrimônio Liquido")?.TotalValue ?? 0;
                    decimal passivoNaoCirculante = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Não Circulante")?.TotalValue ?? 0;

                    // 🔹 Total do mês (já positivo)
                    decimal totalPassivo = passivoFinanceiro + passivoOperacional + patrimonioLiquidoPos + passivoNaoCirculante;
                    totalPassivo = Math.Abs(totalPassivo);

                    return new MonthPainelContabilRespone
                    {
                        Id = balancete.Id,
                        Name = balancete.DateMonth.GetDescription(),
                        DateMonth = (int)balancete.DateMonth,
                        Totalizer = totalizerResponses,
                        MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                        {
                            Name = "TOTAL DO PASSIVO",
                            TotalValue = totalPassivo
                        }
                    };
                })
                .OrderBy(m => m.DateMonth)
                .ToList();

            return new PainelBalancoContabilRespone
            {
                Months = months
            };
        }
        private async Task<PainelBalancoContabilRespone> BuildPainelByTypeDREOrcado(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _budgetRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationDREAsync(accountPlanId, typeClassification);
            var totalizersBase = await _totalizerClassificationRepository.GetByAccountPlansId(accountPlanId);
            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);

            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _budgetDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _budgetDataRepository.GetByAccountPlanClassificationId(accountPlanId);

            var months = new List<MonthPainelContabilRespone>();

            foreach (var balancete in balancetes.OrderBy(b => b.DateMonth))
            {
                var totalizerResponses = totalizersBase.Select(totalizer =>
                {
                    var relatedClassifications = classifications
                        .Where(c => c.TotalizerClassificationId == totalizer.Id)
                        .ToList();

                    var classificationsResp = relatedClassifications.Select(classification =>
                    {
                        var datas = balanceteDataClassifications
                            .Where(x => x.AccountPlanClassificationId == classification.Id)
                            .SelectMany(x =>
                                balanceteData
                                    .Where(bd => bd.CostCenter == x.CostCenter && bd.BudgetId == balancete.Id)
                                    .Select(bd => new BalanceteDataResponse
                                    {
                                        Id = bd.Id,
                                        CostCenter = bd.CostCenter,
                                        Name = bd.Name,
                                        InitialValue = bd.InitialValue,
                                        CreditValue = bd.Credit,
                                        DebitValue = bd.Debit,
                                        Value = bd.FinalValue
                                    })
                            ).ToList();

                        return new ClassificationRespone
                        {
                            Id = classification.Id,
                            Name = classification.Name,
                            TypeOrder = classification.TypeOrder,
                            Value = datas.Sum(a => a.CreditValue - a.DebitValue),
                            Datas = datas
                        };
                    }).ToList();

                    return new TotalizerParentRespone
                    {
                        Id = totalizer.Id,
                        Name = totalizer.Name,
                        TypeOrder = totalizer.TypeOrder,
                        Classifications = classificationsResp,
                        TotalValue = classificationsResp.Sum(a => a.Value)
                    };
                }).ToList();

                // === SEÇÃO DE CÁLCULOS === (mantida igual)
                var receitaOperacionalBruta = totalizerResponses.FirstOrDefault(t => t.Name == "Receita Operacional Bruta")?.TotalValue ?? 0;
                var deducoes = totalizerResponses.FirstOrDefault(t => t.Name == "(-) Deduções da Receita Bruta")?.TotalValue ?? 0;
                var receitaLiquida = totalizerResponses.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas");
                if (receitaLiquida != null) receitaLiquida.TotalValue = 0;
                var lucroBruto = totalizerResponses.FirstOrDefault(t => t.Name == "Lucro Bruto");
                if (lucroBruto != null) lucroBruto.TotalValue = 0;
                var margemContribuicao = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Contribuição");
                var despesasOperacionais = totalizerResponses.FirstOrDefault(t => t.Name == "(-) Despesas Operacionais");
                var lucroOperacional = totalizerResponses.FirstOrDefault(t => t.Name == "Lucro Operacional");
                if (lucroOperacional != null) lucroOperacional.TotalValue = 0;
                var lucroAntes = totalizerResponses.FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro");
                if (lucroAntes != null) lucroAntes.TotalValue = 0;
                var resultadoAntes = totalizerResponses.FirstOrDefault(t => t.Name == "Resultado do Exercício Antes do Imposto");
                if (resultadoAntes != null) resultadoAntes.TotalValue = 0;
                var lucroLiquido = totalizerResponses.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo");
                if (lucroLiquido != null) lucroLiquido.TotalValue = 0;
                var ebitda = totalizerResponses.FirstOrDefault(t => t.Name == "EBITDA");
                var nopat = totalizerResponses.FirstOrDefault(t => t.Name == "NOPAT");

                var custoMercadorias = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;
                var custoServicos = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;
                var despesasV = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Variáveis")?.Value ?? 0;
                var outrosReceitas = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Outras Receitas não Operacionais")?.Value ?? 0;
                var ganhosEPerdas = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Ganhos e Perdas de Capital")?.Value ?? 0;
                var receitasFin = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;
                var despesasFin = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0;
                var csll = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;
                var irpj = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;
                var despDep = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas com Depreciação");
                var outrosResultOp = totalizerResponses.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Outros  Resultados Operacionais")?.Value ?? 0;

                if (despesasOperacionais != null)
                    despesasOperacionais.TotalValue = despesasOperacionais.TotalValue + despDep.Value - outrosResultOp;

                var receitaLiquidaValor = receitaOperacionalBruta + deducoes;
                if (receitaLiquida != null) receitaLiquida.TotalValue = receitaLiquidaValor;
                if (lucroBruto != null) lucroBruto.TotalValue = receitaLiquidaValor + custoMercadorias + custoServicos;
                if (margemContribuicao != null && lucroBruto != null)
                    margemContribuicao.TotalValue = lucroBruto.TotalValue + despesasV;

                var margemContriValor = margemContribuicao?.TotalValue ?? 0;
                if (lucroOperacional != null && despesasOperacionais != null)
                    lucroOperacional.TotalValue = margemContriValor + despesasOperacionais.TotalValue + outrosResultOp;
                if (lucroAntes != null)
                    lucroAntes.TotalValue = lucroOperacional?.TotalValue + outrosReceitas + ganhosEPerdas ?? 0;
                if (resultadoAntes != null)
                    resultadoAntes.TotalValue = lucroAntes?.TotalValue + receitasFin + despesasFin ?? 0;
                if (lucroLiquido != null)
                    lucroLiquido.TotalValue = resultadoAntes?.TotalValue + csll + irpj ?? 0;
                if (ebitda != null)
                    ebitda.TotalValue = lucroAntes?.TotalValue - despDep.Value ?? 0;
                if (nopat != null)
                    nopat.TotalValue = lucroAntes?.TotalValue + csll + irpj ?? 0;


                // margens
                var margemBruta = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Bruta %");
                if (margemBruta != null)
                    margemBruta.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round((lucroBruto?.TotalValue ?? 0) / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemContribuicaoPorcentagem = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Contribuição %");
                if (margemContribuicaoPorcentagem != null && margemContribuicao != null)
                    margemContribuicaoPorcentagem.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(margemContribuicao.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemOperacional = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Operacional %");
                if (margemOperacional != null && lucroOperacional != null)
                    margemOperacional.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(lucroOperacional.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemLajir = totalizerResponses.FirstOrDefault(t => t.Name == "Margem LAJIR %");
                if (margemLajir != null && lucroAntes != null)
                    margemLajir.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(lucroAntes.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemLAIR = totalizerResponses.FirstOrDefault(t => t.Name == "Margem LAIR %");
                if (margemLAIR != null && resultadoAntes != null)
                    margemLAIR.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(resultadoAntes.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemLiquida = totalizerResponses.FirstOrDefault(t => t.Name == "Margem Líquida %");
                if (margemLiquida != null && lucroLiquido != null)
                    margemLiquida.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(lucroLiquido.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemEBITDA = totalizerResponses.FirstOrDefault(t => t.Name == "Margem EBITDA %");
                if (margemEBITDA != null && ebitda != null)
                    margemEBITDA.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(ebitda.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;

                var margemNOPAT = totalizerResponses.FirstOrDefault(t => t.Name == "Margem NOPAT %");
                if (margemNOPAT != null && nopat != null)
                    margemNOPAT.TotalValue = receitaLiquidaValor != 0
                        ? Math.Round(nopat.TotalValue / receitaLiquidaValor * 100, 2)
                        : 0;





                despDep.Value = despDep.Value * -1;

                months.Add(new MonthPainelContabilRespone
                {
                    Id = balancete.Id,
                    Name = balancete.DateMonth.GetDescription(),
                    DateMonth = (int)balancete.DateMonth,
                    Totalizer = totalizerResponses.OrderBy(t => t.TypeOrder).ToList()
                });
            }

            // === ACUMULADO SEM MARGENS ===
            var acumulado = CalcularAcumuladoSemMargens(months);
            months.Add(acumulado);

            return new PainelBalancoContabilRespone { Months = months };
        }

        private MonthPainelContabilRespone CalcularAcumuladoSemMargens(List<MonthPainelContabilRespone> months)
        {
            // Junta todos os totalizadores que aparecem em qualquer mês
            var todosTotalizers = months
                .SelectMany(m => m.Totalizer)
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .OrderBy(t => t.TypeOrder)
                .ToList();

            var acumuladoTotalizers = new List<TotalizerParentRespone>();

            foreach (var totalizer in todosTotalizers)
            {
                bool isMargem = totalizer.Name.Contains("%");

                // mesmo que o totalizador seja de margem, precisamos somar as classificações internas
                var classifAcumuladas = months
                    .SelectMany(m =>
                        m.Totalizer.FirstOrDefault(t => t.Id == totalizer.Id)?.Classifications
                        ?? new List<ClassificationRespone>())
                    .GroupBy(c => c.Id)
                    .Select(g => new ClassificationRespone
                    {
                        Id = g.Key,
                        Name = g.First().Name,
                        TypeOrder = g.First().TypeOrder,
                        Value = g.Sum(x => x.Value)
                    })
                    .OrderBy(c => c.TypeOrder)
                    .ToList();

                // Se for margem, o TotalValue fica zerado, mas as classificações ficam
                var totalValue = isMargem
                    ? 0
                    : months.Sum(m =>
                        m.Totalizer.FirstOrDefault(t => t.Id == totalizer.Id)?.TotalValue ?? 0);

                acumuladoTotalizers.Add(new TotalizerParentRespone
                {
                    Id = totalizer.Id,
                    Name = totalizer.Name,
                    TypeOrder = totalizer.TypeOrder,
                    TotalValue = totalValue,
                    Classifications = classifAcumuladas
                });
            }

            // === CAPTURA TOTALIZADORES BASE ===
            decimal receitaLiquida = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Receita Líquida"))?.TotalValue ?? 0;
            decimal lucroBruto = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Lucro Bruto"))?.TotalValue ?? 0;
            decimal margemContribuicao = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Margem Contribuição") && !t.Name.Contains("%"))?.TotalValue ?? 0;
            decimal lucroOperacional = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Lucro Operacional"))?.TotalValue ?? 0;
            decimal lucroAntes = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Lucro Antes"))?.TotalValue ?? 0;
            decimal resultadoAntes = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Resultado do Exercício Antes"))?.TotalValue ?? 0;
            decimal lucroLiquido = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("Lucro Líquido"))?.TotalValue ?? 0;
            decimal ebitda = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("EBITDA"))?.TotalValue ?? 0;
            decimal nopat = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains("NOPAT"))?.TotalValue ?? 0;

            // === REAPLICA AS MARGENS ===
            void SetMargem(string busca, decimal numerador)
            {
                var margem = acumuladoTotalizers.FirstOrDefault(t => t.Name.Contains(busca));
                if (margem != null)
                    margem.TotalValue = receitaLiquida != 0
                        ? Math.Round(numerador / receitaLiquida * 100, 2)
                        : 0;
            }

            SetMargem("Margem Bruta", lucroBruto);
            SetMargem("Margem Contribuição %", margemContribuicao);
            SetMargem("Margem Operacional", lucroOperacional);
            SetMargem("Margem LAJIR", lucroAntes);
            SetMargem("Margem LAIR", resultadoAntes);
            SetMargem("Margem Líquida", lucroLiquido);
            SetMargem("Margem EBITDA", ebitda);
            SetMargem("Margem NOPAT", nopat);

            // === Retorna o mês acumulado ===
            return new MonthPainelContabilRespone
            {
                Id = 0,
                Name = "ACUMULADO",
                DateMonth = 13,
                Totalizer = acumuladoTotalizers
            };
        }

        private decimal? ApplyBalancoReclassificadoTotalAtivoValueRules(string name, Dictionary<string, TotalizerParentRespone> totals, Dictionary<string, ClassificationRespone> classes)
        {
            decimal GetValue(string key) =>
                totals.TryGetValue(key, out var t) ? t.TotalValue :
                classes.TryGetValue(key, out var c) ? c.Value : 0;

            return name switch
            {
                "Ativo Financeiro" => GetValue("Caixa e Equivalente de Caixa") + GetValue("Aplicação Financeira"),
                "Ativo Operacional" => GetValue("Clientes") + GetValue("Estoques") + GetValue("Outros Ativos Operacionais") + GetValue("Contas Transitórias Ativo"),
                "Outros Ativos Operacionais Total" => GetValue("Outros Ativos Operacionais") + GetValue("Contas Transitórias Ativo"),
                "Ativo Não Circulante" => GetValue("Ativo Não Circulante Financeiro") + GetValue("Ativo Não Circulante Operacional"),
                "Ativo Fixo" => GetValue("Investimentos") + GetValue("Imobilizado") + GetValue("Depreciação / Amort. Acumulada") + GetValue("Intangível"),

                _ => null
            };
        }

        private decimal? ApplyBalancoReclassificadoTotalPassivoValueRules(string name, Dictionary<string, TotalizerParentRespone> totals, Dictionary<string, ClassificationRespone> classes)
        {
            decimal GetValue(string key) =>
                totals.TryGetValue(key, out var t) ? t.TotalValue :
                classes.TryGetValue(key, out var c) ? c.Value : 0;

            return name switch
            {
                "Passivo Financeiro" => GetValue("Empréstimos e Financiamentos"),
                "Passivo Operacional" => GetValue("Fornecedores") + GetValue("Obrigações Tributárias e Trabalhistas") + GetValue("Outros Passivos Operacionais") + GetValue("Contas Transitórias Passivo"),
                "Outros Passivos Operacionais Total" => GetValue("Outros Passivos Operacionais") + GetValue("Contas Transitórias Passivo"),
                "Passivo Não Circulante" => GetValue("Passivo Não Circulante Financeiro") + GetValue("Passivo Não Circulante Operacional"),
                "Patrimônio Liquido" => GetValue("Capital Social") + GetValue("Reservas") + GetValue("Lucros / Prejuízos Acumulado") + GetValue("Distribuição de Lucro") + GetValue("Resultado Acumulado"),

                _ => null
            };
        }

        #endregion
    }
}
