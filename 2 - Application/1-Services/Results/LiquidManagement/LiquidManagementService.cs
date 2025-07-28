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

                var passivoNaoCirculante = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Financeiro")?.TotalValue ?? 0;
                var patrimonioLiquido = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                var ativoNaoCirculante = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Não Circulante Financeiro")?.TotalValue ?? 0;
                var ativoFixo = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                var saldoTesouraria = valorAtivoFinanceiro - valorPassivoFinanceiro;
                var ncg = valorAtivoOperacional - valorPassivoOperacional;
                var cdg = (passivoNaoCirculante + patrimonioLiquido) - (ativoNaoCirculante + ativoFixo);
                decimal? indiceDeLiquidez = ncg != 0 ? saldoTesouraria / ncg : (decimal?)null;

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

        #endregion

        #region Dinâmica do Capital de Giro

        public async Task<PainelCapitalDynamicsResponseDto> GetCapitalDynamics(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var capitalDinamics = new List<CapitalDynamicsResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var receitaMensal = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "Receita Operacional Bruta")?.TotalValue ?? 0;
                var cliente = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedor = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional - valorPassivoOperacional;

                decimal pMR = 0;
                decimal pME = 0;
                decimal pMP = 0;
                decimal cicloNCG = 0;

                if (receitaMensal > 0)
                {
                    int multiplicadorDias = monthAtivo.DateMonth * 30;
                    pMR = (estoque / receitaMensal) * multiplicadorDias;
                    pME = (cliente / receitaMensal) * multiplicadorDias;
                    pMP = (fornecedor / receitaMensal) * multiplicadorDias;
                    cicloNCG = (ncg / receitaMensal) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCG = (pME + pMR) - pMP;

                capitalDinamics.Add(new CapitalDynamicsResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
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


        #endregion

        #region Geração de Fluxo de Caixa Bruto

        public async Task<PainelGrossCashFlowResponseDto> GetGrossCashFlow(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var grossCashFlow = new List<GrossCashFlowResponseDto>();
            decimal? ncgMesAnterior = null;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal ebitda = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "EBITDA")?.TotalValue ?? 0;
                decimal margemEbitda = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                decimal valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                decimal valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;

                decimal ncg = valorAtivoOperacional - valorPassivoOperacional;

                decimal variacaoNCG = ncgMesAnterior.HasValue ? ncg - ncgMesAnterior.Value : 0;
                ncgMesAnterior = ncg;

                decimal fluxoDeCaixaOperacional = variacaoNCG - ebitda;

                var receitaMensal = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "Receita Operacional Bruta")?.TotalValue ?? 0;

                decimal geracaoCaixa = receitaMensal != 0 ? fluxoDeCaixaOperacional / receitaMensal : 0;
                decimal aumentoReducaoFluxoCaixa = margemEbitda != 0 ? geracaoCaixa / margemEbitda : 0;

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



        #endregion

        #region Rotatividade
        public async Task<PainelTurnoverResponseDto> GetTurnover(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var turnover = new List<TurnoverResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                var receitaMensal = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "Receita Operacional Bruta")?.TotalValue ?? 0;
                var cliente = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                var fornecedor = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional - valorPassivoOperacional;

                decimal pMR = 0, pME = 0, pMP = 0, cicloNCG = 0;

                if (receitaMensal > 0)
                {
                    int multiplicadorDias = monthAtivo.DateMonth * 30;

                    pMR = (estoque / receitaMensal) * multiplicadorDias;
                    pME = (cliente / receitaMensal) * multiplicadorDias;
                    pMP = (fornecedor / receitaMensal) * multiplicadorDias;
                    cicloNCG = (ncg / receitaMensal) * multiplicadorDias;
                }

                var cicloFinanceiroOperacoesPrincipaisNCG = (pME + pMR) - pMP;

                var giroPME = pME != 0 ? 30 / pME : 0;
                var giroPMR = pMR != 0 ? 30 / pMR : 0;
                var giroPMP = pMP != 0 ? 30 / pMP : 0;
                var giroCaixa = cicloFinanceiroOperacoesPrincipaisNCG != 0 ? 30 / cicloFinanceiroOperacoesPrincipaisNCG : 0;

                turnover.Add(new TurnoverResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    GiroPME = giroPME,
                    GiroPMP = giroPMR,
                    GiroPMR = giroPMP,
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

        #endregion

        #region Liquidez
        public async Task<PainelLiquidityResponseDto> GetLiquidity(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

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


        #endregion

        #region Estrutura de Capital 
        public async Task<PainelCapitalStructureResponseDto> GetCapitalStructure(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);

            var capitalStructure = new List<CapitalStructureResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal emprestimosACurtoPrazo = monthAtivo.Totalizer
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
                    TerceirosCurtoPrazo = endividamentoTerceirosCurtoPrazo,
                    TerceirosLongoPrazo = endividamentoTerceirosLongoPrazo,
                    ParticipacaoCapitalTerceiros = participacaoCapitaldeTerceiros,
                    ParticipacaoCapitalProprio = participacaoCapitaldeProprio
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

        #endregion

        #region Dados
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

                    return new MonthPainelContabilRespone
                    {
                        Id = balancete.Id,
                        Name = balancete.DateMonth.GetDescription(),
                        DateMonth = (int)balancete.DateMonth,
                        Totalizer = totalizerResponses,
                        MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                        {
                            Name = "TOTAL DO ATIVO",
                            TotalValue = totalizerResponses.Sum(t => t.TotalValue)
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
                 .Where(c => c.TypeOrder >= 18 && c.TypeOrder <= 33)
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
                            var ruleValue = ApplyBalancoReclassificadoTotalPassivoValueRules(totalizer.Name, totalizerMap, classificationMap);
                            if (ruleValue.HasValue)
                                totalizer.TotalValue = ruleValue.Value;
                        }
                    }

                    return new MonthPainelContabilRespone
                    {
                        Id = balancete.Id,
                        Name = balancete.DateMonth.GetDescription(),
                        DateMonth = (int)balancete.DateMonth,
                        Totalizer = totalizerResponses,
                        MonthPainelContabilTotalizer = new MonthPainelContabilTotalizerRespone
                        {
                            Name = "TOTAL DO PASSIVO",
                            TotalValue = totalizerResponses.Sum(t => t.TotalValue)
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



            var totalizerq = await _totalizerClassificationRepository.GetByAccountPlanId(accountPlanId);

            var classificationTotalizerIds = totalizerq
                .Where(c => c.TypeOrder >= 11 && c.TypeOrder <= 30)
                .Select(c => c.Id)
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

                // Map para facilitar acesso rápido
                var totalizerMap = totalizerResponses.ToDictionary(t => t.Name);
                var classificationMap = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .ToDictionary(c => c.Name);

                // Aplicar regras especiais (Lucros, EBITDA, etc.)
                for (int i = 0; i < 3; i++)
                {
                    foreach (var totalizer in totalizerResponses.OrderBy(t => t.TypeOrder))
                    {
                        var ruleValue = ApplyDRETotalValueRules(totalizer.Name, totalizerMap, classificationMap);
                        if (ruleValue.HasValue)
                            totalizer.TotalValue = ruleValue.Value;
                    }
                }

                // Aplicar regras de percentual (%)
                foreach (var totalizer in totalizerResponses)
                {
                    var percentage = ApplyDREPercentageRules(
                        totalizer.Name,
                        totalizerMap,
                        totalizer.TotalValue
                    );

                    if (percentage.HasValue)
                        totalizer.TotalValue = percentage.Value;
                }

                return new MonthPainelContabilRespone
                {
                    Id = balancete.Id,
                    Name = balancete.DateMonth.GetDescription(),
                    DateMonth = (int)balancete.DateMonth,
                    Totalizer = totalizerResponses.OrderBy(t => t.TypeOrder).ToList()

                };

            }).OrderBy(m => m.DateMonth).ToList();

            return new PainelBalancoContabilRespone { Months = months };
        }
        private decimal? ApplyBalancoReclassificadoTotalAtivoValueRules(string name, Dictionary<string, TotalizerParentRespone> totals, Dictionary<string, ClassificationRespone> classes)
        {
            decimal GetValue(string key) =>
                totals.TryGetValue(key, out var t) ? t.TotalValue :
                classes.TryGetValue(key, out var c) ? c.Value : 0;

            return name switch
            {
                "Ativo Financeiro" => GetValue("Caixa e Equivalente de Caixa") + GetValue("Aplicação Financeira"),
                "Ativo Operacional" => GetValue("Clientes") + GetValue("Estoques") + GetValue("Outros Ativos Operacionais"),
                "Outros Ativos Operacionais Total" => GetValue("Outros Ativos Operacionais") + GetValue("Contas Transitórias Ativo"),
                "Ativo Não Circulante" => GetValue("Ativo Não Circulante Financeiro") + GetValue("Ativo Não Circulante Operacional"),
                "Ativo Fixo" => GetValue("Investimentos") + GetValue("Imobilizado") + GetValue("Depreciação / Amort. Acumulada"),

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
                "Passivo Operacional" => GetValue("Fornecedores") + GetValue("Obrigações Trabalhistas") + GetValue("Obrigações Tributárias"),
                "Outros Passivos Operacionais Total" => GetValue("Outros Passivos Operacionais") + GetValue("Contas Transitórias Passivo"),
                "Passivo Não Circulante" => GetValue("Passivo Não Circulante Financeiro") + GetValue("Passivo Não Circulante Operacional"),
                "Patrimônio Liquido" => GetValue("Capital Social") + GetValue("Reservas") + GetValue("Lucros / Prejuízos Acumulado") + GetValue("Distribuição de Lucro") + GetValue("Resultado Acumulado"),

                _ => null
            };
        }

        private decimal? ApplyDRETotalValueRules(string name, Dictionary<string, TotalizerParentRespone> totals, Dictionary<string, ClassificationRespone> classes)
        {
            decimal GetValue(string key) =>
                totals.TryGetValue(key, out var t) ? t.TotalValue :
                classes.TryGetValue(key, out var c) ? c.Value : 0;

            return name switch
            {
                "(=) Receita Líquida de Vendas" => GetValue("Receita Operacional Bruta") + GetValue("(-) Deduções da Receita Bruta"),
                "Lucro Bruto" => GetValue("(=) Receita Líquida de Vendas") + GetValue("(-) Custos das Mercadorias"),
                "Margem Contribuição" => GetValue("Lucro Bruto") + GetValue("Despesas Variáveis"),
                "Lucro Operacional" => GetValue("Lucro Bruto") + GetValue("(-) Despesas Operacionais") + GetValue("Outros  Resultados Operacionais"),
                "Lucro Antes do Resultado Financeiro" => GetValue("Lucro Operacional") + GetValue("Outras Receitas não Operacionais") + GetValue("Ganhos e Perdas de Capital"),
                "Resultado do Exercício Antes do Imposto" => GetValue("Lucro Antes do Resultado Financeiro") + GetValue("Receitas Financeiras") + GetValue("Despesas Financeiras"),
                "Lucro Líquido do Periodo" => GetValue("Resultado do Exercício Antes do Imposto") + GetValue("Provisão para CSLL") + GetValue("Provisão para IRPJ"),
                "EBITDA" => GetValue("Lucro Antes do Resultado Financeiro") + GetValue("Despesas com Depreciação"),
                "NOPAT" => GetValue("Lucro Antes do Resultado Financeiro") + GetValue("Provisão para CSLL") + GetValue("Provisão para IRPJ"),
                _ => null
            };
        }

        private decimal? ApplyDREPercentageRules(string name, Dictionary<string, TotalizerParentRespone> totals, decimal? totalValue)
        {
            decimal Get(string key) => totals.TryGetValue(key, out var t) ? t.TotalValue : 0;

            return name switch
            {
                "Margem Bruta %" => SafeDivide(Get("Lucro Bruto"), Get("(=) Receita Líquida de Vendas")),
                "Margem de Contribuição %" => SafeDivide(Get("Margem Contribuição"), Get("(=) Receita Líquida de Vendas")),
                "Margem Operacional %" => SafeDivide(Get("Lucro Operacional"), Get("(=) Receita Líquida de Vendas")),
                "Margem LAJIR %" => SafeDivide(Get("Lucro Antes do Resultado Financeiro"), Get("(=) Receita Líquida de Vendas")),
                "Margem LAIR %" => SafeDivide(Get("Resultado do Exercício Antes do Imposto"), Get("(=) Receita Líquida de Vendas")),
                "Margem Líquida %" => SafeDivide(Get("Lucro Líquido do Periodo"), Get("(=) Receita Líquida de Vendas")),
                "Margem EBITDA %" => SafeDivide(Get("EBITDA"), Get("(=) Receita Líquida de Vendas")),
                "Margem NOPAT %" => SafeDivide(Get("NOPAT"), Get("(=) Receita Líquida de Vendas")),
                _ => null
            };
        }
        #endregion
    }
}
