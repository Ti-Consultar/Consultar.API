using _2___Application._2_Dto_s.CashFlow;
using _2___Application._2_Dto_s.Results.OperationalEfficiency;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application.Base;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.CashFlow
{
    public class CashFlowService : BaseService
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

        public CashFlowService(
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
        public async Task<PainelCashFlowResponseDto> GetCashFlowAntigoOficial(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var cashFlow = new List<CashFlowResponseDto>();

            CashFlowResponseDto previousMonth = null;

            var painelAtivoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);
            var painelPassivoAnterior = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year - 1, 2);

            var dezembroAtivo = painelAtivoAnterior?.Months?.FirstOrDefault(m => m.DateMonth == 12);
            var dezembroPassivo = painelPassivoAnterior?.Months?.FirstOrDefault(m => m.DateMonth == 12);

            decimal investimentoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Investimentos")?.TotalValue ?? 0;
            decimal clienteAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Clientes")?.TotalValue ?? 0;
            decimal estoqueAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Estoques")?.TotalValue ?? 0;
            decimal outrosAtivosAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;
            decimal depreciacaoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Depreciação / Amort. Acumulada")?.TotalValue ?? 0;
            decimal fornecedoresAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Fornecedores")?.TotalValue ?? 0;
            decimal obrigacoesTributariasETrabalhistasAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
            decimal outrosPassivosOperacionaisAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;
            decimal AtivoNaoCirculanteAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
            decimal exigivelLongoPrazoAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
            decimal patrimonioLiquidoAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Patrimônio Liquido")?.TotalValue ?? 0;
            decimal imobilizadoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Imobilizado")?.TotalValue ?? 0;
            decimal EmprestimoEFinanciamentoAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;
            decimal disponibilidadeDezembroAnterior =
                dezembroAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                decimal ativoFinanceiro = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal clientes = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosOperacionaisTotal = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;
                decimal fornecedores = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;
                decimal realizavelLongoPrazo = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLongoPrazo = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal imobilizado = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Imobilizado")?.TotalValue ?? 0;
                decimal emprestimoEFinanciamento = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;

                // 🔹 Lucro Líquido do período e Depreciação
                decimal lucroLiquidoDoPeriodo = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;
                decimal depreciacaoAmortizacao = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Depreciação / Amort. Acumulada")?.TotalValue ?? 0;

                // 🔹 Variações
                decimal variacaoClientes = (clientes - clienteAnterior) * -1;
                decimal variacaoEstoques = (estoque - estoqueAnterior) * -1;
                decimal variacaoOutrosAtivosOperacionais = (outrosAtivosOperacionaisTotal - outrosAtivosAnterior) * -1;
                decimal variacaoDepreciacaoAmortAcumulada = depreciacaoAmortizacao - depreciacaoAnterior;
                decimal variacaoFornecedores = (fornecedores - fornecedoresAnterior) * -1;
                decimal variacaoObrigacoes = (obrigacoesTributariasETrabalhistas - obrigacoesTributariasETrabalhistasAnterior) * -1;
                decimal variacaoOutrosPassivosOperacionais = (outrosPassivosOperacionaisTotal - outrosPassivosOperacionaisAnterior) * -1;
                decimal variacaoAtivoNaoCirculante = (realizavelLongoPrazo - AtivoNaoCirculanteAnterior) * -1;
                decimal variacaoInvestimento = ((monthAtivo.Totalizer.FirstOrDefault(c => c.Name == "Investimentos")?.TotalValue ?? 0) - investimentoAnterior) * -1;
                decimal variacaoPassivoNaoCirculante = exigivelLongoPrazo - exigivelLongoPrazoAnterior;
                decimal variacaoImobilizado = (imobilizado - imobilizadoAnterior) * -1;
                decimal variacaoEmprestimosFinanciamento = (emprestimoEFinanciamento - EmprestimoEFinanciamentoAnterior) * -1;







                decimal variacaoNCG = variacaoClientes + variacaoEstoques + variacaoOutrosAtivosOperacionais + variacaoFornecedores + variacaoObrigacoes + variacaoOutrosPassivosOperacionais;
                decimal fluxoCaixaOperacional = variacaoNCG + variacaoDepreciacaoAmortAcumulada + lucroLiquidoDoPeriodo;
                decimal fluxoCaixaLivre = fluxoCaixaOperacional + variacaoAtivoNaoCirculante + variacaoInvestimento + variacaoImobilizado;
                decimal fluxoDeCaixaEmpresa = fluxoCaixaLivre + variacaoEmprestimosFinanciamento + variacaoPassivoNaoCirculante;

                decimal patrimonio = monthPassivo.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;


                decimal PatrimonioL = patrimonio - lucroLiquidoDoPeriodo;
                decimal variacaoPatrimonioLiquido = patrimonioLiquidoAnterior - PatrimonioL;

                

                var dto = new CashFlowResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    LucroOperacionalLiquido = lucroLiquidoDoPeriodo,
                    DepreciacaoAmortizacao = variacaoDepreciacaoAmortAcumulada,
                    VariacaoNCG = variacaoNCG,
                    FluxoDeCaixaOperacional = fluxoCaixaOperacional,
                    FluxoDeCaixaLivre = fluxoCaixaLivre,
                    FluxoDeCaixaDaEmpresa = fluxoDeCaixaEmpresa,
                    Clientes = variacaoClientes,
                    Estoques = variacaoEstoques,
                    OutrosAtivosOperacionais = variacaoOutrosAtivosOperacionais,
                    Fornecedores = variacaoFornecedores,
                    ObrigacoesTributariasTrabalhistas = variacaoObrigacoes,
                    OutrosPassivosOperacionais = variacaoOutrosPassivosOperacionais,
                    AtivoNaoCirculante = variacaoAtivoNaoCirculante,
                    VariacaoInvestimento = variacaoInvestimento,
                    VariacaoImobilizado = variacaoImobilizado,
                    PassivoNaoCirculante = variacaoPassivoNaoCirculante,
                    CaptacoesAmortizacoesFinanceira = emprestimoEFinanciamento,
                    VariacaoPatrimonioLiquido = variacaoPatrimonioLiquido,
                    DisponibilidadeInicioDoPeriodo = monthAtivo.DateMonth == 1 ? disponibilidadeDezembroAnterior : previousMonth?.DisponibilidadeFinalDoPeriodo ?? 0,
                    DisponibilidadeFinalDoPeriodo = ativoFinanceiro
                };

                cashFlow.Add(dto);
                previousMonth = dto;

                // 🔹 Atualiza variáveis do mês anterior
                clienteAnterior = clientes;
                estoqueAnterior = estoque;
                outrosAtivosAnterior = outrosAtivosOperacionaisTotal;
                depreciacaoAnterior = depreciacaoAmortizacao;
                fornecedoresAnterior = fornecedores;
                obrigacoesTributariasETrabalhistasAnterior = obrigacoesTributariasETrabalhistas;
                outrosPassivosOperacionaisAnterior = outrosPassivosOperacionaisTotal;
                AtivoNaoCirculanteAnterior = realizavelLongoPrazo;
                investimentoAnterior = monthAtivo.Totalizer.FirstOrDefault(c => c.Name == "Investimentos")?.TotalValue ?? 0;
                exigivelLongoPrazoAnterior = exigivelLongoPrazo;
                imobilizadoAnterior = imobilizado;
                EmprestimoEFinanciamentoAnterior = emprestimoEFinanciamento;
                patrimonioLiquidoAnterior = patrimonio;
            }

            // 🔢 Totalizador geral (acumulado do ano)
            var totalGeral = new CashFlowResponseDto
            {
                Name = "ACUMULADO",
                DateMonth = 13,
                LucroOperacionalLiquido = cashFlow.Sum(x => x.LucroOperacionalLiquido),
                DepreciacaoAmortizacao = cashFlow.Sum(x => x.DepreciacaoAmortizacao),
                VariacaoNCG = cashFlow.Sum(x => x.VariacaoNCG),
                FluxoDeCaixaOperacional = cashFlow.Sum(x => x.FluxoDeCaixaOperacional),
                FluxoDeCaixaLivre = cashFlow.Sum(x => x.FluxoDeCaixaLivre),
                FluxoDeCaixaDaEmpresa = cashFlow.Sum(x => x.FluxoDeCaixaDaEmpresa),
                Clientes = cashFlow.Sum(x => x.Clientes),
                Estoques = cashFlow.Sum(x => x.Estoques),
                OutrosAtivosOperacionais = cashFlow.Sum(x => x.OutrosAtivosOperacionais),
                Fornecedores = cashFlow.Sum(x => x.Fornecedores),
                ObrigacoesTributariasTrabalhistas = cashFlow.Sum(x => x.ObrigacoesTributariasTrabalhistas),
                OutrosPassivosOperacionais = cashFlow.Sum(x => x.OutrosPassivosOperacionais),
                AtivoNaoCirculante = cashFlow.Sum(x => x.AtivoNaoCirculante),
                VariacaoInvestimento = cashFlow.Sum(x => x.VariacaoInvestimento),
                VariacaoImobilizado = cashFlow.Sum(x => x.VariacaoImobilizado),
                PassivoNaoCirculante = cashFlow.Sum(x => x.PassivoNaoCirculante),
                VariacaoPatrimonioLiquido = cashFlow.Sum(x => x.VariacaoPatrimonioLiquido),
                CaptacoesAmortizacoesFinanceira = cashFlow.Sum(x => x.CaptacoesAmortizacoesFinanceira),
                DisponibilidadeInicioDoPeriodo = cashFlow.FirstOrDefault()?.DisponibilidadeInicioDoPeriodo ?? 0,
                DisponibilidadeFinalDoPeriodo = cashFlow.LastOrDefault()?.DisponibilidadeFinalDoPeriodo ?? 0
            };

            cashFlow.Add(totalGeral);

            return new PainelCashFlowResponseDto
            {
                CashFlow = new CashFlowGroupedDto
                {
                    Months = cashFlow
                }
            };
        }

        public async Task<PainelCashFlowResponseDto> GetCashFlow(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var cashFlow = new List<CashFlowResponseDto>();

            CashFlowResponseDto previousMonth = null;

            // 🔥 inicializar com base em dezembro do ano anterior
            var painelAtivoAnterior = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year - 1, 1);
            var painelPassivoAnterior = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year - 1, 2);
            var painelDREAnterior = await BuildPainelByTypeDRE(accountPlanId, year - 1, 3);

            var dezembroAtivo = painelAtivoAnterior?.Months?.FirstOrDefault(m => m.DateMonth == 12);
            var dezembroPassivo = painelPassivoAnterior?.Months?.FirstOrDefault(m => m.DateMonth == 12);
            var dezembroDRE = painelDREAnterior?.Months?.FirstOrDefault(m => m.DateMonth == 12);

            decimal investimentoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Investimentos")?.TotalValue ?? 0;
            decimal clienteAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Clientes")?.TotalValue ?? 0;
            decimal estoqueAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Estoques")?.TotalValue ?? 0;
            decimal outrosAtivosAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;
            decimal depreciacaoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Depreciação / Amort. Acumulada")?.TotalValue ?? 0;
            decimal fornecedoresAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Fornecedores")?.TotalValue ?? 0;
            decimal obrigacoesTributariasETrabalhistasAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
            decimal outrosPassivosOperacionaisAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;
            decimal AtivoNaoCirculanteAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
            decimal exigivelLongoPrazoAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
            decimal patrimonioLiquidoAnterior = dezembroPassivo?.Totalizer.FirstOrDefault(c => c.Name == "Patrimônio Liquido")?.TotalValue ?? 0;
            decimal imobilizadoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Imobilizado")?.TotalValue ?? 0;
            decimal EmprestimoEFinanciamentoAnterior = dezembroAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;
            decimal disponibilidadeDezembroAnterior =
                dezembroAtivo?.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var receitaLiquida = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                var outrosResultadosOperacionais = monthDRE.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(t => t.Name == "Outros  Resultados Operacionais")?.Value ?? 0;
                var outrosResultadosNaoOperacionais = monthDRE.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(t => t.Name == "Outras Receitas não Operacionais")?.Value ?? 0;
                var outrosResultados = outrosResultadosOperacionais - outrosResultadosNaoOperacionais;

                var receitaFinanceira = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;
                var despesaFinanceira = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0;
                var resultadosFinanceiros = receitaFinanceira + despesaFinanceira;

                var custoMercadorias = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;
                var custoServicosPrestados = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;
                var despesasVariaveis = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Variáveis")?.Value ?? 0;
                var despesasOperacional = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;
                var custos = custoMercadorias + custoServicosPrestados;

                var provisaoCSLL = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;
                var provisaoIRPJ = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;
                var impostos = provisaoCSLL + provisaoIRPJ;

                var lucroAntes = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro");
                if (lucroAntes != null) lucroAntes.TotalValue = 0;

                var resultadoAntes = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "Resultado do Exercício Antes do Imposto");
                if (resultadoAntes != null && lucroAntes != null)
                    resultadoAntes.TotalValue = lucroAntes.TotalValue + receitaFinanceira + despesaFinanceira;

                decimal lucroLiquidoDoPeriodo = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo")?.TotalValue ?? 0;

                var lucroLiquido = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo");
                if (lucroLiquido != null && resultadoAntes != null)
                    lucroLiquido.TotalValue = resultadoAntes.TotalValue + provisaoCSLL + provisaoIRPJ;

                var patrimonioLiquido = monthPassivo.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;
                var emprestimoEFinanciamento = monthPassivo.Totalizer.FirstOrDefault(t => t.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;
                var imobilizado = (monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Imobilizado")?.TotalValue ?? 0); //* -1;
                var depreciacaoAmortAcumulada = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Depreciação / Amort. Acumulada")?.TotalValue ?? 0;

                decimal ativoFinanceiro = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal disponibilidade = ativoFinanceiro;

                decimal clientes = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosOperacionaisTotal = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedores = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal somaAtivos = disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal;
                decimal somaPassivo = fornecedores + obrigacoesTributariasETrabalhistas + outrosPassivosOperacionaisTotal;
                decimal necessidadeDeCapitalDeGiro = somaAtivos - somaPassivo;

                decimal realizavelLongoPrazo = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLongoPrazo = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixos = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoLiquido = necessidadeDeCapitalDeGiro + realizavelLongoPrazo + exigivelLongoPrazo + ativosFixos;
                decimal ncgTotal = necessidadeDeCapitalDeGiro + ativoFinanceiro;
                decimal investimentosAtivosFixos = capitalInvestidoLiquido - ncgTotal;

                var investimentos = monthAtivo?.Totalizer.FirstOrDefault(c => c.Name == "Investimentos")?.TotalValue ?? 0;


                // Variações
                decimal variacaoClientes = (clientes - clienteAnterior);
                decimal variacaoEstoques = (estoque - estoqueAnterior) ;
                decimal variacaoOutrosAtivosOperacionais = (outrosAtivosOperacionaisTotal - outrosAtivosAnterior);
                decimal variacaoDepreciacaoAmortAcumulada = (depreciacaoAmortAcumulada - depreciacaoAnterior);
                decimal variacaoFornecedores = (fornecedores - fornecedoresAnterior);
                decimal variacaoObrigacoes = (obrigacoesTributariasETrabalhistas - obrigacoesTributariasETrabalhistasAnterior);
                decimal variacaoOutrosPassivosOperacionais = (outrosPassivosOperacionaisTotal - outrosPassivosOperacionaisAnterior);
                decimal variacaoAtivoNaoCirculante = (realizavelLongoPrazo - AtivoNaoCirculanteAnterior);
                decimal variacaoInvestimento = (investimentos - investimentoAnterior);
                decimal variacaoPassivoNaoCirculante = exigivelLongoPrazo - exigivelLongoPrazoAnterior;
                decimal variacaoImobilizado = (imobilizado - imobilizadoAnterior);
                decimal variacaoEmprestimosFinanciamento = (emprestimoEFinanciamento - EmprestimoEFinanciamentoAnterior);
                decimal Patrimonio = patrimonioLiquido - lucroLiquido.TotalValue;







                decimal variacaoAnteriorPatrimonio = patrimonioLiquidoAnterior + lucroLiquido.TotalValue;

                decimal variacaoPatrimonioLiquido = patrimonioLiquidoAnterior - variacaoAnteriorPatrimonio;

                investimentoAnterior = investimentos;
                clienteAnterior = clientes;
                estoqueAnterior = estoque;
                outrosAtivosAnterior = outrosAtivosOperacionaisTotal;
                depreciacaoAnterior = depreciacaoAmortAcumulada;
                fornecedoresAnterior = fornecedores;
                obrigacoesTributariasETrabalhistasAnterior = obrigacoesTributariasETrabalhistas;
                outrosPassivosOperacionaisAnterior = outrosPassivosOperacionaisTotal;
                AtivoNaoCirculanteAnterior = realizavelLongoPrazo;
                exigivelLongoPrazoAnterior = exigivelLongoPrazo;
                patrimonioLiquidoAnterior = Patrimonio;
                imobilizadoAnterior = imobilizado;
                EmprestimoEFinanciamentoAnterior = emprestimoEFinanciamento;
                var variacaoNCG = variacaoClientes + variacaoEstoques + variacaoOutrosAtivosOperacionais - variacaoFornecedores - variacaoObrigacoes - variacaoOutrosPassivosOperacionais;


                decimal ngcNegativa = variacaoNCG * -1;
                decimal depreciacaoNegativa = variacaoDepreciacaoAmortAcumulada * -1;
                var fluxoCaixaOperacional = ngcNegativa + depreciacaoNegativa + lucroLiquidoDoPeriodo;



                decimal AtivoNaoCirculanteNegativo = variacaoAtivoNaoCirculante * -1;
                decimal investimentoNegativo = variacaoInvestimento * -1;
                decimal imobilizadoNegativo = variacaoImobilizado * -1;

                var fluxoCaixaLivre = fluxoCaixaOperacional + AtivoNaoCirculanteNegativo + investimentoNegativo + imobilizadoNegativo;
                var fluxoDeCaixaEmpresa = fluxoCaixaLivre + variacaoEmprestimosFinanciamento + variacaoPassivoNaoCirculante + variacaoPatrimonioLiquido;

                // fora do loop (onde você declara os “anteriores”)
                decimal xAnterior = 0m;

                // dentro do loop
                decimal disponibilidadeInicial = previousMonth?.DisponibilidadeFinalDoPeriodo ?? 0m;
                decimal disponibilidadeFinal = disponibilidade; // seu valor já calculado para o mês atual

                // Diferença desejada: Inicial - Final
                decimal diferenca = disponibilidadeInicial - disponibilidadeFinal;

                // Fluxo que você já calcula pelas variações
                decimal fluxo = fluxoCaixaLivre + variacaoEmprestimosFinanciamento + variacaoPassivoNaoCirculante;

                // Fechamento (x): diferença - fluxo
                decimal x = diferenca - fluxo;

                // Variação mês a mês do x (usando memória)
                decimal variacaoPatri = x - xAnterior;
                xAnterior = x;





                var dto = new CashFlowResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    //ReceitaLiquida = receitaLiquida,
                    //CustosOperacionais = custos,
                    //DespesasVariaveis = despesasVariaveis,
                    //DespesasOperacionais = despesasOperacional,
                    //OutrosResultados = outrosResultados,
                    //ResultadosFinanceiros = resultadosFinanceiros,
                    //Provisoes = impostos,
                    LucroOperacionalLiquido = lucroLiquidoDoPeriodo,
                    DepreciacaoAmortizacao = variacaoDepreciacaoAmortAcumulada * -1,
                    VariacaoNCG = variacaoNCG * -1,
                    Clientes = variacaoClientes * -1,
                    Estoques = variacaoEstoques * -1,
                    OutrosAtivosOperacionais = variacaoOutrosAtivosOperacionais * -1,
                    Fornecedores = variacaoFornecedores,
                    ObrigacoesTributariasTrabalhistas = variacaoObrigacoes,
                    OutrosPassivosOperacionais = variacaoOutrosPassivosOperacionais,
                    FluxoDeCaixaOperacional = fluxoCaixaOperacional,
                    AtivoNaoCirculante = variacaoAtivoNaoCirculante * -1,
                    VariacaoInvestimento = variacaoInvestimento * -1,
                    VariacaoImobilizado = variacaoImobilizado * -1,
                    FluxoDeCaixaLivre = fluxoCaixaLivre,
                    CaptacoesAmortizacoesFinanceira = variacaoEmprestimosFinanciamento,
                    PassivoNaoCirculante = variacaoPassivoNaoCirculante,
                   // VariacaoPatrimonioLiquido = variacaoPatrimonioLiquido * -1,
                    VariacaoPatrimonioLiquido = variacaoPatri * -1,
                    FluxoDeCaixaDaEmpresa = diferenca,
                   // FluxoDeCaixaDaEmpresa = diferenca,

                    DisponibilidadeInicioDoPeriodo = monthAtivo.DateMonth == 1
                                                                    ? disponibilidadeDezembroAnterior // se janeiro, pega dezembro anterior
        :                                                           (previousMonth?.DisponibilidadeFinalDoPeriodo ?? 0),

                    DisponibilidadeFinalDoPeriodo = disponibilidade,
                };

                cashFlow.Add(dto);
                previousMonth = dto;
            }

            return new PainelCashFlowResponseDto
            {
                CashFlow = new CashFlowGroupedDto
                {
                    Months = cashFlow
                }
            };
        }
        public async Task<PainelCashFlowResponseDto> GetCashFlowAntigo(int accountPlanId, int year)
        {
            var painelAtivo = await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
            var painelPassivo = await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
            var painelDRE = await BuildPainelByTypeDRE(accountPlanId, year, 3);
            var cashFlow = new List<CashFlowResponseDto>();

            CashFlowResponseDto previousMonth = null;

            decimal investimentoAnterior = 0;
            decimal clienteAnterior = 0;
            decimal estoqueAnterior = 0;
            decimal outrosAtivosAnterior = 0;
            decimal depreciacaoAnterior = 0;
            decimal fornecedoresAnterior = 0;
            decimal obrigacoesTributariasETrabalhistasAnterior = 0;
            decimal outrosPassivosOperacionaisAnterior = 0;
            decimal AtivoNaoCirculanteAnterior = 0;
            decimal realizavelLongoPrazoAnterior = 0;
            decimal exigivelLongoPrazoAnterior = 0;
            decimal patrimonioLiquidoAnterior = 0;
            decimal imobilizadoAnterior = 0;

            foreach (var monthAtivo in painelAtivo.Months.OrderBy(m => m.DateMonth))
            {
                var monthPassivo = painelPassivo.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);
                var monthDRE = painelDRE.Months.FirstOrDefault(m => m.DateMonth == monthAtivo.DateMonth);

                var receitaLiquida = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "(=) Receita Líquida de Vendas")?.TotalValue ?? 0;
                var outrosResultadosOperacionais = monthDRE.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(t => t.Name == "Outros  Resultados Operacionais")?.Value ?? 0;
                var outrosResultadosNaoOperacionais = monthDRE.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(t => t.Name == "Outras Receitas não Operacionais")?.Value ?? 0;
                var outrosResultados = outrosResultadosOperacionais - outrosResultadosNaoOperacionais;

                var receitaFinanceira = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Receitas Financeiras")?.Value ?? 0;
                var despesaFinanceira = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Financeiras")?.Value ?? 0;
                var resultadosFinanceiros = receitaFinanceira + despesaFinanceira;

                var custoMercadorias = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos das Mercadorias")?.Value ?? 0;
                var custoServicosPrestados = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "(-) Custos dos Serviços Prestados")?.Value ?? 0;
                var despesasVariaveis = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Despesas Variáveis")?.Value ?? 0;
                var despesasOperacional = monthDRE?.Totalizer
                    .FirstOrDefault(c => c.Name == "(-) Despesas Operacionais")?.TotalValue ?? 0;
                var custosEDespesasOperacionais = custoMercadorias + custoServicosPrestados + despesasOperacional;
                var custos = custoMercadorias + custoServicosPrestados;

                var provisaoCSLL = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para CSLL")?.Value ?? 0;
                var provisaoIRPJ = monthDRE?.Totalizer.SelectMany(t => t.Classifications)
                    .FirstOrDefault(c => c.Name == "Provisão para IRPJ")?.Value ?? 0;
                var impostos = provisaoCSLL + provisaoIRPJ;

                var lucroAntes = monthDRE.Totalizer
                   .FirstOrDefault(t => t.Name == "Lucro Antes do Resultado Financeiro");
                if (lucroAntes != null) lucroAntes.TotalValue = 0;

                var resultadoAntes = monthDRE.Totalizer
                    .FirstOrDefault(t => t.Name == "Resultado do Exercício Antes do Imposto");

                if (resultadoAntes != null && lucroAntes != null)
                    resultadoAntes.TotalValue = lucroAntes.TotalValue + receitaFinanceira + despesaFinanceira;

                var lucroLiquido = monthDRE.Totalizer.FirstOrDefault(t => t.Name == "Lucro Líquido do Periodo");

                var teste = lucroLiquido.TotalValue;

                if (lucroLiquido != null && resultadoAntes != null)
                    lucroLiquido.TotalValue = resultadoAntes.TotalValue + provisaoCSLL + provisaoIRPJ;


                var patrimonioLiquido = monthPassivo.Totalizer.FirstOrDefault(t => t.Name == "Patrimônio Liquido")?.TotalValue ?? 0;

                var emprestimoEFinanciamento = monthPassivo.Totalizer.FirstOrDefault(t => t.Name == "Empréstimos e Financiamentos")?.TotalValue ?? 0;
                var imobilizado = (monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Imobilizado")?.TotalValue ?? 0) * -1;




                var depreciacaoAmortAcumulada = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Depreciação / Amort. Acumulada")?.TotalValue ?? 0;

                decimal ativoFinanceiro = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Financeiro")?.TotalValue ?? 0;
                decimal disponibilidade = ativoFinanceiro;

                decimal clientes = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Clientes")?.TotalValue ?? 0;
                decimal estoque = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Estoques")?.TotalValue ?? 0;
                decimal outrosAtivosOperacionaisTotal = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Outros Ativos Operacionais Total")?.TotalValue ?? 0;

                decimal fornecedores = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Fornecedores")?.TotalValue ?? 0;
                decimal obrigacoesTributariasETrabalhistas = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Obrigações Tributárias e Trabalhistas")?.TotalValue ?? 0;
                decimal outrosPassivosOperacionaisTotal = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Outros Passivos Operacionais Total")?.TotalValue ?? 0;

                decimal somaAtivos = disponibilidade + clientes + estoque + outrosAtivosOperacionaisTotal;
                decimal somaPassivo = fornecedores + obrigacoesTributariasETrabalhistas + outrosPassivosOperacionaisTotal;
                decimal necessidadeDeCapitalDeGiro = somaAtivos + somaPassivo;

                decimal realizavelLongoPrazo = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Não Circulante")?.TotalValue ?? 0;
                decimal exigivelLongoPrazo = monthPassivo?.Totalizer.FirstOrDefault(t => t.Name == "Passivo Não Circulante Operacional")?.TotalValue ?? 0;
                decimal ativosFixos = monthAtivo.Totalizer.FirstOrDefault(t => t.Name == "Ativo Fixo")?.TotalValue ?? 0;

                decimal capitalInvestidoLiquido = necessidadeDeCapitalDeGiro + realizavelLongoPrazo + exigivelLongoPrazo + ativosFixos;
                decimal ncgTotal = necessidadeDeCapitalDeGiro + ativoFinanceiro;
                decimal investimentosAtivosFixos = capitalInvestidoLiquido - ncgTotal;

                var investimentos = monthAtivo?.Totalizer
                  .FirstOrDefault(c => c.Name == "Investimentos")?.TotalValue ?? 0;

              

                // Variações
                decimal variacaoClientes = clientes + clienteAnterior * -1;
                decimal variacaoEstoques = estoque  + estoqueAnterior * -1;
                decimal variacaoOutrosAtivosOperacionais = outrosAtivosOperacionaisTotal + outrosAtivosAnterior * -1;
                decimal variacaoDepreciacaoAmortAcumulada = depreciacaoAmortAcumulada - depreciacaoAnterior;
                decimal variacaoFornecedores = fornecedores - fornecedoresAnterior;
                decimal variacaoObrigacoes = obrigacoesTributariasETrabalhistas - obrigacoesTributariasETrabalhistasAnterior ;
                decimal variacaoOutrosPassivosOperacionais = outrosPassivosOperacionaisTotal - outrosPassivosOperacionaisAnterior;
                decimal variacaoAtivoNaoCirculante = realizavelLongoPrazo - AtivoNaoCirculanteAnterior;
                decimal variacaoInvestimento = investimentos - investimentoAnterior;
                decimal variacaoPassivoNaoCirculante = exigivelLongoPrazo - exigivelLongoPrazoAnterior;

              
                decimal variacaoImobilizado = imobilizado - imobilizadoAnterior;

                decimal Patrimonio =  patrimonioLiquido - lucroLiquido.TotalValue;


                decimal variacaoPatrimonioLiquido = patrimonioLiquidoAnterior  - Patrimonio ;

                investimentoAnterior = investimentos;
                clienteAnterior = clientes;
                estoqueAnterior = estoque;
                outrosAtivosAnterior = outrosAtivosOperacionaisTotal;
                depreciacaoAnterior = depreciacaoAmortAcumulada;
                fornecedoresAnterior = fornecedores;
                obrigacoesTributariasETrabalhistasAnterior = obrigacoesTributariasETrabalhistas;
                outrosPassivosOperacionaisAnterior = outrosPassivosOperacionaisTotal;
                AtivoNaoCirculanteAnterior = realizavelLongoPrazo;
                exigivelLongoPrazoAnterior = exigivelLongoPrazo;
                patrimonioLiquidoAnterior = Patrimonio;
                imobilizadoAnterior = imobilizado;



                var variacaoNCG = variacaoClientes + variacaoEstoques + variacaoOutrosAtivosOperacionais + variacaoFornecedores + variacaoObrigacoes + variacaoOutrosPassivosOperacionais;



                var fluxoCaixaOperacional = (variacaoNCG + variacaoDepreciacaoAmortAcumulada) - teste ;
                var fluxoCaixaLivre = fluxoCaixaOperacional + variacaoAtivoNaoCirculante + variacaoInvestimento - variacaoImobilizado;


                var fluxoDeCaixaEmpresa = fluxoCaixaLivre + variacaoDepreciacaoAmortAcumulada + variacaoPassivoNaoCirculante + variacaoPatrimonioLiquido;

                var dto = new CashFlowResponseDto
                {
                    Name = monthAtivo.Name,
                    DateMonth = monthAtivo.DateMonth,
                    //ReceitaLiquida = receitaLiquida,
                    //CustosOperacionais = custos,
                    //DespesasVariaveis = despesasVariaveis,
                    //DespesasOperacionais = despesasOperacional,
                    //OutrosResultados = outrosResultados,
                    //ResultadosFinanceiros = resultadosFinanceiros,
                    //Provisoes = impostos,
                    LucroOperacionalLiquido = teste,
                    DepreciacaoAmortizacao = variacaoDepreciacaoAmortAcumulada,
                    VariacaoNCG = variacaoNCG,
                    Clientes = variacaoClientes,
                    Estoques = variacaoEstoques,
                    OutrosAtivosOperacionais = variacaoOutrosAtivosOperacionais,
                    Fornecedores = variacaoFornecedores,
                    ObrigacoesTributariasTrabalhistas = variacaoObrigacoes,
                    OutrosPassivosOperacionais = variacaoOutrosPassivosOperacionais,
                    FluxoDeCaixaOperacional = fluxoCaixaOperacional,
                    AtivoNaoCirculante = variacaoAtivoNaoCirculante,
                    VariacaoInvestimento = variacaoInvestimento,
                    VariacaoImobilizado = variacaoImobilizado,
                    FluxoDeCaixaLivre = fluxoCaixaLivre,
                    CaptacoesAmortizacoesFinanceira = emprestimoEFinanciamento,
                    PassivoNaoCirculante = variacaoPassivoNaoCirculante,
                    VariacaoPatrimonioLiquido = variacaoPatrimonioLiquido,
                    FluxoDeCaixaDaEmpresa = fluxoDeCaixaEmpresa,
                    DisponibilidadeInicioDoPeriodo = previousMonth?.DisponibilidadeFinalDoPeriodo ?? 0,
                    DisponibilidadeFinalDoPeriodo = disponibilidade,
                };

                cashFlow.Add(dto);
                previousMonth = dto;
            }


            return new PainelCashFlowResponseDto
            {
                CashFlow = new CashFlowGroupedDto
                {
                    Months = cashFlow
                }
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