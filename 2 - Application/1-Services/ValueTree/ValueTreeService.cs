using _2___Application._2_Dto_s.CashFlow;
using _2___Application._2_Dto_s.Results.OperationalEfficiency;
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
                Clientes = clientes,
                Estoques = estoque,
                OutrosAtivosOperacionais = outrosAtivosOperacionaisTotal,
                Fornecedores = fornecedores,
                OutrosPassivosOperacionais = outrosPassivosOperacionaisTotal,
                RealizavelLongoPrazo = realizavelLongoPrazo,
                ExigivelLongoPrazo = exigivelLongoPrazo,
                AtivosFixos = ativosFixos,
                CapitalDeGiro = cdg,
                CapitalInvestido = capitalInvestidoLiquido
            };

            var indicators = new ReturnIndicatorsDto
            {
                NOPAT = nOPAT,
                CapitalInvestido = capitalInvestidoLiquido,
                ROIC = roic,
                WACC = wacc,
                SPREAD = evaSPREAD,
                EVA = eva


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
            var painelOperationalEfficiency = await GetOperationalEfficiency(accountPlanId, year);

            // === Valores do mês selecionado ===
            var monthAtivo = painelAtivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == month);
            var monthOperationalEfficiency = painelOperationalEfficiency
                .OperationalEfficiency
                        .Months
                    .FirstOrDefault(m => m.DateMonth == month);


            var meses = painelOperationalEfficiency.OperationalEfficiency.Months
     .Where(m => m.DateMonth <= month); // << filtro aqui

            var monthOperationalEfficiencyAcum = new
            {
                ReceitasLiquidas = meses.Sum(m => m.ReceitasLiquidas),
                CustosDespesas = meses.Sum(m => m.CustosDespesas),
                EBITDA = meses.Sum(m => m.EBITDA),
                MargemEBITDA = meses.Sum(m => m.MargemEBITDA),
                LucroOperacionalAntesJurosImpostos = meses.Sum(m => m.LucroOperacionalAntesJurosImpostos),
                ResultadoFinanceiro = meses.Sum(m => m.ResultadoFinanceiro),
                Impostos = meses.Sum(m => m.Impostos),
                LucroLiquido = meses.Sum(m => m.LucroLiquido),
                NOPAT = meses.Sum(m => m.NOPAT),
                MargemNOPAT = meses.Sum(m => m.MargemNOPAT),
                Disponivel = meses.Sum(m => m.Disponivel),
                Clientes = meses.Sum(m => m.Clientes),
                Estoques = meses.Sum(m => m.Estoques),
                Fornecedores = meses.Sum(m => m.Fornecedores),
                NCGCEF = meses.Sum(m => m.NCGCEF),
                NCGTotal = meses.Sum(m => m.NCGTotal),
                InvestimentosAtivosFixos = meses.Sum(m => m.InvestimentosAtivosFixos),
                CapitalInvestidoLiquido = meses.Sum(m => m.CapitalInvestidoLiquido),
                CapitalTurnover = meses.Sum(m => m.CapitalTurnover),
                ROIC = meses.Sum(m => m.ROIC),
                WACC = meses.Sum(m => m.WACC),
                EVASPREAD = meses.Sum(m => m.EVASPREAD),
                EVA = meses.Sum(m => m.EVA)
            };


            
            ///decimal roic = capitalInvestidoLiquido != 0 ? (nOPAT / capitalInvestidoLiquido) * 100 : 0;
            ///decimal evaSPREAD = roic - wacc;

            var roicAcumuladoFinal =  monthOperationalEfficiencyAcum.CapitalInvestidoLiquido != 0 ? (monthOperationalEfficiencyAcum.NOPAT / monthOperationalEfficiencyAcum.CapitalInvestidoLiquido) * 100 : 0;




            // === Acumulados do ano ===
            var acumuladoAtivo = painelAtivo.Months
             .Where(m => m.DateMonth <= month) // << FILTRO
             .SelectMany(m => m.Totalizer)
             .GroupBy(t => t.Name)
             .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalValue));

             var acumuladoPassivo = painelPassivo.Months
             .Where(m => m.DateMonth <= month)
             .SelectMany(m => m.Totalizer)
             .GroupBy(t => t.Name)
             .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalValue));

            var acumuladoDRE = painelDRE.Months
                .Where(m => m.DateMonth <= month)
                .SelectMany(m => m.Totalizer)
                .GroupBy(t => t.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalValue));

            var acumuladoClassDRE = painelDRE.Months
                .Where(m => m.DateMonth <= month)
                .SelectMany(m => m.Totalizer.SelectMany(t => t.Classifications))
                .GroupBy(c => c.Name)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Value));
            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);

            decimal waccTotalAno = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;

            // valor mensal
            decimal wacc = waccTotalAno / 12;

            decimal waccAcumulado = wacc * month;
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

            decimal outrosAtivosOpMes = monthAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;
            decimal outrosAtivosOpAcum = acumuladoAtivo.ContainsKey("Outros Ativos Operacionais Total") ? acumuladoAtivo["Outros Ativos Operacionais Total"] : 0;


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

            decimal margemLajir = monthDRE?.Totalizer
               .FirstOrDefault(c => c.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;
            decimal margemLajirAcumulada = acumuladoDRE.ContainsKey("Lucro Antes do Resultado Financeiro") ? acumuladoDRE["Lucro Antes do Resultado Financeiro"] : 0;


            decimal margemContribuicao = monthDRE?.Totalizer
               .FirstOrDefault(c => c.Name == "Margem Contribuição")?.TotalValue ?? 0;
            decimal margemContribuicaoAcumulada = acumuladoDRE.ContainsKey("Margem Contribuição") ? acumuladoDRE["Margem Contribuição"] : 0;

            decimal patrimonioLiquido = monthPassivo?.Totalizer
         .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            decimal obrigacoesTributariasETrabalhistasMes = monthPassivo?.Totalizer
                  .FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
            decimal obrigacoesTributariasETrabalhistasAcum = acumuladoDRE.ContainsKey("Obrigações Tributárias e Trabalhistas") ? acumuladoDRE["Obrigações Tributárias e Trabalhistas"] : 0;

            decimal somaAtivos = disponibilidadeMes + clientesMes + estoqueMes + outrosAtivosOpMes;
            decimal somaPassivo = fornecedoresMes + obrigacoesTributariasETrabalhistasMes + outrosPassivosOpMes;
            decimal necessidadeDeCapitalDeGiro = somaAtivos + somaPassivo;

            decimal somaAtivosAcum = disponibilidadeAcum + clientesAcum + estoqueAcum + outrosAtivosOpAcum;
            decimal somaPassivoAcum = fornecedoresAcum + obrigacoesTributariasETrabalhistasAcum + outrosAtivosOpAcum;
            decimal necessidadeDeCapitalDeGiroAcum = somaAtivosAcum + somaPassivoAcum;

            decimal roicMes = 0, roicAcum = 0;


            decimal capitalInvestidoMes = monthOperationalEfficiency.CapitalInvestidoLiquido;
            roicMes = monthOperationalEfficiency.ROIC;
            // fazer para todos os que dependem de eficiencia operacional obtendo ele;
            // fazer para o acumulado esse operational Eficiency

            var passivoNaoCirculante = monthPassivo?.Totalizer
            .FirstOrDefault(t => t.Name == "Passivo Não Circulante")?.TotalValue ?? 0;

            var patrimonioLiquidoGestaoLiquidez = monthPassivo?.Totalizer
                .FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

            var ativoNaoCirculante = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

            var ativoFixo = monthAtivo.Totalizer
                .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;
            var cdg = ((passivoNaoCirculante + patrimonioLiquidoGestaoLiquidez) - (ativoNaoCirculante + ativoFixo));


            var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
            var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
            
            var ncg = valorAtivoOperacional - valorPassivoOperacional;

            decimal necessidadeDeCapitalDeGiros = ncg;
            decimal capitalInvestidoAcum = monthOperationalEfficiencyAcum.CapitalInvestidoLiquido;





            roicAcum = monthOperationalEfficiencyAcum.ROIC;

            decimal spread = Math.Round(monthOperationalEfficiency.EVASPREAD, 2);
            decimal spreadAcumulado = Math.Round(monthOperationalEfficiencyAcum.EVASPREAD, 2);

            decimal eva = Math.Round(monthOperationalEfficiency.EVA, 2);
            decimal evaAcmulado = monthOperationalEfficiencyAcum.EVA;


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
                ImpostosAcumulado = impostosAcum,
                LAJIR = margemLajir,
                LAJIRAcumulado = margemLajirAcumulada,
                MargemContribuicao = margemContribuicao,
                MargemContribuicaoAcumulado = margemContribuicaoAcumulada
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
                CapitalInvestidoAcumulado = capitalInvestidoAcum,
                CapitalDeGiro = necessidadeDeCapitalDeGiros,
            };

            var indicators = new ReturnIndicatorsDto
            {
                NOPAT = nOPATMes,
                NOPATAcumulado = nOPATAcum,
                CapitalInvestido = capitalInvestidoMes,
                CapitalInvestidoAcumulado = capitalInvestidoAcum,
                ROIC = Math.Round(roicMes, 2),
                ROICAcumulado = Math.Round(roicAcumuladoFinal, 2),
                WACC = Math.Round(wacc, 2),
                WACCAcumulado = Math.Round(waccAcumulado, 2),
                SPREAD = spread,
                SPREADAcumulado = spreadAcumulado,
                EVA = eva,
                EVA_Acumulado = evaAcmulado

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

        public async Task<PainelOperationalEfficiencyResponseDto> GetOperationalEfficiency(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var operationalEfficiency = new List<OperationalEfficiencyResponseDto>();

            var parameter = await _parameterRepository.GetByAccountPlanIdYear(accountPlanId, year);
            decimal wacc = parameter.FirstOrDefault(a => a.Name == "WACC")?.ParameterValue ?? 0;
            wacc = wacc / 12;

            foreach (var monthAtivo in painelAtivo.Months)
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                // Receitas
                var receitaLiquida = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;

                var receitaFinanceira = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;

                // Custos e Despesas
                var custoMercadorias = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;

                var custoServicosPrestados = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;

                var despesasOperacional = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;

                var custosEDespesasOperacionais = custoMercadorias + custoServicosPrestados + despesasOperacional;

                // Resultado e Lucros
                var ebitda = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "EBITDA")?.TotalValue ?? 0;

                var margemEbitda = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "Margem EBITDA %")?.TotalValue ?? 0;

                var lucroOperacionalAntesJurosImpostos = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "Lucro Antes do Resultado Financeiro")?.TotalValue ?? 0;

                var provisaoCSLL = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;

                var provisaoIRPJ = monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;

                var resultadoFinanceiro = receitaFinanceira + (monthDRE?.Totalizer
                    .SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0);

                var impostos = provisaoCSLL + provisaoIRPJ;

                var lucroLiquido = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                // Ativos Circulantes
                decimal ativoFinanceiro = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

                decimal disponibilidade = ativoFinanceiro; // mesmo valor

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

                // Necessidade de Capital de Giro

                decimal somaAtivos = disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal;

                decimal somaPassivo = fornecedores - obrigacoesTributariasETrabalhistas - outrosPassivosOperacionaisTotal;

                decimal necessidadeDeCapitalDeGiro = somaAtivos - somaPassivo;

                var valorAtivoOperacional = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Operacional")?.TotalValue ?? 0;
                var valorPassivoOperacional = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Operacional")?.TotalValue ?? 0;
                var ncg = valorAtivoOperacional - valorPassivoOperacional;

                // Ativo e Passivo não circulantes + Ativos Fixos
                decimal realizavelLongoPrazo = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;

                decimal exigivelLongoPrazo = monthPassivo?.Totalizer
                    .FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;

                decimal ativosFixos = monthAtivo.Totalizer
                    .FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoLiquido = disponibilidade + ncg + realizavelLongoPrazo - exigivelLongoPrazo + ativosFixos;


                // Indicadores financeiros
                decimal nOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "NOPAT")?.TotalValue ?? 0;

                decimal margemNOPAT = monthDRE?.Totalizer
                    .FirstOrDefault(t => t.Name == "Margem NOPAT %")?.TotalValue ?? 0;


                var ncgTotal = ncg + disponibilidade;


                decimal roic = capitalInvestidoLiquido != 0 ? (nOPAT / capitalInvestidoLiquido) * 100 : 0;
                decimal evaSPREAD = roic - wacc;
                decimal turnover = receitaLiquida != 0 ? capitalInvestidoLiquido / receitaLiquida : 0;
                //  decimal ncgTotal = necessidadeDeCapitalDeGiro - ativoFinanceiro;
                decimal realNCG = clientes + estoque - fornecedores;
                decimal investimentosAtivosFixos = capitalInvestidoLiquido - ncgTotal;

                decimal evaSpreadPorcentagem = Math.Round(roic - wacc, 2);

                decimal eva = capitalInvestidoLiquido != 0 ? (evaSpreadPorcentagem / 100) * capitalInvestidoLiquido : 0;

                // Adiciona ao resultado
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
                    NCGCEF = realNCG,
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

            return new PainelOperationalEfficiencyResponseDto
            {
                OperationalEfficiency = new OperationalEfficiencyGroupedDto
                {
                    Months = operationalEfficiency
                }
            };
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