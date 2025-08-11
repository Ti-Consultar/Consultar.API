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

        public async Task<ValueTreeResultDto> GettAll(int accountPlanId, int month, int year)
        {
            // === Painéis completos do ano ===
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);

            // === Valores do mês selecionado ===
            var monthAtivo = painelAtivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == month);

            // === Acumulados do ano ===
            var acumuladoAtivo = painelAtivo.Months
                .SelectMany(m => m.Totalizer)
                .GroupBy(t => t.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalValue));

            var acumuladoPassivo = painelPassivo.Months
                .SelectMany(m => m.Totalizer)
                .GroupBy(t => t.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalValue));

            var acumuladoDRE = painelDRE.Months
                .SelectMany(m => m.Totalizer)
                .GroupBy(t => t.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalValue));

            var acumuladoClassDRE = painelDRE.Months
                .SelectMany(m => m.Totalizer.SelectMany(t => t.Classifications))
                .GroupBy(c => c.Name)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Value));

            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = (parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0) / 12;

            // === Custos Variáveis ===
            decimal custoMercadoriasMes = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;
            decimal custoMercadoriasAcum = acumuladoClassDRE.ContainsKey("(-) Custos das Mercadorias")
                ? acumuladoClassDRE["(-) Custos das Mercadorias"] : 0;

            decimal custoServicosMes = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;
            decimal custoServicosAcum = acumuladoClassDRE.ContainsKey("(-) Custos dos Serviços Prestados")
                ? acumuladoClassDRE["(-) Custos dos Serviços Prestados"] : 0;

            decimal despesasVariaveisMes = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Despesas Variáveis")?.Value ?? 0;
            decimal despesasVariaveisAcum = acumuladoClassDRE.ContainsKey("Despesas Variáveis")
                ? acumuladoClassDRE["Despesas Variáveis"] : 0;

            decimal custosMes = custoMercadoriasMes + custoServicosMes + despesasVariaveisMes;
            decimal custosAcum = custoMercadoriasAcum + custoServicosAcum + despesasVariaveisAcum;

            // === Despesas Operacionais ===
            decimal despesasOpMes = monthDRE?.Totalizer
                .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;
            decimal despesasOpAcum = acumuladoDRE.ContainsKey("(-) Despesas Operacionais")
                ? acumuladoDRE["(-) Despesas Operacionais"] : 0;

            decimal outrosResOpMes = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Outros  Resultados Operacionais")?.Value ?? 0;
            decimal outrosResOpAcum = acumuladoClassDRE.ContainsKey("Outros  Resultados Operacionais")
                ? acumuladoClassDRE["Outros  Resultados Operacionais"] : 0;

            // === Impostos ===
            decimal impostosMes = (monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0)
                + (monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0);

            decimal impostosAcum = (acumuladoClassDRE.ContainsKey("Provisão para CSLL") ? acumuladoClassDRE["Provisão para CSLL"] : 0)
                + (acumuladoClassDRE.ContainsKey("Provisão para IRPJ") ? acumuladoClassDRE["Provisão para IRPJ"] : 0);

            // === Ativos e Passivos ===
            decimal disponibilidadeMes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
            decimal disponibilidadeAcum = acumuladoAtivo.ContainsKey("Ativo Financeiro") ? acumuladoAtivo["Ativo Financeiro"] : 0;

            decimal clientesMes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
            decimal clientesAcum = acumuladoAtivo.ContainsKey("Clientes") ? acumuladoAtivo["Clientes"] : 0;

            decimal estoqueMes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
            decimal estoqueAcum = acumuladoAtivo.ContainsKey("Estoques") ? acumuladoAtivo["Estoques"] : 0;

            decimal outrosAtivosOpMes = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;
            decimal outrosAtivosOpAcum = acumuladoPassivo.ContainsKey("Outros Ativos Operacionais Total") ? acumuladoPassivo["Outros Ativos Operacionais Total"] : 0;

            decimal fornecedoresMes = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
            decimal fornecedoresAcum = acumuladoPassivo.ContainsKey("Fornecedores") ? acumuladoPassivo["Fornecedores"] : 0;

            decimal outrosPassivosOpMes = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;
            decimal outrosPassivosOpAcum = acumuladoPassivo.ContainsKey("Outros Passivos Operacionais Total") ? acumuladoPassivo["Outros Passivos Operacionais Total"] : 0;

            decimal realizavelLongoPrazoMes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
            decimal realizavelLongoPrazoAcum = acumuladoAtivo.ContainsKey("Ativo Não Circulante") ? acumuladoAtivo["Ativo Não Circulante"] : 0;

            decimal exigivelLongoPrazoMes = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
            decimal exigivelLongoPrazoAcum = acumuladoPassivo.ContainsKey("Passivo Não Circulante Operacional") ? acumuladoPassivo["Passivo Não Circulante Operacional"] : 0;

            decimal ativosFixosMes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;
            decimal ativosFixosAcum = acumuladoAtivo.ContainsKey("Ativo Fixo") ? acumuladoAtivo["Ativo Fixo"] : 0;

            // === Indicadores ===
            decimal receitaLiquidaMes = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
            decimal receitaLiquidaAcum = acumuladoDRE.ContainsKey("(=) Receita Líquida de Vendas") ? acumuladoDRE["(=) Receita Líquida de Vendas"] : 0;

            decimal nOPATMes = monthDRE?.Totalizer.FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;
            decimal nOPATAcum = acumuladoDRE.ContainsKey("NOPAT") ? acumuladoDRE["NOPAT"] : 0;

            decimal roicMes = 0, roicAcum = 0;
            decimal capitalInvestidoMes = disponibilidadeMes + clientesMes + estoqueMes + outrosAtivosOpMes - (fornecedoresMes + outrosPassivosOpMes) + realizavelLongoPrazoMes + exigivelLongoPrazoMes + ativosFixosMes;
            decimal capitalInvestidoAcum = disponibilidadeAcum + clientesAcum + estoqueAcum + outrosAtivosOpAcum - (fornecedoresAcum + outrosPassivosOpAcum) + realizavelLongoPrazoAcum + exigivelLongoPrazoAcum + ativosFixosAcum;

            if (capitalInvestidoMes != 0) roicMes = (nOPATMes / capitalInvestidoMes) * 100;
            if (capitalInvestidoAcum != 0) roicAcum = (nOPATAcum / capitalInvestidoAcum) * 100;

            // === DTOs ===
            var economic = new EconomicViewDto
            {
                ReceitaLiquida = receitaLiquidaMes,
                ReceitaLiquidaAcumulado = receitaLiquidaAcum,
                CustoDespesaVariavel = custosMes,
                CustoDespesaVariavelAcumulado = custosAcum,
                DespesasOperacionais = despesasOpMes,
                DespesasOperacionaisAcumulado = despesasOpAcum,
                OutrosResultadosOperacionais = outrosResOpMes,
                OutrosResultadosOperacionaisAcumulado = outrosResOpAcum,
                NOPAT = nOPATMes,
                NOPATAcumulado = nOPATAcum,
                Impostos = impostosMes,
                ImpostosAcumulado = impostosAcum
            };

            var financial = new FinancialViewDto
            {
                Disponivel = disponibilidadeMes,
                DisponivelAcumulado = disponibilidadeAcum,
                Clientes = clientesMes,
                ClientesAcumulado = clientesAcum,
                Estoques = estoqueMes,
                EstoquesAcumulado = estoqueAcum,
                OutrosAtivosOperacionais = outrosAtivosOpMes,
                OutrosAtivosOperacionaisAcumulado = outrosAtivosOpAcum,
                Fornecedores = fornecedoresMes,
                FornecedoresAcumulado = fornecedoresAcum,
                OutrosPassivosOperacionais = outrosPassivosOpMes,
                OutrosPassivosOperacionaisAcumulado = outrosPassivosOpAcum,
                RealizavelLongoPrazo = realizavelLongoPrazoMes,
                RealizavelLongoPrazoAcumulado = realizavelLongoPrazoAcum,
                ExigivelLongoPrazo = exigivelLongoPrazoMes,
                ExigivelLongoPrazoAcumulado = exigivelLongoPrazoAcum,
                AtivosFixos = ativosFixosMes,
                AtivosFixosAcumulado = ativosFixosAcum,
                CapitalInvestido = capitalInvestidoMes,
                CapitalInvestidoAcumulado= capitalInvestidoAcum
            };

            var indicators = new ReturnIndicatorsDto
            {
                NOPAT = nOPATMes,
                NOPATAcumulado = nOPATAcum,
                CapitalInvestido = capitalInvestidoMes,
                CapitalInvestidoAcumulado = capitalInvestidoAcum,
                ROIC = roicMes,
                ROICAcumulado = roicAcum,
                WACC = wacc,
                SPREAD = roicMes - wacc,
                SPREADAcumulado = roicAcum - wacc
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
                       .FirstOrDefault(c => c.Name == "Patrimônio Liquido").TotalValue;

                var contasTransitorias = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Contas Transitórias").Value;


                var totalPassivoCirculante = totalizerResponses
                    .FirstOrDefault(c => c.Name == "Total Passivo Circulante").TotalValue;

                decimal total = totalPassivoCirculante + contasTransitorias + patrimonioLiquidos;

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

                    decimal totalAtivo = ativoFinanceiro + ativoOperacional + ativoFixo;

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
                 .Where(c => c.TypeOrder >= 18 && c.TypeOrder <= 33)
                 .Distinct()
                 .ToList();

            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);

            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var costCenters = model.Select(a => a.CostCenter).ToList();

            var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var balanceteDataClassifications = await _balanceteDataRepository.GetByAccountPlanClassificationId(accountPlanId);
            var painelBalancoContabilPassivo = await BuildPainelByTypePassivo(accountPlanId, year, 2);


            decimal acumuladoAnterior = 0;


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


                    // cálculos 


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

                    decimal passivoFinanceiro = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Financeiro")?.TotalValue ?? 0;
                    decimal passivoOperacional = totalizerResponses.FirstOrDefault(a => a.Name == "Passivo Operacional")?.TotalValue ?? 0;
                    var patrimonioLiquido = totalizerResponses.FirstOrDefault(a => a.Name == "Patrimônio Liquido");
                    decimal outrosPassivosOperacionaisTotal = totalizerResponses.FirstOrDefault(a => a.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;


                    decimal lucrosPrejuizos = totalizerResponses.FirstOrDefault(a => a.Name == "Lucros / Prejuízos Acumulados")?.TotalValue ?? 0;

                    patrimonioLiquido.TotalValue = patrimonioLiquido.TotalValue + lucrosPrejuizos;

                    decimal totalPassivo = passivoFinanceiro + passivoOperacional + patrimonioLiquido.TotalValue + (resultadoAcumulado.TotalValue * -1);

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
                    .FirstOrDefault(c => c.Name == "Despesas com Depreciação")?.Value ?? 0;

                var outrosResultadosOperacionais = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Outros Resultados Operacionais")?.Value ?? 0;

                if (despesasOperacionais != null)
                    despesasOperacionais.TotalValue = despesasOperacionais.TotalValue + despesasDepreciacao - outrosResultadosOperacionais;

                // cálculos 
                var receitaLiquidaValor = receitaOperacionalBruta + deducoes;
                if (receitaLiquida != null) receitaLiquida.TotalValue = receitaLiquidaValor;
                if (lucroBruto != null) lucroBruto.TotalValue = receitaLiquidaValor + custoMercadorias + custoDosServicosPrestados;
                if (margemContribuicao != null && lucroBruto != null)
                    margemContribuicao.TotalValue = lucroBruto.TotalValue + despesasV;

                if (lucroOperacional != null && lucroBruto != null && despesasOperacionais != null)
                    lucroOperacional.TotalValue = lucroBruto.TotalValue + despesasOperacionais.TotalValue + outrosResultadosOperacionais;

                if (lucroAntes != null && lucroOperacional != null)
                    lucroAntes.TotalValue = lucroOperacional.TotalValue + outrosReceitas + ganhosEPerdas;

                if (resultadoAntes != null && lucroAntes != null)
                    resultadoAntes.TotalValue = lucroAntes.TotalValue + receitasFinanceiras + despesasFinanceiras;

                if (lucroLiquido != null && resultadoAntes != null)
                    lucroLiquido.TotalValue = resultadoAntes.TotalValue + provisaoCSLL + provisaoIRPJ;

                if (ebitda != null && lucroAntes != null)
                    ebitda.TotalValue = lucroAntes.TotalValue + despesasDepreciacao;

                if (nopat != null && lucroAntes != null)
                    nopat.TotalValue = lucroAntes.TotalValue + provisaoCSLL + provisaoIRPJ;

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