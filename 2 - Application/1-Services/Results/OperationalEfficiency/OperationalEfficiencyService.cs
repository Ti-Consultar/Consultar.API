using _2___Application._2_Dto_s.Results.LiquidManagement;
using _2___Application._2_Dto_s.Results.OperationalEfficiency;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application.Base;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.Results.OperationalEfficiency
{
    public class OperationalEfficiencyService : BaseService
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
        private readonly ParameterRepository _parameterRepository;

        public OperationalEfficiencyService(
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
            ParameterRepository parameterRepository,
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
            _parameterRepository = parameterRepository;
        }

        #region


        public async Task<PainelOperationalEfficiencyResponseDto> GetOperationalEfficiencyAntigo(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var operationalEfficiency = new List<OperationalEfficiencyResponseDto>();

            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            decimal waccTotal = wacc;
            wacc = wacc / 12;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                // Receitas
                decimal receitaLiquida = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

                decimal receitaFinanceira = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;

                // Custos e Despesas
                decimal custoMercadorias = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;

                decimal custoServicosPrestados = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;

                decimal despesasOperacional = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;

                decimal custosEDespesasOperacionais = custoMercadorias + custoServicosPrestados + despesasOperacional;

                // Lucros
                decimal ebitda = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "EBITDA")?.TotalValue ?? 0;

                decimal lucroOperacionalAntesJurosImpostos = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal provisaoCSLL = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;

                decimal provisaoIRPJ = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

                decimal resultadoFinanceiro = receitaFinanceira + (monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0);

                decimal impostos = provisaoCSLL + provisaoIRPJ;
                decimal lucroLiquido = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                // Ativos Circulantes
                decimal disponibilidade = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

                decimal clientes = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;

                decimal estoque = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                decimal outrosAtivosOperacionaisTotal = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                // Passivos Circulantes
                decimal fornecedores = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;

                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                // NCG
                decimal ncg = (disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal)
                              - (fornecedores - obrigacoesTributariasETrabalhistas - outrosPassivosOperacionaisTotal);

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncgTotal = valorAtivoOperacional - valorPassivoOperacional + disponibilidade;

                // Ativo e Passivo Não Circulantes + Ativos Fixos
                decimal realizavelLongoPrazo = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                decimal exigivelLongoPrazo = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;

                decimal ativosFixos = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoLiquido = disponibilidade + ncgTotal + realizavelLongoPrazo - exigivelLongoPrazo + ativosFixos;

                // NOPAT
                decimal nOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal margemNOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal margemEbitda = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                // Indicadores percentuais do mês
                decimal roic = capitalInvestidoLiquido != 0 ? (nOPAT / capitalInvestidoLiquido) * 100 : 0;
                decimal turnover = receitaLiquida != 0 ? capitalInvestidoLiquido / receitaLiquida : 0;
                decimal evaSPREAD = roic - wacc;
                decimal eva = capitalInvestidoLiquido != 0 ? (evaSPREAD / 100) * capitalInvestidoLiquido : 0;

                operationalEfficiency.Add(new OperationalEfficiencyResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    ReceitasLiquidas = receitaLiquida,
                    CustosDespesas = custosEDespesasOperacionais,
                    EBITDA = ebitda,
                    MargemEBITDA = margemEbitda,
                    LucroOperacionalAntesJurosImpostos = lucroOperacionalAntesJurosImpostos,
                    ResultadoFinanceiro = resultadoFinanceiro,
                    Impostos = impostos,
                    LucroLiquido = lucroLiquido,
                    NOPAT = nOPAT,
                    MargemNOPAT = margemNOPAT,
                    Disponivel = disponibilidade,
                    Clientes = clientes,
                    Estoques = estoque,
                    Fornecedores = fornecedores,
                    NCGCEF = ncg,
                    NCGTotal = ncgTotal,
                    InvestimentosAtivosFixos = capitalInvestidoLiquido - ncgTotal,
                    CapitalInvestidoLiquido = capitalInvestidoLiquido,
                    CapitalTurnover = turnover,
                    ROIC = roic,
                    WACC = wacc,
                    EVASPREAD = evaSPREAD,
                    EVA = eva
                });
            }

            // === ACUMULADO ANUAL ===
            var acumulado = new OperationalEfficiencyResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                ReceitasLiquidas = operationalEfficiency.Sum(x => x.ReceitasLiquidas),
                CustosDespesas = operationalEfficiency.Sum(x => x.CustosDespesas),
                EBITDA = operationalEfficiency.Sum(x => x.EBITDA),
                LucroOperacionalAntesJurosImpostos = operationalEfficiency.Sum(x => x.LucroOperacionalAntesJurosImpostos),
                ResultadoFinanceiro = operationalEfficiency.Sum(x => x.ResultadoFinanceiro),
                Impostos = operationalEfficiency.Sum(x => x.Impostos),
                LucroLiquido = operationalEfficiency.Sum(x => x.LucroLiquido),
                NOPAT = operationalEfficiency.Sum(x => x.NOPAT),
                Disponivel = operationalEfficiency.Sum(x => x.Disponivel),
                Clientes = operationalEfficiency.Sum(x => x.Clientes),
                Estoques = operationalEfficiency.Sum(x => x.Estoques),
                Fornecedores = operationalEfficiency.Sum(x => x.Fornecedores),
                NCGCEF = operationalEfficiency.Sum(x => x.NCGCEF),
                NCGTotal = operationalEfficiency.Sum(x => x.NCGTotal),
                InvestimentosAtivosFixos = operationalEfficiency.Sum(x => x.InvestimentosAtivosFixos),
                CapitalInvestidoLiquido = operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido),
                WACC = waccTotal,
                // Percentuais calculados a partir dos totalizadores acumulados
                MargemEBITDA = operationalEfficiency.Sum(x => x.ReceitasLiquidas) != 0
                    ? Math.Round(operationalEfficiency.Sum(x => x.EBITDA) / operationalEfficiency.Sum(x => x.ReceitasLiquidas) * 100, 2)
                    : 0,
                MargemNOPAT = operationalEfficiency.Sum(x => x.ReceitasLiquidas) != 0
                    ? Math.Round(operationalEfficiency.Sum(x => x.NOPAT) / operationalEfficiency.Sum(x => x.ReceitasLiquidas) * 100, 2)
                    : 0,
                ROIC = operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) != 0
                    ? Math.Round(operationalEfficiency.Sum(x => x.NOPAT) / operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) * 100, 2)
                    : 0,
                CapitalTurnover = operationalEfficiency.Sum(x => x.ReceitasLiquidas) != 0
                    ? Math.Round(operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) / operationalEfficiency.Sum(x => x.ReceitasLiquidas), 2)
                    : 0,
                EVASPREAD = operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) != 0
                    ? Math.Round((operationalEfficiency.Sum(x => x.NOPAT) / operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) * 100) - wacc, 2)
                    : 0,
                EVA = operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) != 0
                    ? Math.Round(((operationalEfficiency.Sum(x => x.NOPAT) / operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido) * 100) - wacc) / 100 * operationalEfficiency.Sum(x => x.CapitalInvestidoLiquido), 2)
                    : 0
            };

            operationalEfficiency.Add(acumulado);

            return new PainelOperationalEfficiencyResponseDto
            {
                OperationalEfficiency = new OperationalEfficiencyGroupedDto
                {
                    Months = operationalEfficiency
                }
            };
        }
        public async Task<PainelOperationalEfficiencyResponseDto> GetOperationalEfficiency(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var operationalEfficiency = new List<OperationalEfficiencyResponseDto>();

            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            decimal waccTotal = wacc;
            wacc = wacc / 12;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                // Receitas
                decimal receitaLiquida = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

                decimal receitaFinanceira = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;

                // Custos e Despesas
                decimal custoMercadorias = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;

                decimal custoServicosPrestados = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;

                decimal despesasOperacional = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;

                decimal custosEDespesasOperacionais = custoMercadorias + custoServicosPrestados + despesasOperacional;

                // Lucros
                decimal ebitda = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "EBITDA")?.TotalValue ?? 0;

                decimal lucroOperacionalAntesJurosImpostos = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal provisaoCSLL = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;

                decimal provisaoIRPJ = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

                decimal resultadoFinanceiro = receitaFinanceira + (monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0);

                decimal impostos = provisaoCSLL + provisaoIRPJ;
                decimal lucroLiquido = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                // Ativos Circulantes
                decimal disponibilidade = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

                decimal clientes = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;

                decimal estoque = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                decimal outrosAtivosOperacionaisTotal = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                // Passivos Circulantes
                decimal fornecedores = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;

                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                // NCG
                decimal ncg = (clientes + estoque) - (fornecedores);

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncgTotal = valorAtivoOperacional - valorPassivoOperacional + disponibilidade;

                // Ativo e Passivo Não Circulantes + Ativos Fixos
                decimal realizavelLongoPrazo = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                decimal exigivelLongoPrazo = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;

                decimal ativosFixos = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                var investimentosAtivosFixos = realizavelLongoPrazo - exigivelLongoPrazo + ativosFixos;

                decimal capitalInvestidoLiquido =   ncgTotal + investimentosAtivosFixos;

                // NOPAT
                decimal nOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal margemNOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal margemEbitda = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                // Indicadores percentuais do mês
                decimal roic = capitalInvestidoLiquido != 0 ? (nOPAT / capitalInvestidoLiquido) * 100 : 0;
                decimal turnover = receitaLiquida != 0 ? receitaLiquida / capitalInvestidoLiquido : 0;
                decimal evaSPREAD = roic - wacc;
                decimal eva = capitalInvestidoLiquido != 0 ? (evaSPREAD / 100) * capitalInvestidoLiquido : 0;

                operationalEfficiency.Add(new OperationalEfficiencyResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    ReceitasLiquidas = receitaLiquida,
                    CustosDespesas = custosEDespesasOperacionais,
                    EBITDA = ebitda,
                    MargemEBITDA = margemEbitda,
                    LucroOperacionalAntesJurosImpostos = lucroOperacionalAntesJurosImpostos,
                    ResultadoFinanceiro = resultadoFinanceiro,
                    Impostos = impostos,
                    LucroLiquido = lucroLiquido,
                    NOPAT = nOPAT,
                    MargemNOPAT = margemNOPAT,
                    Disponivel = disponibilidade,
                    Clientes = clientes,
                    Estoques = estoque,
                    Fornecedores = fornecedores,
                    NCGCEF = ncg,
                    NCGTotal = ncgTotal,
                    InvestimentosAtivosFixos = investimentosAtivosFixos,
                    CapitalInvestidoLiquido = capitalInvestidoLiquido,
                    CapitalTurnover = turnover,
                    ROIC = roic,
                    WACC = wacc,
                    EVASPREAD = evaSPREAD,
                    EVA = eva
                });
            }

            // === ACUMULADO ANUAL ===
            var ultimoMes = operationalEfficiency.OrderByDescending(x => x.DateMonth).FirstOrDefault();

            // Soma do WACC até o mês disponível (acumulado)
            var mesesDisponiveis = operationalEfficiency.Count;
            var waccAcumulado = wacc * mesesDisponiveis;

            // ROIC acumulado
            decimal capitalInvestidoFinal = ultimoMes?.CapitalInvestidoLiquido ?? 0;
            decimal totalNOPAT = operationalEfficiency.Sum(x => x.NOPAT);
            decimal totalReceitaLiquida = operationalEfficiency.Sum(x => x.ReceitasLiquidas);

            decimal roicAcumulado = capitalInvestidoFinal != 0 ? (totalNOPAT / capitalInvestidoFinal) * 100 : 0;

            // EVA Spread acumulado = ROIC acumulado - WACC acumulado
            decimal evaSpreadAcumulado = roicAcumulado - waccAcumulado;

            // EVA acumulado = EVA Spread acumulado (%) * Capital investido final
            decimal evaAcumulado = (evaSpreadAcumulado / 100) * capitalInvestidoFinal;

            var acumulado = new OperationalEfficiencyResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,

                ReceitasLiquidas = totalReceitaLiquida,
                CustosDespesas = operationalEfficiency.Sum(x => x.CustosDespesas),
                EBITDA = operationalEfficiency.Sum(x => x.EBITDA),
                LucroOperacionalAntesJurosImpostos = operationalEfficiency.Sum(x => x.LucroOperacionalAntesJurosImpostos),
                ResultadoFinanceiro = operationalEfficiency.Sum(x => x.ResultadoFinanceiro),
                Impostos = operationalEfficiency.Sum(x => x.Impostos),
                LucroLiquido = operationalEfficiency.Sum(x => x.LucroLiquido),
                NOPAT = totalNOPAT,

                Disponivel = ultimoMes?.Disponivel ?? 0,
                Clientes = ultimoMes?.Clientes ?? 0,
                Estoques = ultimoMes?.Estoques ?? 0,
                Fornecedores = ultimoMes?.Fornecedores ?? 0,
                NCGCEF = ultimoMes?.NCGCEF ?? 0,
                NCGTotal = ultimoMes?.NCGTotal ?? 0,
                InvestimentosAtivosFixos = ultimoMes?.InvestimentosAtivosFixos ?? 0,
                CapitalInvestidoLiquido = capitalInvestidoFinal,

                WACC = waccAcumulado, // ✅ acumulado até o mês
                ROIC = Math.Round(roicAcumulado, 2), // ✅ acumulado anual
                EVASPREAD = Math.Round(evaSpreadAcumulado, 2), // ✅ ROIC acumulado - WACC acumulado
                EVA = Math.Round(evaAcumulado, 2), // ✅ valor em dinheiro do EVA acumulado

                MargemEBITDA = totalReceitaLiquida != 0
                    ? Math.Round(operationalEfficiency.Sum(x => x.EBITDA) / totalReceitaLiquida * 100, 2)
                    : 0,
                MargemNOPAT = totalReceitaLiquida != 0
                    ? Math.Round(totalNOPAT / totalReceitaLiquida * 100, 2)
                    : 0,
                CapitalTurnover = capitalInvestidoFinal != 0
                    ? Math.Round(totalReceitaLiquida / capitalInvestidoFinal, 2)
                    : 0
            };



            operationalEfficiency.Add(acumulado);

            return new PainelOperationalEfficiencyResponseDto
            {
                OperationalEfficiency = new OperationalEfficiencyGroupedDto
                {
                    Months = operationalEfficiency
                }
            };
        }
        public async Task<PainelOperationalEfficiencyComparativoResponseDto> GetOperationalEfficiencyComparativo(int accountPlanId, int year)
        {
            // Painéis
            var painelAtivoRealizado = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);

            var painelPassivoRealizado = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);

            var painelDRERealizado = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            // Parâmetros
            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            decimal waccTotal = wacc;
            wacc = wacc / 12;

            var meses = Enumerable.Range(1, 12).ToList();
            var lista = new List<OperationalEfficiencyComparativoMesDto>();

            foreach (var mes in meses)
            {
                var ativoR = painelAtivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var ativoO = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var passivoR = painelPassivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var passivoO = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var dreR = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var dreO = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                // ------------ REALIZADO (mes) ------------
                decimal receitaLiquidaR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                decimal receitaFinanceiraR = dreR?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;

                decimal custoMercadoriasR = dreR?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;
                decimal custoServicosPrestadosR = dreR?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;
                decimal despesasOperacionalR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;
                decimal custosEDespesasOperacionaisR = custoMercadoriasR + custoServicosPrestadosR + despesasOperacionalR;

                decimal ebitdaR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "EBITDA")?.TotalValue ?? 0;
                decimal lucroOperacionalAntesJurosImpostosR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;
                decimal provisaoCSLLR = dreR?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;
                decimal provisaoIRPJR = dreR?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

                decimal resultadoFinanceiroR = receitaFinanceiraR + (dreR?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0);

                decimal impostosR = provisaoCSLLR + provisaoIRPJR;
                decimal lucroLiquidoR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                decimal disponibilidadeR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientesR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoqueR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosOperacionaisTotalR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedoresR = passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesTributariasETrabalhistasR = passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosOperacionaisTotalR = passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal ncgR = (clientesR + estoqueR) - (fornecedoresR);
                var valorAtivoOperacionalR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacionalR = passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncgTotalR = valorAtivoOperacionalR - valorPassivoOperacionalR + disponibilidadeR;

                decimal realizavelLongoPrazoR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLongoPrazoR = passivoR?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixosR = ativoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                var investimentosAtivosFixosR = realizavelLongoPrazoR - exigivelLongoPrazoR + ativosFixosR;
                decimal capitalInvestidoLiquidoR = ncgTotalR + investimentosAtivosFixosR;

                decimal nOPATR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;
                decimal margemNOPATR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;
                decimal margemEbitdaR = dreR?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                decimal roicR = capitalInvestidoLiquidoR != 0 ? (nOPATR / capitalInvestidoLiquidoR) * 100 : 0;
                decimal turnoverR = capitalInvestidoLiquidoR != 0 ? receitaLiquidaR / capitalInvestidoLiquidoR : 0;
                decimal evaSPREADR = roicR - wacc;
                decimal evaR = capitalInvestidoLiquidoR != 0 ? (evaSPREADR / 100) * capitalInvestidoLiquidoR : 0;

                var realizado = new OperationalEfficiencyResponseDto
                {
                    Name = dreR?.Name ?? new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    ReceitasLiquidas = receitaLiquidaR,
                    CustosDespesas = custosEDespesasOperacionaisR,
                    EBITDA = ebitdaR,
                    MargemEBITDA = margemEbitdaR,
                    LucroOperacionalAntesJurosImpostos = lucroOperacionalAntesJurosImpostosR,
                    ResultadoFinanceiro = resultadoFinanceiroR,
                    Impostos = impostosR,
                    LucroLiquido = lucroLiquidoR,
                    NOPAT = nOPATR,
                    MargemNOPAT = margemNOPATR,
                    Disponivel = disponibilidadeR,
                    Clientes = clientesR,
                    Estoques = estoqueR,
                    Fornecedores = fornecedoresR,
                    NCGCEF = ncgR,
                    NCGTotal = ncgTotalR,
                    InvestimentosAtivosFixos = investimentosAtivosFixosR,
                    CapitalInvestidoLiquido = capitalInvestidoLiquidoR,
                    CapitalTurnover = turnoverR,
                    ROIC = roicR,
                    WACC = wacc,
                    EVASPREAD = evaSPREADR,
                    EVA = evaR
                };

                // ------------ ORÇADO (mes) ------------
                decimal receitaLiquidaO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                decimal receitaFinanceiraO = dreO?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;

                decimal custoMercadoriasO = dreO?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;
                decimal custoServicosPrestadosO = dreO?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;
                decimal despesasOperacionalO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;
                decimal custosEDespesasOperacionaisO = custoMercadoriasO + custoServicosPrestadosO + despesasOperacionalO;

                decimal ebitdaO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "EBITDA")?.TotalValue ?? 0;
                decimal lucroOperacionalAntesJurosImpostosO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;
                decimal provisaoCSLLO = dreO?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;
                decimal provisaoIRPJO = dreO?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

                decimal resultadoFinanceiroO = receitaFinanceiraO + (dreO?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0);

                decimal impostosO = provisaoCSLLO + provisaoIRPJO;
                decimal lucroLiquidoO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                decimal disponibilidadeO = ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientesO = ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoqueO = ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                decimal fornecedoresO = passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                decimal ncgO = (clientesO + estoqueO) - (fornecedoresO);
                var valorAtivoOperacionalO = ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacionalO = passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncgTotalO = valorAtivoOperacionalO - valorPassivoOperacionalO + disponibilidadeO;

                decimal realizavelLongoPrazoO = ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLongoPrazoO = passivoO?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixosO = ativoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                var investimentosAtivosFixosO = realizavelLongoPrazoO - exigivelLongoPrazoO + ativosFixosO;
                decimal capitalInvestidoLiquidoO = ncgTotalO + investimentosAtivosFixosO;

                decimal nOPATO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;
                decimal margemNOPATO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;
                decimal margemEbitdaO = dreO?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                decimal roicO = capitalInvestidoLiquidoO != 0 ? (nOPATO / capitalInvestidoLiquidoO) * 100 : 0;
                decimal turnoverO = capitalInvestidoLiquidoO != 0 ? receitaLiquidaO / capitalInvestidoLiquidoO : 0;
                decimal evaSPREADO = roicO - wacc;
                decimal evaO = capitalInvestidoLiquidoO != 0 ? (evaSPREADO / 100) * capitalInvestidoLiquidoO : 0;

                var orcado = new OperationalEfficiencyResponseDto
                {
                    Name = dreO?.Name ?? new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    ReceitasLiquidas = receitaLiquidaO,
                    CustosDespesas = custosEDespesasOperacionaisO,
                    EBITDA = ebitdaO,
                    MargemEBITDA = margemEbitdaO,
                    LucroOperacionalAntesJurosImpostos = lucroOperacionalAntesJurosImpostosO,
                    ResultadoFinanceiro = resultadoFinanceiroO,
                    Impostos = impostosO,
                    LucroLiquido = lucroLiquidoO,
                    NOPAT = nOPATO,
                    MargemNOPAT = margemNOPATO,
                    Disponivel = disponibilidadeO,
                    Clientes = clientesO,
                    Estoques = estoqueO,
                    Fornecedores = fornecedoresO,
                    NCGCEF = ncgO,
                    NCGTotal = ncgTotalO,
                    InvestimentosAtivosFixos = investimentosAtivosFixosO,
                    CapitalInvestidoLiquido = capitalInvestidoLiquidoO,
                    CapitalTurnover = turnoverO,
                    ROIC = roicO,
                    WACC = wacc,
                    EVASPREAD = evaSPREADO,
                    EVA = evaO
                };

                // ------------ VARIAÇÃO (R - O) ------------
                var variacao = new OperationalEfficiencyResponseDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    ReceitasLiquidas = realizado.ReceitasLiquidas - orcado.ReceitasLiquidas,
                    CustosDespesas = realizado.CustosDespesas - orcado.CustosDespesas,
                    EBITDA = realizado.EBITDA - orcado.EBITDA,
                    MargemEBITDA = realizado.MargemEBITDA - orcado.MargemEBITDA,
                    LucroOperacionalAntesJurosImpostos = realizado.LucroOperacionalAntesJurosImpostos - orcado.LucroOperacionalAntesJurosImpostos,
                    ResultadoFinanceiro = realizado.ResultadoFinanceiro - orcado.ResultadoFinanceiro,
                    Impostos = realizado.Impostos - orcado.Impostos,
                    LucroLiquido = realizado.LucroLiquido - orcado.LucroLiquido,
                    NOPAT = realizado.NOPAT - orcado.NOPAT,
                    MargemNOPAT = realizado.MargemNOPAT - orcado.MargemNOPAT,
                    Disponivel = realizado.Disponivel - orcado.Disponivel,
                    Clientes = realizado.Clientes - orcado.Clientes,
                    Estoques = realizado.Estoques - orcado.Estoques,
                    Fornecedores = realizado.Fornecedores - orcado.Fornecedores,
                    NCGCEF = realizado.NCGCEF - orcado.NCGCEF,
                    NCGTotal = realizado.NCGTotal - orcado.NCGTotal,
                    InvestimentosAtivosFixos = realizado.InvestimentosAtivosFixos - orcado.InvestimentosAtivosFixos,
                    CapitalInvestidoLiquido = realizado.CapitalInvestidoLiquido - orcado.CapitalInvestidoLiquido,
                    CapitalTurnover = realizado.CapitalTurnover - orcado.CapitalTurnover,
                    ROIC = realizado.ROIC - orcado.ROIC,
                    WACC = wacc,
                    EVASPREAD = realizado.EVASPREAD - orcado.EVASPREAD,
                    EVA = realizado.EVA - orcado.EVA
                };

                lista.Add(new OperationalEfficiencyComparativoMesDto
                {
                    Name = realizado.Name,
                    DateMonth = mes,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            // ------------ ACUMULADO para Realizado & Orcado & Variacao ------------
            // Realizado acumulado
            var realizados = lista.Select(x => x.Realizado).Where(x => x != null).ToList();
            var orcados = lista.Select(x => x.Orcado).Where(x => x != null).ToList();
            var variacoes = lista.Select(x => x.Variacao).Where(x => x != null).ToList();

            // Realizado acumulado: soma valores financeiros; margens e índices recalculados quando aplicável
            var ultimoRealizado = realizados.OrderByDescending(x => x.DateMonth).FirstOrDefault();
            var mesesDisponiveisR = realizados.Count;
            var waccAcumuladoR = wacc * mesesDisponiveisR;
            var totalNOPATR = realizados.Sum(x => x.NOPAT);
            var totalReceitaLiquidaR = realizados.Sum(x => x.ReceitasLiquidas);
            var capitalInvestidoFinalR = ultimoRealizado?.CapitalInvestidoLiquido ?? 0;
            var roicAcumuladoR = capitalInvestidoFinalR != 0 ? (totalNOPATR / capitalInvestidoFinalR) * 100 : 0;
            var evaSpreadAcumuladoR = roicAcumuladoR - waccAcumuladoR;
            var evaAcumuladoR = (evaSpreadAcumuladoR / 100) * capitalInvestidoFinalR;

            var acumuladoRealizado = new OperationalEfficiencyResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                ReceitasLiquidas = totalReceitaLiquidaR,
                CustosDespesas = realizados.Sum(x => x.CustosDespesas),
                EBITDA = realizados.Sum(x => x.EBITDA),
                LucroOperacionalAntesJurosImpostos = realizados.Sum(x => x.LucroOperacionalAntesJurosImpostos),
                ResultadoFinanceiro = realizados.Sum(x => x.ResultadoFinanceiro),
                Impostos = realizados.Sum(x => x.Impostos),
                LucroLiquido = realizados.Sum(x => x.LucroLiquido),
                NOPAT = totalNOPATR,
                Disponivel = ultimoRealizado?.Disponivel ?? 0,
                Clientes = ultimoRealizado?.Clientes ?? 0,
                Estoques = ultimoRealizado?.Estoques ?? 0,
                Fornecedores = ultimoRealizado?.Fornecedores ?? 0,
                NCGCEF = ultimoRealizado?.NCGCEF ?? 0,
                NCGTotal = ultimoRealizado?.NCGTotal ?? 0,
                InvestimentosAtivosFixos = ultimoRealizado?.InvestimentosAtivosFixos ?? 0,
                CapitalInvestidoLiquido = capitalInvestidoFinalR,
                WACC = waccAcumuladoR,
                ROIC = Math.Round(roicAcumuladoR, 2),
                EVASPREAD = Math.Round(evaSpreadAcumuladoR, 2),
                EVA = Math.Round(evaAcumuladoR, 2),
                MargemEBITDA = totalReceitaLiquidaR != 0 ? Math.Round(realizados.Sum(x => x.EBITDA) / totalReceitaLiquidaR * 100, 2) : 0,
                MargemNOPAT = totalReceitaLiquidaR != 0 ? Math.Round(totalNOPATR / totalReceitaLiquidaR * 100, 2) : 0,
                CapitalTurnover = capitalInvestidoFinalR != 0 ? Math.Round(totalReceitaLiquidaR / capitalInvestidoFinalR, 2) : 0
            };

            // Orcado acumulado
            var ultimoOrcado = orcados.OrderByDescending(x => x.DateMonth).FirstOrDefault();
            var mesesDisponiveisO = orcados.Count;
            var waccAcumuladoO = wacc * mesesDisponiveisO;
            var totalNOPATO = orcados.Sum(x => x.NOPAT);
            var totalReceitaLiquidaO = orcados.Sum(x => x.ReceitasLiquidas);
            var capitalInvestidoFinalO = ultimoOrcado?.CapitalInvestidoLiquido ?? 0;
            var roicAcumuladoO = capitalInvestidoFinalO != 0 ? (totalNOPATO / capitalInvestidoFinalO) * 100 : 0;
            var evaSpreadAcumuladoO = roicAcumuladoO - waccAcumuladoO;
            var evaAcumuladoO = (evaSpreadAcumuladoO / 100) * capitalInvestidoFinalO;

            var acumuladoOrcado = new OperationalEfficiencyResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                ReceitasLiquidas = totalReceitaLiquidaO,
                CustosDespesas = orcados.Sum(x => x.CustosDespesas),
                EBITDA = orcados.Sum(x => x.EBITDA),
                LucroOperacionalAntesJurosImpostos = orcados.Sum(x => x.LucroOperacionalAntesJurosImpostos),
                ResultadoFinanceiro = orcados.Sum(x => x.ResultadoFinanceiro),
                Impostos = orcados.Sum(x => x.Impostos),
                LucroLiquido = orcados.Sum(x => x.LucroLiquido),
                NOPAT = totalNOPATO,
                Disponivel = ultimoOrcado?.Disponivel ?? 0,
                Clientes = ultimoOrcado?.Clientes ?? 0,
                Estoques = ultimoOrcado?.Estoques ?? 0,
                Fornecedores = ultimoOrcado?.Fornecedores ?? 0,
                NCGCEF = ultimoOrcado?.NCGCEF ?? 0,
                NCGTotal = ultimoOrcado?.NCGTotal ?? 0,
                InvestimentosAtivosFixos = ultimoOrcado?.InvestimentosAtivosFixos ?? 0,
                CapitalInvestidoLiquido = capitalInvestidoFinalO,
                WACC = waccAcumuladoO,
                ROIC = Math.Round(roicAcumuladoO, 2),
                EVASPREAD = Math.Round(evaSpreadAcumuladoO, 2),
                EVA = Math.Round(evaAcumuladoO, 2),
                MargemEBITDA = totalReceitaLiquidaO != 0 ? Math.Round(orcados.Sum(x => x.EBITDA) / totalReceitaLiquidaO * 100, 2) : 0,
                MargemNOPAT = totalReceitaLiquidaO != 0 ? Math.Round(totalNOPATO / totalReceitaLiquidaO * 100, 2) : 0,
                CapitalTurnover = capitalInvestidoFinalO != 0 ? Math.Round(totalReceitaLiquidaO / capitalInvestidoFinalO, 2) : 0
            };

            // Variacao acumulada = Realizado acumulado - Orcado acumulado (para campos relevantes)
            var acumuladoVariacao = new OperationalEfficiencyResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                ReceitasLiquidas = acumuladoRealizado.ReceitasLiquidas - acumuladoOrcado.ReceitasLiquidas,
                CustosDespesas = acumuladoRealizado.CustosDespesas - acumuladoOrcado.CustosDespesas,
                EBITDA = acumuladoRealizado.EBITDA - acumuladoOrcado.EBITDA,
                LucroOperacionalAntesJurosImpostos = acumuladoRealizado.LucroOperacionalAntesJurosImpostos - acumuladoOrcado.LucroOperacionalAntesJurosImpostos,
                ResultadoFinanceiro = acumuladoRealizado.ResultadoFinanceiro - acumuladoOrcado.ResultadoFinanceiro,
                Impostos = acumuladoRealizado.Impostos - acumuladoOrcado.Impostos,
                LucroLiquido = acumuladoRealizado.LucroLiquido - acumuladoOrcado.LucroLiquido,
                NOPAT = acumuladoRealizado.NOPAT - acumuladoOrcado.NOPAT,
                Disponivel = acumuladoRealizado.Disponivel - acumuladoOrcado.Disponivel,
                Clientes = acumuladoRealizado.Clientes - acumuladoOrcado.Clientes,
                Estoques = acumuladoRealizado.Estoques - acumuladoOrcado.Estoques,
                Fornecedores = acumuladoRealizado.Fornecedores - acumuladoOrcado.Fornecedores,
                NCGCEF = acumuladoRealizado.NCGCEF - acumuladoOrcado.NCGCEF,
                NCGTotal = acumuladoRealizado.NCGTotal - acumuladoOrcado.NCGTotal,
                InvestimentosAtivosFixos = acumuladoRealizado.InvestimentosAtivosFixos - acumuladoOrcado.InvestimentosAtivosFixos,
                CapitalInvestidoLiquido = acumuladoRealizado.CapitalInvestidoLiquido - acumuladoOrcado.CapitalInvestidoLiquido,
                CapitalTurnover = acumuladoRealizado.CapitalTurnover - acumuladoOrcado.CapitalTurnover,
                ROIC = acumuladoRealizado.ROIC - acumuladoOrcado.ROIC,
                WACC = acumuladoRealizado.WACC - acumuladoOrcado.WACC,
                EVASPREAD = acumuladoRealizado.EVASPREAD - acumuladoOrcado.EVASPREAD,
                EVA = acumuladoRealizado.EVA - acumuladoOrcado.EVA,
                MargemEBITDA = acumuladoRealizado.MargemEBITDA - acumuladoOrcado.MargemEBITDA,
                MargemNOPAT = acumuladoRealizado.MargemNOPAT - acumuladoOrcado.MargemNOPAT
            };

            // Adiciona acumulados na lista como último item
            lista.Add(new OperationalEfficiencyComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = acumuladoRealizado,
                Orcado = acumuladoOrcado,
                Variacao = acumuladoVariacao
            });

            return new PainelOperationalEfficiencyComparativoResponseDto { Months = lista };
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
//{
//    "name": "January",
//        "dateMonth": 1,
//        "receitasLiquidas": 7662779.53,
//        "custosDespesas": -7213473.3,
//        "ebitda": 507002.85,
//        "margemEBITDA": 6.62,
//        "lucroOperacionalAntesJurosImpostos": 433904.62,
//        "resultadoFinanceiro": -195342.76,
//        "impostos": -79383.61,
//        "lucroLiquido": 159178.25,
//        "nopat": 354521.01,
//        "margemNOPAT": 4.63,
//        "disponivel": 3972995.18,
//        "clientes": 8651191.83,
//        "estoques": 9564219.28,
//        "fornecedores": 16332151.62,
//        "ncgcef": 1883259.49,
//        "ncgTotal": 2854139.89,
//        "investimentosAtivosFixos": 12975284.06,
//        "capitalInvestidoLiquido": 15829423.95,
//        "capitalTurnover": 0.48408454749864727,
//        "roic": 2.23963304741737,
//        "wacc": 1.6308333333333334,
//        "evaspread": 0.6087997140840365,
//        "eva": 96369.48774875
//      },