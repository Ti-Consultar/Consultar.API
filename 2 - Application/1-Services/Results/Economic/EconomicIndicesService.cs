using _2___Application._2_Dto_s.DashBoards;
using _2___Application._2_Dto_s.Painel;
using _2___Application._2_Dto_s.Results.EconomicIndices;
using _2___Application._2_Dto_s.Results.LiquidManagement;
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

namespace _2___Application._1_Services.Results
{
    public class EconomicIndicesService : BaseService
    {
        private readonly ClassificationRepository _repository;
        private readonly AccountPlanClassificationRepository _accountClassificationRepository;
        private readonly BalanceteDataRepository _balanceteDataRepository;
        private readonly BalanceteRepository _balanceteRepository;
        private readonly BudgetRepository _budgetRepository;
        private readonly BudgetDataRepository _budgetDataRepository;
        private readonly CompanyRepository _companyRepository;
        private readonly TotalizerClassificationRepository _totalizerClassificationRepository;
        private readonly TotalizerClassificationTemplateRepository _totalizerClassificationTemplateRepository;
        private readonly BalancoReclassificadoTemplateRepository _balancoReclassificadoTemplateRepository;
        private readonly BalancoReclassificadoRepository _balancoReclassificadoRepository;
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly ParameterRepository _parameterRepository;

        public EconomicIndicesService(
            ClassificationRepository repository,
            AccountPlanClassificationRepository accountClassificationRepository,
            BalanceteDataRepository balanceteDataRepository,
            BalanceteRepository balanceteRepository,
            BudgetRepository budgetRepository,
            BudgetDataRepository budgetDataRepository,
            CompanyRepository companyRepository,
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
            _companyRepository = companyRepository;
            _totalizerClassificationRepository = _talizerClassificationRepository;
            _totalizerClassificationTemplateRepository = totalizerClassificationTemplateRepository;
            _balancoReclassificadoTemplateRepository = balancoReclassificadoTemplateRepository;
            _balancoReclassificadoRepository = balancoReclassificadoRepository;
            _accountPlansRepository = accountPlansRepository;
            _parameterRepository = parameterRepository;
        }
        #region Lucratividade

        public async Task<PainelProfitabilityResponseDto> GetProfitability(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var profitabilities = new List<ProfitabilityResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal margemBruta = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0;

                decimal margemEbitda = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                decimal margemOperacional = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0;

                decimal margemNOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal margemLiquida = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0;

                profitabilities.Add(new ProfitabilityResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    MargemBruta = margemBruta,
                    MargemEBITDA = margemEbitda,
                    MargemOperacional = margemOperacional,
                    MargemNOPAT = margemNOPAT,
                    MargemLiquida = margemLiquida
                });
            }

            // 🔢 Acumulado anual das margens (usando o mês ACUMULADO da DRE)
            var acumuladoDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == 13);

            var acumulado = new ProfitabilityResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                MargemBruta = acumuladoDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0,
                MargemEBITDA = acumuladoDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0,
                MargemOperacional = acumuladoDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0,
                MargemNOPAT = acumuladoDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0,
                MargemLiquida = acumuladoDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0
            };

            profitabilities.Add(acumulado);

            return new PainelProfitabilityResponseDto
            {
                Profitability = new ProfitabilityGroupedDto
                {
                    Months = profitabilities
                }
            };
        }

        public async Task<PainelProfitabilityComparativoResponseDto> GetProfitabilityComparativo(int accountPlanId, int year)
        {
            var painelDRERealizado = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            var meses = Enumerable.Range(1, 12).ToList();
            var lista = new List<ProfitabilityComparativoMesDto>();

            foreach (var mes in meses)
            {
                var monthRealizado = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthOrcado = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                // 🔹 Realizado
                var realizado = new ProfitabilityItemDto
                {
                    MargemBruta = monthRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0,
                    MargemEBITDA = monthRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0,
                    MargemOperacional = monthRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0,
                    MargemNOPAT = monthRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0,
                    MargemLiquida = monthRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0
                };

                // 🔹 Orçado
                var orcado = new ProfitabilityItemDto
                {
                    MargemBruta = monthOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0,
                    MargemEBITDA = monthOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0,
                    MargemOperacional = monthOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0,
                    MargemNOPAT = monthOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0,
                    MargemLiquida = monthOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0
                };

                // 🔹 Variação
                var variacao = new ProfitabilityItemDto
                {
                    MargemBruta = realizado.MargemBruta - orcado.MargemBruta,
                    MargemEBITDA = realizado.MargemEBITDA - orcado.MargemEBITDA,
                    MargemOperacional = realizado.MargemOperacional - orcado.MargemOperacional,
                    MargemNOPAT = realizado.MargemNOPAT - orcado.MargemNOPAT,
                    MargemLiquida = realizado.MargemLiquida - orcado.MargemLiquida
                };

                lista.Add(new ProfitabilityComparativoMesDto
                {
                    Name = new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    Realizado = realizado,
                    Orcado = orcado,
                    Variacao = variacao
                });
            }

            // 🔹 ACUMULADO
            var acumuladoRealizado = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == 13);
            var acumuladoOrcado = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == 13);

            lista.Add(new ProfitabilityComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new ProfitabilityItemDto
                {
                    MargemBruta = acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0,
                    MargemEBITDA = acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0,
                    MargemOperacional = acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0,
                    MargemNOPAT = acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0,
                    MargemLiquida = acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0
                },
                Orcado = new ProfitabilityItemDto
                {
                    MargemBruta = acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0,
                    MargemEBITDA = acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0,
                    MargemOperacional = acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0,
                    MargemNOPAT = acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0,
                    MargemLiquida = acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0
                },
                Variacao = new ProfitabilityItemDto
                {
                    MargemBruta = (acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0) -
                                  (acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0),
                    MargemEBITDA = (acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0) -
                                   (acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0),
                    MargemOperacional = (acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0) -
                                        (acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0),
                    MargemNOPAT = (acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0) -
                                  (acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0),
                    MargemLiquida = (acumuladoRealizado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0) -
                                    (acumuladoOrcado?.Totalizer.FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0)
                }
            });

            return new PainelProfitabilityComparativoResponseDto
            {
                Profitability = new ProfitabilityComparativoGroupedDto
                {
                    Months = lista
                }
            };
        }

        public async Task<List<DashBoardDto>> GetGroupDashboard(int groupId, int year)
        {
            // 1. Pega todas as empresas do grupo
            var companyIds = await _companyRepository.GetCompanyIdsByGroupId(groupId);

            if (!companyIds.Any())
                return new List<DashBoardDto>();

            // 2. Cria um dicionário para somar os dados por mês
            var dashboardDictionary = new Dictionary<int, DashBoardDto>();

            foreach (var companyId in companyIds)
            {
                // Pega o accountPlanId de cada empresa (supondo que você tenha esse método)
                var accountPlanId = await _companyRepository.GetAccountPlanIdByCompanyId(companyId.Value);

                var companyDashboard = await GetDashboard(accountPlanId.Value, year);

                foreach (var monthData in companyDashboard)
                {
                    if (!dashboardDictionary.ContainsKey(monthData.DateMonth))
                    {
                        // Cria novo registro no dicionário
                        dashboardDictionary[monthData.DateMonth] = new DashBoardDto
                        {
                            Name = monthData.Name,
                            DateMonth = monthData.DateMonth,
                            ReceitaLiquida = monthData.ReceitaLiquida,
                            MargemBruta = monthData.MargemBruta,
                            MargemLiquida = monthData.MargemLiquida,
                            VariacaoReceitaLiquida = 0,  // ainda vamos calcular
                            VariacaoMargemBruta = 0,
                            VariacaoMargemLiquida = 0
                        };
                    }
                    else
                    {
                        // Soma os valores das filiais
                        dashboardDictionary[monthData.DateMonth].ReceitaLiquida += monthData.ReceitaLiquida;
                        dashboardDictionary[monthData.DateMonth].MargemBruta += monthData.MargemBruta;
                        dashboardDictionary[monthData.DateMonth].MargemLiquida += monthData.MargemLiquida;
                    }
                }
            }

            // 3. Calcula a variação mês a mês
            DashBoardDto? anterior = null;
            var result = dashboardDictionary.OrderBy(d => d.Key).Select(kvp =>
            {
                var current = kvp.Value;

                if (anterior != null)
                {
                    current.VariacaoReceitaLiquida = anterior.ReceitaLiquida != 0
                        ? (current.ReceitaLiquida - anterior.ReceitaLiquida) / anterior.ReceitaLiquida * 100
                        : 0;

                    current.VariacaoMargemBruta = anterior.MargemBruta != 0
                        ? (current.MargemBruta - anterior.MargemBruta) / anterior.MargemBruta * 100
                        : 0;

                    current.VariacaoMargemLiquida = anterior.MargemLiquida != 0
                        ? (current.MargemLiquida - anterior.MargemLiquida) / anterior.MargemLiquida * 100
                        : 0;
                }

                anterior = current;
                return current;
            }).ToList();

            return result;
        }


        public async Task<List<DashBoardDto>> GetDashboard(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);


            var dashboard = new List<DashBoardDto>();

            decimal? receitaAnterior = null;
            decimal? margemBrutaAnterior = null;
            decimal? margemLiquidaAnterior = null;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal receitaLiquida = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

                decimal margemBruta = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0;

                decimal margemLiquida = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Líquida %")?.TotalValue ?? 0;

                // Calcula variação em relação ao mês anterior
                decimal variacaoReceitaLiquida = receitaAnterior.HasValue && receitaAnterior.Value != 0
                    ? (receitaLiquida - receitaAnterior.Value) / receitaAnterior.Value * 100
                    : 0;

                decimal variacaoMargemBruta = margemBrutaAnterior.HasValue && margemBrutaAnterior.Value != 0
                    ? (margemBruta - margemBrutaAnterior.Value) / margemBrutaAnterior.Value * 100
                    : 0;

                decimal variacaoMargemLiquida = margemLiquidaAnterior.HasValue && margemLiquidaAnterior.Value != 0
                    ? (margemLiquida - margemLiquidaAnterior.Value) / margemLiquidaAnterior.Value * 100
                    : 0;

                dashboard.Add(new DashBoardDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    ReceitaLiquida = receitaLiquida,
                    VariacaoReceitaLiquida = Math.Round(variacaoReceitaLiquida, 2),
                    MargemBruta = margemBruta,
                    VariacaoMargemBruta = Math.Round(variacaoMargemBruta, 2),
                    MargemLiquida = margemLiquida,
                    VariacaoMargemLiquida = Math.Round(variacaoMargemLiquida, 2)
                });

                // Atualiza valores para o próximo loop
                receitaAnterior = receitaLiquida;
                margemBrutaAnterior = margemBruta;
                margemLiquidaAnterior = margemLiquida;
            }

            return dashboard;
        }

        public async Task<List<DashBoardGestaoPrazoMedioDto>> GetDashboardGestaoPrazoMedio(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var dashboard = new List<DashBoardGestaoPrazoMedioDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal clientes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoques = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal fornecedores = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                // --- Cálculos Turnover ---
                var receitaMensal = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional + valorPassivoOperacional;

                decimal pMR = 0, pME = 0, pMP = 0, cicloNCG = 0;

                if (receitaMensal > 0)
                {
                    int multiplicadorDias = monthAtivo.DateMonth * 30;
                    pMR = (clientes / receitaMensal) * multiplicadorDias;
                    pME = (estoques / receitaMensal) * multiplicadorDias;
                    pMP = (fornecedores / receitaMensal) * multiplicadorDias;
                    cicloNCG = (ncg / receitaMensal) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCG = pME + pMR + pMP;

                var giroPME = pME != 0 ? 30 / pME : 0;
                var giroPMR = pMR != 0 ? 30 / pMR : 0;
                var giroPMP = pMP != 0 ? 30 / pMP : 0;
                var giroCaixa = cicloFinanceiroOperacoesPrincipaisNCG != 0 ? 30 / cicloFinanceiroOperacoesPrincipaisNCG : 0;

                // --- Monta DTO unificado ---
                dashboard.Add(new DashBoardGestaoPrazoMedioDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    Clientes = clientes,
                    Estoques = estoques,
                    Fornecedores = fornecedores,

                    // Turnover
                    GiroPME = giroPME,
                    GiroPMR = giroPMR,
                    GiroPMP = giroPMP,
                    GiroCaixa = giroCaixa
                });
            }

            return dashboard;
        }

        #endregion

        #region Rentabilidade
        public async Task<PainelRentabilityResponseDto> GetRentabilibty(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelBCAtivo = await BuildPainelByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // 🔁 Carrega também o painel do ANO ANTERIOR (somente o Ativo é necessário aqui)
            var painelPassivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year - 1, 2);

            var dezembroAnoAnterior = painelPassivoAnoAnterior.Months
                .FirstOrDefault(a => a.DateMonth == 12);

            decimal patrimonioLiquidoAnoAnterior = dezembroAnoAnterior?.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var rentabilities = new List<RentabilityResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthBcAtivo = painelBCAtivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal lucroLiquido = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                var  ativoTotal = monthBcAtivo?.Totalizer.Sum(a => a.TotalValue);
                decimal nopat = monthDRE?.Totalizer
                .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal disponibilidade = monthAtivo?.Totalizer
                                    .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

                decimal clientes = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;

                decimal estoque = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                decimal outrosAtivosOperacionaisTotal = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedores = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;

                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal somaAtivos = disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal;

                decimal somaPassivo = fornecedores - obrigacoesTributariasETrabalhistas - outrosPassivosOperacionaisTotal;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional - valorPassivoOperacional;

                decimal necessidadeDeCapitalDeGiro = ncg;

                decimal realizavelLongoPrazo = monthAtivo?.Totalizer
                  .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                decimal exigivelLongoPrazo = monthPassivo?.Totalizer
                     .FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;

                decimal ativosFixos = monthAtivo?.Totalizer
                  .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoLiquido = disponibilidade + necessidadeDeCapitalDeGiro + realizavelLongoPrazo - exigivelLongoPrazo + ativosFixos;// inverti o exigivel para ser subtração

                decimal patrimonioLiquido = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                // ⚠️ Evita divisão por zero
                decimal roi = capitalInvestidoLiquido != 0 ? nopat / capitalInvestidoLiquido : 0;
                decimal roe = patrimonioLiquido != 0 ? lucroLiquido / patrimonioLiquido : 0;
                decimal roeInicial = patrimonioLiquidoAnoAnterior != 0 ? lucroLiquido / patrimonioLiquidoAnoAnterior : 0;

                rentabilities.Add(new RentabilityResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    ROI = roi * 100,
                    LiquidoMensalROE = roe * 100,
                    LiquidoInicioROE = roeInicial * 100,
                });
            }

            return new PainelRentabilityResponseDto
            {
                Rentability = new RentabilityGroupedDto
                {
                    Months = rentabilities
                }
            };
        }
        public async Task<PainelRentabilityComparativoResponseDto> GetRentabilityComparativo(int accountPlanId, int year)
        {
            // 🔹 Painel Realizado
            var painelAtivoR = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivoR = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE_R = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // 🔹 Painel Orçado
            var painelAtivoO = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoO = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);
            var painelDRE_O = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            // 🔹 Passivo do ano anterior para ROE inicial
            var painelPassivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year - 1, 2);
            var dezembroAnoAnterior = painelPassivoAnoAnterior.Months.FirstOrDefault(m => m.DateMonth == 12);
            decimal patrimonioLiquidoAnoAnterior = dezembroAnoAnterior?.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var lista = new List<RentabilityComparativoMesDto>();

            for (int mes = 1; mes <= 12; mes++)
            {
                // 🔹 REALIZADO
                var monthAtivoR_M = painelAtivoR.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoR_M = painelPassivoR.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthDRE_R_M = painelDRE_R.Months.FirstOrDefault(m => m.DateMonth == mes);

                decimal lucroLiquidoR = monthDRE_R_M?.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;
                decimal nopatR = monthDRE_R_M?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal disponibilidadeR = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientesR = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoqueR = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosR = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedoresR = monthPassivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesR = monthPassivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosR = monthPassivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal ativoOperacionalR = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                decimal passivoOperacionalR = monthPassivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

                decimal ncgR = ativoOperacionalR - passivoOperacionalR;
                decimal realizavelLP_R = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLP_R = monthPassivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixosR = monthAtivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoR = disponibilidadeR + ncgR + realizavelLP_R - exigivelLP_R + ativosFixosR;
                decimal patrimonioLiquidoR = monthPassivoR_M?.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                decimal roiR = capitalInvestidoR != 0 ? (nopatR / capitalInvestidoR) * 100 : 0;
                decimal roeR = patrimonioLiquidoR != 0 ? (lucroLiquidoR / patrimonioLiquidoR) * 100 : 0;
                decimal roeInicialR = patrimonioLiquidoAnoAnterior != 0 ? (lucroLiquidoR / patrimonioLiquidoAnoAnterior) * 100 : 0;

                // 🔹 ORÇADO
                var monthAtivoO_M = painelAtivoO.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoO_M = painelPassivoO.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthDRE_O_M = painelDRE_O.Months.FirstOrDefault(m => m.DateMonth == mes);

                decimal lucroLiquidoO = monthDRE_O_M?.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;
                decimal nopatO = monthDRE_O_M?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal disponibilidadeO = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientesO = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoqueO = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosO = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedoresO = monthPassivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesO = monthPassivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosO = monthPassivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal ativoOperacionalO = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                decimal passivoOperacionalO = monthPassivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

                decimal ncgO = ativoOperacionalO - passivoOperacionalO;
                decimal realizavelLP_O = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLP_O = monthPassivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixosO = monthAtivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoO = disponibilidadeO + ncgO + realizavelLP_O - exigivelLP_O + ativosFixosO;
                decimal patrimonioLiquidoO = monthPassivoO_M?.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                decimal roiO = capitalInvestidoO != 0 ? (nopatO / capitalInvestidoO) * 100 : 0;
                decimal roeO = patrimonioLiquidoO != 0 ? (lucroLiquidoO / patrimonioLiquidoO) * 100 : 0;
                decimal roeInicialO = patrimonioLiquidoAnoAnterior != 0 ? (lucroLiquidoO / patrimonioLiquidoAnoAnterior) * 100 : 0;

                // 🔹 VARIAÇÃO
                decimal variacaoROI = roiR - roiO;
                decimal variacaoROE = roeR - roeO;
                decimal variacaoROEInicial = roeInicialR - roeInicialO;

                // 🔹 ADICIONA AO RESULTADO
                lista.Add(new RentabilityComparativoMesDto
                {
                    Name = new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    Realizado = new RentabilityItemDto
                    {
                        ROI = Math.Round(roiR, 2),
                        LiquidoMensalROE = Math.Round(roeR, 2),
                        LiquidoInicioROE = Math.Round(roeInicialR, 2)
                    },
                    Orcado = new RentabilityItemDto
                    {
                        ROI = Math.Round(roiO, 2),
                        LiquidoMensalROE = Math.Round(roeO, 2),
                        LiquidoInicioROE = Math.Round(roeInicialO, 2)
                    },
                    Variacao = new RentabilityVariacaoDto
                    {
                        ROI = Math.Round(variacaoROI, 2),
                        LiquidoMensalROE = Math.Round(variacaoROE, 2),
                        LiquidoInicioROE = Math.Round(variacaoROEInicial, 2)
                    }
                });
            }

            // 🔹 TOTAL ACUMULADO
            lista.Add(new RentabilityComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new RentabilityItemDto
                {
                    ROI = lista.Sum(x => x.Realizado.ROI),
                    LiquidoMensalROE = lista.Sum(x => x.Realizado.LiquidoMensalROE),
                    LiquidoInicioROE = lista.Sum(x => x.Realizado.LiquidoInicioROE)
                },
                Orcado = new RentabilityItemDto
                {
                    ROI = lista.Sum(x => x.Orcado.ROI),
                    LiquidoMensalROE = lista.Sum(x => x.Orcado.LiquidoMensalROE),
                    LiquidoInicioROE = lista.Sum(x => x.Orcado.LiquidoInicioROE)
                },
                Variacao = new RentabilityVariacaoDto
                {
                    ROI = lista.Sum(x => x.Variacao.ROI),
                    LiquidoMensalROE = lista.Sum(x => x.Variacao.LiquidoMensalROE),
                    LiquidoInicioROE = lista.Sum(x => x.Variacao.LiquidoInicioROE)
                }
            });

            return new PainelRentabilityComparativoResponseDto
            {
                Months = lista
            };
        }

        #endregion

        #region Expectativa de Retorno

        public async Task<PainelReturnExpectationResponseDto> GetReturnExpectation(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var painelAtivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);

            var dezembroAnoAnterior = painelAtivoAnoAnterior.Months
                .FirstOrDefault(a => a.DateMonth == 12);

            decimal patrimonioLiquidoAnoAnterior = dezembroAnoAnterior?.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var returnExpectations = new List<ReturnExpectationResponseDto>();

            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = parameter
                .FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            wacc = wacc / 12;
            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal disponibilidade = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

                decimal clientes = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;

                decimal estoque = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

                decimal outrosAtivosOperacionaisTotal = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedores = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;

                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal somaAtivos = disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal;

                decimal somaPassivo = fornecedores + obrigacoesTributariasETrabalhistas + outrosPassivosOperacionaisTotal;

                decimal necessidadeDeCapitalDeGiro = somaAtivos - somaPassivo;

                decimal realizavelLongoPrazo = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                decimal exigivelLongoPrazo = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;

                decimal ativosFixos = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoLiquido = necessidadeDeCapitalDeGiro + realizavelLongoPrazo + exigivelLongoPrazo + ativosFixos;

                decimal nOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                // Evitar divisão por zero e calcular ROIC já em percentual
                decimal roic = capitalInvestidoLiquido != 0 ? (nOPAT / capitalInvestidoLiquido) * 100 : 0;

                // O WACC deve estar já no formato percentual, se estiver decimal, ajuste aqui:
                // Exemplo: wacc = wacc * 100; (se necessário)

                decimal criacaoValor = roic - wacc;




                returnExpectations.Add(new ReturnExpectationResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    ROIC = Math.Round(roic, 2),          // opcional arredondar 2 casas decimais
                    KE = Math.Round(wacc, 2),
                    CriacaoValor = Math.Round(criacaoValor, 2),
                });
            }

            return new PainelReturnExpectationResponseDto
            {
                ReturnExpectation = new ReturnExpectationGroupedDto
                {
                    Months = returnExpectations
                }
            };
        }

        public async Task<PainelReturnExpectationComparativoResponseDto> GetReturnExpectationComparativo(int accountPlanId, int year)
        {
            var painelAtivoRealizado = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivoRealizado = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRERealizado = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var painelAtivoOrcado = await BuildPainelBalancoReclassificadoByTypeAtivoOrcado(accountPlanId, year, 1);
            var painelPassivoOrcado = await BuildPainelBalancoReclassificadoByTypePassivoOrcado(accountPlanId, year, 2);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            wacc = wacc / 12;

            var lista = new List<ReturnExpectationComparativoMesDto>();

            for (int mes = 1; mes <= 12; mes++)
            {
                // 🔹 REALIZADO
                var monthAtivoR = painelAtivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoR = painelPassivoRealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthDRER = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == mes);

                // --- Capital Investido Realizado
                decimal disponibilidadeR = monthAtivoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientesR = monthAtivoR?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoqueR = monthAtivoR?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosR = monthAtivoR?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedoresR = monthPassivoR?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesR = monthPassivoR?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosR = monthPassivoR?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal necessidadeDeCapitalGiroR = (disponibilidadeR + clientesR + estoqueR + outrosAtivosR) - (fornecedoresR + obrigacoesR + outrosPassivosR);

                decimal realizavelLP_R = monthAtivoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLP_R = monthPassivoR?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixosR = monthAtivoR?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoR = necessidadeDeCapitalGiroR + realizavelLP_R + exigivelLP_R + ativosFixosR;

                decimal nOPATR = monthDRER?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;
                decimal roicR = capitalInvestidoR != 0 ? (nOPATR / capitalInvestidoR) * 100 : 0;
                decimal criacaoValorR = roicR - wacc;

                // 🔹 ORÇADO
                var monthAtivoO = painelAtivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthPassivoO = painelPassivoOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthDREO = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                decimal disponibilidadeO = monthAtivoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientesO = monthAtivoO?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoqueO = monthAtivoO?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosO = monthAtivoO?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedoresO = monthPassivoO?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesO = monthPassivoO?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosO = monthPassivoO?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal necessidadeDeCapitalGiroO = (disponibilidadeO + clientesO + estoqueO + outrosAtivosO) - (fornecedoresO + obrigacoesO + outrosPassivosO);

                decimal realizavelLP_O = monthAtivoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLP_O = monthPassivoO?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixosO = monthAtivoO?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoO = necessidadeDeCapitalGiroO + realizavelLP_O + exigivelLP_O + ativosFixosO;

                decimal nOPATO = monthDREO?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;
                decimal roicO = capitalInvestidoO != 0 ? (nOPATO / capitalInvestidoO) * 100 : 0;
                decimal criacaoValorO = roicO - wacc;

                // 🔹 VARIAÇÃO
                decimal variacaoROIC = roicR - roicO;
                decimal variacaoCriacaoValor = criacaoValorR - criacaoValorO;
                decimal variacaoPercentualCriacaoValor = criacaoValorO != 0 ? (variacaoCriacaoValor / criacaoValorO) * 100 : 0;

                // 🔹 ADICIONA AO RESULTADO
                lista.Add(new ReturnExpectationComparativoMesDto
                {
                    Name = new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    Realizado = new ReturnExpectationItemDto
                    {
                        ROIC = Math.Round(roicR, 2),
                        KE = Math.Round(wacc, 2),
                        CriacaoValor = Math.Round(criacaoValorR, 2)
                    },
                    Orcado = new ReturnExpectationItemDto
                    {
                        ROIC = Math.Round(roicO, 2),
                        KE = Math.Round(wacc, 2),
                        CriacaoValor = Math.Round(criacaoValorO, 2)
                    },
                    Variacao = new ReturnExpectationVariacaoDto
                    {
                        ROIC = Math.Round(variacaoROIC, 2),
                        CriacaoValor = Math.Round(variacaoCriacaoValor, 2),
                        VariacaoPercentualCriacaoValor = Math.Round(variacaoPercentualCriacaoValor, 2)
                    }
                });
            }

            // 🔹 TOTAL ACUMULADO
            lista.Add(new ReturnExpectationComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new ReturnExpectationItemDto
                {
                    ROIC = lista.Sum(x => x.Realizado.ROIC),
                    KE = wacc,
                    CriacaoValor = lista.Sum(x => x.Realizado.CriacaoValor)
                },
                Orcado = new ReturnExpectationItemDto
                {
                    ROIC = lista.Sum(x => x.Orcado.ROIC),
                    KE = wacc,
                    CriacaoValor = lista.Sum(x => x.Orcado.CriacaoValor)
                },
                Variacao = new ReturnExpectationVariacaoDto
                {
                    ROIC = lista.Sum(x => x.Variacao.ROIC),
                    CriacaoValor = lista.Sum(x => x.Variacao.CriacaoValor),
                    VariacaoPercentualCriacaoValor = lista.Sum(x => x.Variacao.VariacaoPercentualCriacaoValor)
                }
            });

            return new PainelReturnExpectationComparativoResponseDto
            {
                Months = lista
            };
        }








        #endregion
        #region EBITDA
        public async Task<PainelEBITDAResponseDto> GetEBITDA(int accountPlanId, int year)
        {
            // Monta o painel DRE mensalizado com acumulado
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // Monta o balanço reclassificado do ano anterior
            var painelAtivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);


            // Lista dos meses do EBITDA
            var ebitdaList = new List<EBITDAResponseDto>();

            // Itera pelos meses normais (exclui o acumulado do painel DRE)
            foreach (var monthDRE in painelDRE.Months.Where(m => m.DateMonth != 13).OrderBy(m => m.DateMonth))
            {
                decimal lucroAntesDoResultadoFinanceiro = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal despesasComDepreciacao = monthDRE.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Despesas com Depreciação")
                    .Sum(c => c.Value);

                decimal ebitda = lucroAntesDoResultadoFinanceiro + despesasComDepreciacao;

                ebitdaList.Add(new EBITDAResponseDto
                {
                    Name = monthDRE.Name,
                    DateMonth = monthDRE.DateMonth,
                    LucroOperacionalAntesDoResultadoFinanceiro = lucroAntesDoResultadoFinanceiro,
                    DespesasDepreciacao = despesasComDepreciacao,
                    EBITDA = ebitda
                });
            }

            // 🔢 Totalizador geral (acumulado do ano)
            var totalGeral = new EBITDAResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                LucroOperacionalAntesDoResultadoFinanceiro = ebitdaList.Sum(x => x.LucroOperacionalAntesDoResultadoFinanceiro),
                DespesasDepreciacao = ebitdaList.Sum(x => x.DespesasDepreciacao),
                EBITDA = ebitdaList.Sum(x => x.EBITDA)
            };

            ebitdaList.Add(totalGeral);

            // ✅ Retorno final
            return new PainelEBITDAResponseDto
            {
                EBITDA = new EBITDAGroupedDto
                {
                    Months = ebitdaList
                }
            };
        }
        public async Task<PainelEBITDAResponseDto> GetEBITDAOrcado(int accountPlanId, int year)
        {
            // Monta o painel DRE mensalizado com acumulado
            var painelDRE = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            // Monta o balanço reclassificado do ano anterior
            var painelAtivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);

            // Lista dos meses do EBITDA
            var ebitdaList = new List<EBITDAResponseDto>();

            // Itera pelos meses normais (exclui o acumulado do painel DRE)
            foreach (var monthDRE in painelDRE.Months.Where(m => m.DateMonth != 13).OrderBy(m => m.DateMonth))
            {
                decimal lucroAntesDoResultadoFinanceiro = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal despesasComDepreciacao = monthDRE.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Despesas com Depreciação")
                    .Sum(c => c.Value);

                decimal ebitda = lucroAntesDoResultadoFinanceiro + despesasComDepreciacao;

                ebitdaList.Add(new EBITDAResponseDto
                {
                    Name = monthDRE.Name,
                    DateMonth = monthDRE.DateMonth,
                    LucroOperacionalAntesDoResultadoFinanceiro = lucroAntesDoResultadoFinanceiro,
                    DespesasDepreciacao = despesasComDepreciacao,
                    EBITDA = ebitda
                });
            }

            // 🔢 Totalizador geral (acumulado do ano)
            var totalGeral = new EBITDAResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                LucroOperacionalAntesDoResultadoFinanceiro = ebitdaList.Sum(x => x.LucroOperacionalAntesDoResultadoFinanceiro),
                DespesasDepreciacao = ebitdaList.Sum(x => x.DespesasDepreciacao),
                EBITDA = ebitdaList.Sum(x => x.EBITDA)
            };

            ebitdaList.Add(totalGeral);

            // ✅ Retorno final
            return new PainelEBITDAResponseDto
            {
                EBITDA = new EBITDAGroupedDto
                {
                    Months = ebitdaList
                }
            };
        }


        public async Task<PainelEBITDAComparativoResponseDto> GetEBITDAComparativo(int accountPlanId, int year)
        {
            // 🔹 Painel Realizado e Orçado
            var painelDRERealizado = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            var meses = Enumerable.Range(1, 12).ToList();
            var lista = new List<EBITDAComparativoMesDto>();

            foreach (var mes in meses)
            {
                var monthRealizado = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthOrcado = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                // --- 🟩 REALIZADO ---
                decimal lucroAntesFinanceiroRealizado = monthRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal depRealizada = monthRealizado?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Despesas com Depreciação")
                    .Sum(c => c.Value) ?? 0;

                decimal ebitdaRealizado = lucroAntesFinanceiroRealizado + depRealizada;

                // --- 🟦 ORÇADO ---
                decimal lucroAntesFinanceiroOrcado = monthOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal depOrcada = monthOrcado?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Despesas com Depreciação")
                    .Sum(c => c.Value) ?? 0;

                decimal ebitdaOrcado = lucroAntesFinanceiroOrcado + depOrcada;

                // --- 🔸 VARIAÇÕES ---
                decimal variacaoLucro = lucroAntesFinanceiroRealizado - lucroAntesFinanceiroOrcado;
                decimal variacaoDep = depRealizada - depOrcada;
                decimal variacaoEbitda = ebitdaRealizado - ebitdaOrcado;
                decimal variacaoPercentual = ebitdaOrcado != 0 ? (variacaoEbitda / ebitdaOrcado) * 100 : 0;

                // --- 🧩 ADICIONAR AO RESULTADO ---
                lista.Add(new EBITDAComparativoMesDto
                {
                    Name = new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    Realizado = new EBITDAItemDto
                    {
                        LucroAntesFinanceiro = lucroAntesFinanceiroRealizado,
                        Depreciacao = depRealizada,
                        EBITDA = ebitdaRealizado
                    },
                    Orcado = new EBITDAItemDto
                    {
                        LucroAntesFinanceiro = lucroAntesFinanceiroOrcado,
                        Depreciacao = depOrcada,
                        EBITDA = ebitdaOrcado
                    },
                    Variacao = new EBITDAItemDto
                    {
                        LucroAntesFinanceiro = variacaoLucro,
                        Depreciacao = variacaoDep,
                        EBITDA = variacaoEbitda,
                    }
                });
            }

            // 🔹 TOTAL ACUMULADO
            lista.Add(new EBITDAComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new EBITDAItemDto
                {
                    LucroAntesFinanceiro = lista.Sum(x => x.Realizado.LucroAntesFinanceiro),
                    Depreciacao = lista.Sum(x => x.Realizado.Depreciacao),
                    EBITDA = lista.Sum(x => x.Realizado.EBITDA)
                },
                Orcado = new EBITDAItemDto
                {
                    LucroAntesFinanceiro = lista.Sum(x => x.Orcado.LucroAntesFinanceiro),
                    Depreciacao = lista.Sum(x => x.Orcado.Depreciacao),
                    EBITDA = lista.Sum(x => x.Orcado.EBITDA)
                },
                Variacao = new EBITDAItemDto
                {
                    LucroAntesFinanceiro = lista.Sum(x => x.Variacao.LucroAntesFinanceiro),
                    Depreciacao = lista.Sum(x => x.Variacao.Depreciacao),
                    EBITDA = lista.Sum(x => x.Variacao.EBITDA)
                }
            });

            return new PainelEBITDAComparativoResponseDto { Months = lista };
        }



        #endregion

        #region NOPAT
        public async Task<PainelNOPATResponseDto> GetNOPAT(int accountPlanId, int year)
        {
            // Monta o painel DRE mensalizado com acumulado
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // Monta o balanço reclassificado do ano anterior
            var painelAtivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);

            var dezembroAnoAnterior = painelAtivoAnoAnterior.Months
                .FirstOrDefault(a => a.DateMonth == 12);

            decimal patrimonioLiquidoAnoAnterior = dezembroAnoAnterior?.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            // Pega a margem operacional acumulada do mês "ACUMULADO"
            var acumuladoMes = painelDRE.Months.FirstOrDefault(m => m.DateMonth == 13);
            decimal margemOperacionalAcumulada = acumuladoMes?.Totalizer
                .FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0;

            var nOPAT = new List<NOPATResponseDto>();

            foreach (var monthDRE in painelDRE.Months.Where(m => m.DateMonth != 13).OrderBy(m => m.DateMonth))
            {
                decimal lucroAntesDoResultadoFinanceiro = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal despesasComDepreciacao = monthDRE.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Despesas com Depreciação")
                    .Sum(c => c.Value);

                decimal provisaoCSLL = monthDRE.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Provisão para CSLL")
                    .Sum(c => c.Value);

                decimal provisaoIRPJ = monthDRE.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Provisão para IRPJ")
                    .Sum(c => c.Value);

                decimal provisaoIRPSCSLL = provisaoIRPJ + provisaoCSLL;

                decimal noPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal margemNOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal margemOperacionalMes = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0;

                nOPAT.Add(new NOPATResponseDto
                {
                    Name = monthDRE.Name,
                    DateMonth = monthDRE.DateMonth,
                    MargemNOPAT = margemNOPAT,
                    LucroOperacionalAntes = lucroAntesDoResultadoFinanceiro,
                    MargemOperacionalDRE = margemOperacionalMes, // <- mês mantém sua própria margem
                    ProvisaoIRPJCSLL = provisaoIRPSCSLL,
                    NOPAT = noPAT
                });
            }

            // 🔢 Totalizador geral (acumulado do ano)
            var totalGeral = new NOPATResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                NOPAT = nOPAT.Sum(x => x.NOPAT),
                LucroOperacionalAntes = nOPAT.Sum(x => x.LucroOperacionalAntes),
                ProvisaoIRPJCSLL = nOPAT.Sum(x => x.ProvisaoIRPJCSLL),
                MargemNOPAT = 0,
                MargemOperacionalDRE = margemOperacionalAcumulada // <- apenas aqui usa a margem acumulada
            };

            nOPAT.Add(totalGeral);

            return new PainelNOPATResponseDto
            {
                NOPAT = new NOPATGroupedDto
                {
                    Months = nOPAT
                }
            };
        }
        public async Task<PainelNOPATComparativoResponseDto> GetNOPATComparativo(int accountPlanId, int year)
        {
            // Painel Realizado e Orçado (meses 1..12 e acumulado em 13)
            var painelDRERealizado = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var painelDREOrcado = await BuildPainelByTypeDREOrcado(accountPlanId, year, 3);

            var meses = Enumerable.Range(1, 12).ToList();
            var lista = new List<NOPATComparativoMesDto>();

            foreach (var mes in meses)
            {
                var monthRealizado = painelDRERealizado.Months.FirstOrDefault(m => m.DateMonth == mes);
                var monthOrcado = painelDREOrcado.Months.FirstOrDefault(m => m.DateMonth == mes);

                // --- REALIZADO ---
                decimal lucroAntesReal = monthRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal provisaoIRPJReal = monthRealizado?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Provisão para IRPJ")
                    .Sum(c => c.Value) ?? 0;

                decimal provisaoCSLLReal = monthRealizado?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Provisão para CSLL")
                    .Sum(c => c.Value) ?? 0;

                decimal provisaoTotalReal = provisaoIRPJReal + provisaoCSLLReal;

                decimal margemOperacionalReal = monthRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0;

                decimal margemNOPATReal = monthRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal nopatReal = monthRealizado?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? (lucroAntesReal - provisaoTotalReal);

                // --- ORÇADO ---
                decimal lucroAntesOrc = monthOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                decimal provisaoIRPJOrc = monthOrcado?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Provisão para IRPJ")
                    .Sum(c => c.Value) ?? 0;

                decimal provisaoCSLLOrc = monthOrcado?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .Where(c => c.Name == "Provisão para CSLL")
                    .Sum(c => c.Value) ?? 0;

                decimal provisaoTotalOrc = provisaoIRPJOrc + provisaoCSLLOrc;

                decimal margemOperacionalOrc = monthOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0;

                decimal margemNOPATOrc = monthOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal nopatOrc = monthOrcado?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? (lucroAntesOrc - provisaoTotalOrc);

                // --- VARIAÇÃO ---
                decimal variacaoLucro = lucroAntesReal - lucroAntesOrc;
                decimal variacaoProvisao = provisaoTotalReal - provisaoTotalOrc;
                decimal variacaoNOPAT = nopatReal - nopatOrc;
                decimal variacaoPercentualNOPAT = nopatOrc != 0 ? (variacaoNOPAT / nopatOrc) * 100 : 0;

                // --- ADICIONA À LISTA ---
                lista.Add(new NOPATComparativoMesDto
                {
                    Name = new DateTime(year, mes, 1).ToString("MMMM").ToUpper(),
                    DateMonth = mes,
                    Realizado = new NOPATItemDto
                    {
                        LucroOperacionalAntes = lucroAntesReal,
                        MargemOperacionalDRE = margemOperacionalReal,
                        ProvisaoIRPJCSLL = provisaoTotalReal,
                        MargemNOPAT = margemNOPATReal,
                        NOPAT = nopatReal
                    },
                    Orcado = new NOPATItemDto
                    {
                        LucroOperacionalAntes = lucroAntesOrc,
                        MargemOperacionalDRE = margemOperacionalOrc,
                        ProvisaoIRPJCSLL = provisaoTotalOrc,
                        MargemNOPAT = margemNOPATOrc,
                        NOPAT = nopatOrc
                    },
                    Variacao = new NOPATItemDto
                    {
                        LucroOperacionalAntes = variacaoLucro,
                        MargemOperacionalDRE = margemOperacionalReal - margemOperacionalOrc,
                        ProvisaoIRPJCSLL = variacaoProvisao,
                        MargemNOPAT = margemNOPATReal - margemNOPATOrc,
                        NOPAT = variacaoNOPAT,
                        VariacaoPercentual = variacaoPercentualNOPAT
                    }
                });
            }

            // --- TOTAL ACUMULADO ---
            lista.Add(new NOPATComparativoMesDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                Realizado = new NOPATItemDto
                {
                    LucroOperacionalAntes = lista.Sum(x => x.Realizado.LucroOperacionalAntes),
                    MargemOperacionalDRE = lista.Sum(x => x.Realizado.MargemOperacionalDRE), // soma de % pode não fazer sentido estatístico, mas segue padrão do EBITDA
                    ProvisaoIRPJCSLL = lista.Sum(x => x.Realizado.ProvisaoIRPJCSLL),
                    MargemNOPAT = lista.Sum(x => x.Realizado.MargemNOPAT),
                    NOPAT = lista.Sum(x => x.Realizado.NOPAT)
                },
                Orcado = new NOPATItemDto
                {
                    LucroOperacionalAntes = lista.Sum(x => x.Orcado.LucroOperacionalAntes),
                    MargemOperacionalDRE = lista.Sum(x => x.Orcado.MargemOperacionalDRE),
                    ProvisaoIRPJCSLL = lista.Sum(x => x.Orcado.ProvisaoIRPJCSLL),
                    MargemNOPAT = lista.Sum(x => x.Orcado.MargemNOPAT),
                    NOPAT = lista.Sum(x => x.Orcado.NOPAT)
                },
                Variacao = new NOPATItemDto
                {
                    LucroOperacionalAntes = lista.Sum(x => x.Variacao.LucroOperacionalAntes),
                    MargemOperacionalDRE = lista.Sum(x => x.Variacao.MargemOperacionalDRE),
                    ProvisaoIRPJCSLL = lista.Sum(x => x.Variacao.ProvisaoIRPJCSLL),
                    MargemNOPAT = lista.Sum(x => x.Variacao.MargemNOPAT),
                    NOPAT = lista.Sum(x => x.Variacao.NOPAT),
                    VariacaoPercentual = lista.Sum(x => x.Orcado.NOPAT) != 0
                        ? (lista.Sum(x => x.Variacao.NOPAT) / lista.Sum(x => x.Orcado.NOPAT)) * 100
                        : 0
                }
            });

            return new PainelNOPATComparativoResponseDto { Months = lista };
        }


        #endregion

        #region Dados

        private async Task<PainelBalancoContabilRespone> BuildPainelByTypeAtivo(int accountPlanId, int year, int typeClassification)
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

            var months = balancetes.Select(balancete =>
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

                return new MonthPainelContabilRespone
                {
                    Id = balancete.Id,
                    Name = balancete.DateMonth.GetDescription(),
                    DateMonth = (int)balancete.DateMonth,
                    Totalizer = totalizerResponses,
                    MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                    {
                        Name = "TOTAL GERAL DO ATIVO",
                        TotalValue = totalizerResponses.Sum(t => t.TotalValue)
                    }

                };
            }).OrderBy(a => a.DateMonth).ToList();


            return new PainelBalancoContabilRespone { Months = months };
        }
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


    }
    #endregion
}





