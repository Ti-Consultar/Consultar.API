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
        private readonly TotalizerClassificationRepository _totalizerClassificationRepository;
        private readonly TotalizerClassificationTemplateRepository _totalizerClassificationTemplateRepository;
        private readonly BalancoReclassificadoTemplateRepository _balancoReclassificadoTemplateRepository;
        private readonly BalancoReclassificadoRepository _balancoReclassificadoRepository;
        private readonly AccountPlansRepository _accountPlansRepository;

        public EconomicIndicesService(
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
        #region Lucratividade

        public async Task<PainelProfitabilityResponseDto> GetProfitability(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var profitabilities = new List<ProfitabilityResponseDto>();

            decimal? ncgMesAnterior = null; // <- inicializa como nulo

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal margemBruta = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem Bruta %")?.TotalValue ?? 0;

                decimal margemEbitda = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                decimal margemOperacional = monthDRE?.Totalizer
                      .FirstOrDefault(t => t.Name == "Margem Operacional %")?.TotalValue ?? 0;

                decimal margemNOPAT = monthDRE?.Totalizer
                     .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;

                decimal margemLAIR = monthDRE?.Totalizer
                     .FirstOrDefault(t => t.Name == "Margem LAIR %")?.TotalValue ?? 0;

                profitabilities.Add(new ProfitabilityResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    MargemBruta = margemBruta,
                    MargemEBITDA = margemEbitda,
                    MargemOperacional = margemOperacional,
                    MargemNOPAT = margemNOPAT,
                    MargemLiquida = margemLAIR
                });
            }

            return new PainelProfitabilityResponseDto
            {
                Profitability = new ProfitabilityGroupedDto
                {
                    Months = profitabilities
                }
            };
        }
        #endregion

        #region Rentabilidade
        public async Task<PainelRentabilityResponseDto> GetRentabilibty(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // 🔁 Carrega também o painel do ANO ANTERIOR (somente o Ativo é necessário aqui)
            var painelAtivoAnoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);

            var dezembroAnoAnterior = painelAtivoAnoAnterior.Months
                .FirstOrDefault(a => a.DateMonth == 12);

            decimal patrimonioLiquidoAnoAnterior = dezembroAnoAnterior?.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var rentabilities = new List<RentabilityResponseDto>();

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal lucroLiquido = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                decimal ativoTotal = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Total Ativo Circulante")?.TotalValue ?? 0;

                decimal patrimonioLiquido = monthAtivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                // ⚠️ Evita divisão por zero
                decimal roi = ativoTotal != 0 ? lucroLiquido / ativoTotal : 0;
                decimal roe = patrimonioLiquido != 0 ? lucroLiquido / patrimonioLiquido : 0;
                decimal roeInicial = patrimonioLiquidoAnoAnterior != 0 ? lucroLiquido / patrimonioLiquidoAnoAnterior : 0;

                rentabilities.Add(new RentabilityResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    ROI = roi,
                    LiquidoMensalROE = roe,
                    LiquidoInicioROE = roeInicial,
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

        #endregion

        #region Expectativa de Retorno
        #endregion

        #region EBITDA
        #endregion

        #region NOPAT
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
    }
    #endregion
}





