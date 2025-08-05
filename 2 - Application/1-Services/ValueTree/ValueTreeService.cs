using _2___Application._2_Dto_s.CashFlow;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application._2_Dto_s.ValueTree;
using _2___Application.Base;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.ValueTree
{
    public class ValueTreeService : BaseService
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
        private readonly ParameterRepository _parameterRepository;

        public ValueTreeService(
            ClassificationRepository repository,
            AccountPlanClassificationRepository accountClassificationRepository,
            BalanceteDataRepository balanceteDataRepository,
            BalanceteRepository balanceteRepository,
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
            _totalizerClassificationRepository = _talizerClassificationRepository;
            _totalizerClassificationTemplateRepository = totalizerClassificationTemplateRepository;
            _balancoReclassificadoTemplateRepository = balancoReclassificadoTemplateRepository;
            _balancoReclassificadoRepository = balancoReclassificadoRepository;
            _accountPlansRepository = accountPlansRepository;
            _parameterRepository = parameterRepository;
        }

        #region


        public async Task<ValueTreeResultDto> GetAll(int accountPlanId, int month, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            var monthAtivo = painelAtivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == month);
            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            // === Parâmetros iniciais ===
            decimal wacc = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            wacc /= 12;

            // === Custos Variáveis ===
            var custoMercadorias = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;

            var custoServicosPrestados = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;

            var despesasVariaveis = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Despesas Variáveis")?.Value ?? 0;

            var custos = custoMercadorias + custoServicosPrestados + despesasVariaveis;

            // === Despesas Operacionais ===
            var despesasVendas = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Despesas com Vendas")?.Value ?? 0;

            var despesasPessoalEncargos = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Despesas com Pessoal e Encargos")?.Value ?? 0;

            var despesasAdministrativas = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Despesas Administrativas e Gerais")?.Value ?? 0;

            var outrosResultadosOperacionais = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Outros  Resultados Operacionais")?.Value ?? 0;

            var despesasOperacionaisTotalSomaDRE = monthDRE?.Totalizer
                .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;

            var despesasOperacionais = despesasVendas + despesasPessoalEncargos + despesasAdministrativas;

            // === Impostos ===
            var provisaoCSLL = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;

            var provisaoIRPJ = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

            var impostos = provisaoCSLL + provisaoIRPJ;

            // === Ativo Circulante ===
            decimal disponibilidade = monthAtivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

            decimal clientes = monthAtivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;

            decimal estoque = monthAtivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;

            decimal outrosAtivosOperacionaisTotal = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

            // === Passivo Circulante ===
            decimal fornecedores = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;

            decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;

            decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

            // === Longo Prazo e Investimentos ===
            decimal exigivelLongoPrazo = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;

            decimal realizavelLongoPrazo = monthAtivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

            decimal ativosFixos = monthAtivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

            // === Indicadores e Métricas ===
            decimal receitaLiquida = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

            decimal nOPAT = monthDRE?.Totalizer
                .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

            decimal lajir = monthDRE?.Totalizer
                .FirstOrDefault(t => t.Name == "Margem LAJIR %")?.TotalValue ?? 0;

            decimal margemDeContribuicao = monthDRE?.Totalizer
                .FirstOrDefault(t => t.Name == "Margem Contribuição %")?.TotalValue ?? 0;

            // === Cálculo de NCG e Capital Investido ===
            decimal somaAtivos = disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal;
            decimal somaPassivo = fornecedores + obrigacoesTributariasETrabalhistas + outrosPassivosOperacionaisTotal;

            decimal necessidadeDeCapitalDeGiro = somaAtivos + somaPassivo;
            decimal capitalInvestidoLiquido = necessidadeDeCapitalDeGiro + realizavelLongoPrazo + exigivelLongoPrazo + ativosFixos;

            // === ROIC e EVA ===
            decimal roic = capitalInvestidoLiquido != 0 ? (nOPAT / capitalInvestidoLiquido) * 100 : 0;
            decimal evaSPREAD = roic - wacc;
            decimal eva = capitalInvestidoLiquido != 0 ? evaSPREAD / capitalInvestidoLiquido : 0;

            // === Cálculo do CDG ===
            var passivoNaoCirculante = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Passivo Não Circulante")?.TotalValue ?? 0;

            var patrimonioLiquido = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var ativoNaoCirculante = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

            var ativoFixo = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

            var cdg = (passivoNaoCirculante - patrimonioLiquido) + (ativoNaoCirculante + ativoFixo);

            // === DTO Final ===
            var economic = new EconomicViewDto
            {
                ReceitaLiquida = receitaLiquida,
                CustoDespesaVariavel = custos,
                MargemContribuicao = margemDeContribuicao,
                DespesasOperacionais = despesasOperacionaisTotalSomaDRE,
                OutrosResultadosOperacionais = outrosResultadosOperacionais,
                LAJIR = lajir,
                NOPAT = nOPAT,
                Impostos = impostos
            };

            var financial = new FinancialViewDto
            {
                Disponivel = disponibilidade,
                Clientes =clientes,
                Estoques = estoque,
                OutrosAtivosOperacionais = outrosAtivosOperacionaisTotal,
                Fornecedores = fornecedores,
                OutrosPassivosOperacionais = outrosPassivosOperacionaisTotal,
                RealizavelLongoPrazo = realizavelLongoPrazo,
                ExigivelLongoPrazo = exigivelLongoPrazo,
                AtivosFixos = ativosFixos,
                CapitalDeGiro = cdg,
                CapitalInvestido =capitalInvestidoLiquido
            };

            var indicators = new ReturnIndicatorsDto
            {
                NOPAT = nOPAT,
                CapitalInvestido = capitalInvestidoLiquido,
                ROIC = roic,
                WACC = wacc,
                SPREAD = evaSPREAD,
                EVA= eva

                
            };

            return new ValueTreeResultDto
            {
                EconomicView = economic,
                FinancialView = financial,
                Indicators = indicators
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
                receitaLiquida.TotalValue = 0;

                var lucroBruto = totalizerResponses
                  .FirstOrDefault(t => t.Name == "Lucro Bruto");
                lucroBruto.TotalValue = 0;
                var margemContribuicao = totalizerResponses
                 .FirstOrDefault(t => t.Name == "Margem Contribuição");

                var despesasOperacionais = totalizerResponses
                 .FirstOrDefault(t => t.Name == "(-) Despesas Operacionais");

                var lucroOperacional = totalizerResponses
                 .FirstOrDefault(t => t.Name == "Lucro Operacional");

                lucroOperacional.TotalValue = 0;

                var lucroAntes = totalizerResponses
                .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro");

                lucroAntes.TotalValue = 0;

                var resultadoAntes = totalizerResponses
              .FirstOrDefault(t => t.Name == "Resultado do Exercício Antes do Imposto");
                resultadoAntes.TotalValue = 0;

                var lucroLiquido = totalizerResponses
               .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo");

                lucroLiquido.TotalValue = 0;

                var ebitda = totalizerResponses
               .FirstOrDefault(t => t.Name == "EBITDA");

                var nopat = totalizerResponses
               .FirstOrDefault(t => t.Name == "NOPAT");

                // classificaton
                var custoMercadorias = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;

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
               .FirstOrDefault(c => c.Name == "Provisão CSLL")?.Value ?? 0;

                var provisaoIRPJ = totalizerResponses
               .SelectMany(t => t.Classifications)
               .FirstOrDefault(c => c.Name == "Provisão IRPJ")?.Value ?? 0;

                var despesasDepreciacao = totalizerResponses
               .SelectMany(t => t.Classifications)
               .FirstOrDefault(c => c.Name == "Despesas Com Depreciação")?.Value ?? 0;

                var outrosResultadosOperacionais = totalizerResponses
              .SelectMany(t => t.Classifications)
              .FirstOrDefault(c => c.Name == "Outros Resultados Operacionais")?.Value ?? 0;

                despesasOperacionais.TotalValue = despesasOperacionais.TotalValue - outrosResultadosOperacionais;

                // calculos 
                var receitaLiquidaValor = receitaOperacionalBruta + deducoes;
                receitaLiquida.TotalValue = receitaLiquidaValor;
                lucroBruto.TotalValue = receitaLiquidaValor + custoMercadorias;

                margemContribuicao.TotalValue = (lucroBruto?.TotalValue ?? 0) + despesasV;
                lucroOperacional.TotalValue = (lucroBruto?.TotalValue ?? 0) + despesasOperacionais?.TotalValue ?? 0 + outrosResultadosOperacionais;

                lucroAntes.TotalValue = (lucroOperacional?.TotalValue ?? 0) + outrosReceitas + ganhosEPerdas;
                resultadoAntes.TotalValue = (lucroAntes?.TotalValue ?? 0) + receitasFinanceiras + despesasFinanceiras;
                lucroLiquido.TotalValue = (resultadoAntes?.TotalValue ?? 0) + provisaoCSLL + provisaoIRPJ;
                ebitda.TotalValue = (lucroAntes?.TotalValue ?? 0) + despesasDepreciacao;
                nopat.TotalValue = (lucroAntes?.TotalValue ?? 0) + provisaoCSLL + provisaoIRPJ;


                // calculos de Margens


                var margemBruta = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem Bruta %");

                margemBruta.TotalValue = 0;

                margemBruta.TotalValue = receitaLiquida.TotalValue != 0
                    ? Math.Round((lucroBruto.TotalValue / receitaLiquida.TotalValue) * 100, 2)
                    : 0;


                var margemContribuicaoPorcentagem = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem Contribuição %");

                margemContribuicaoPorcentagem.TotalValue = receitaLiquida.TotalValue != 0
                   ? Math.Round((margemContribuicao.TotalValue / receitaLiquida.TotalValue) * 100, 2)
                   : 0;

                var margemOperacional = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem Operacional %");

                margemOperacional.TotalValue = 0;

                margemOperacional.TotalValue = receitaLiquida.TotalValue != 0
                  ? Math.Round((lucroOperacional.TotalValue / receitaLiquida.TotalValue) * 100, 2)
                  : 0;

                var margemLajir = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem LAJIR %");

                margemLajir.TotalValue = 0;

                margemLajir.TotalValue = receitaLiquida.TotalValue != 0
                 ? Math.Round((lucroAntes.TotalValue / receitaLiquida.TotalValue) * 100, 2)
                 : 0;

                var margemLAIR = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem LAIR %");

                margemLAIR.TotalValue = 0;

                margemLAIR.TotalValue = receitaLiquida.TotalValue != 0
                ? Math.Round((resultadoAntes.TotalValue / receitaLiquida.TotalValue) * 100, 2)
                : 0;

                var margemLiquida = totalizerResponses
                  .FirstOrDefault(t => t.Name == "Margem Líquida %");

                margemLiquida.TotalValue = 0;

                margemLiquida.TotalValue = receitaLiquida.TotalValue != 0
               ? Math.Round((lucroLiquido.TotalValue / receitaLiquida.TotalValue) * 100, 2)
               : 0;

                var margemEBITDA = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem EBITDA %");

                margemEBITDA.TotalValue = receitaLiquida.TotalValue != 0
              ? Math.Round((ebitda.TotalValue / receitaLiquida.TotalValue) * 100, 2)
              : 0;

                var margemNOPAT = totalizerResponses
                   .FirstOrDefault(t => t.Name == "Margem NOPAT %");

                margemNOPAT.TotalValue = receitaLiquida.TotalValue != 0
              ? Math.Round((nopat.TotalValue / receitaLiquida.TotalValue) * 100, 2)
              : 0;

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

        #endregion
    }
}