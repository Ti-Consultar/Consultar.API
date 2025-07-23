using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.Classification.AccountPlanClassification;
using _2___Application._2_Dto_s.Permissions;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services
{
    public class ClassificationService : BaseService
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

        public ClassificationService(
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

        #region Métodos
        #region Template
        public async Task<ResultValue> GetAll()
        {
            try
            {
                var model = await _repository.GetAll();
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .GroupBy(a => new
                    {
                        a.TotalizerClassificationTemplate.Id,
                        a.TotalizerClassificationTemplate.Name,
                        a.TotalizerClassificationTemplate.TypeOrder
                    })
                    .Select(g => new ClassificationtesteResponse
                    {
                        TotalizerClassification = new TotalizerClassificationTemplateResponse
                        {
                            Id = g.Key.Id,
                            Name = g.Key.Name,
                            TypeOrder = g.Key.TypeOrder
                        },
                        Classifications = g.Select(x => new TotalClassificationtesteResponse
                        {
                            Classifications = new ClassificationResponse
                            {
                                Id = x.Id,
                                Name = x.Name,
                                TypeOrder = x.TypeOrder,
                                TypeClassification = x.TypeClassification.GetDescription()

                            }
                        }).ToList()
                    }).ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        private List<ClassificationtesteResponse> MapToClassificationGroup(List<ClassificationModel> model)
        {
            return model
                .GroupBy(a => new
                {
                    a.TotalizerClassificationTemplate.Id,
                    a.TotalizerClassificationTemplate.Name,
                    a.TotalizerClassificationTemplate.TypeOrder
                })
                .Select(g => new ClassificationtesteResponse
                {
                    TotalizerClassification = new TotalizerClassificationTemplateResponse
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        TypeOrder = g.Key.TypeOrder
                    },
                    Classifications = g.Select(x => new TotalClassificationtesteResponse
                    {
                        Classifications = new ClassificationResponse
                        {
                            Id = x.Id,
                            Name = x.Name,
                            TypeOrder = x.TypeOrder,
                            TypeClassification = x.TypeClassification.GetDescription(),

                        }
                    }).ToList()
                }).ToList();
        }

        public async Task<ResultValue> GetByTypeClassificationTemplate(ETypeClassification typeClassification)
        {
            try
            {
                var model = await _repository.GetByTypeClassification(typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = MapToClassificationGroup(model);

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetById(int id)
        {
            try
            {
                var model = await _repository.GetById(id);
                if (model == null)
                    return ErrorResponse(Message.NotFound);

                // Mesmo com um item, o método de agrupamento vai transformar em lista agrupada
                var response = MapToClassificationGroup(new List<ClassificationModel> { model });

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        #region Private


        #endregion
        #endregion

        #region AccountPlan Classification

        public async Task<ResultValue> GetByTypeClassificationReal(int accountPlanId, ETypeClassification typeClassification)
        {
            try
            {
                var model = await _accountClassificationRepository.GetByTypeClassification(accountPlanId, typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .OrderBy(x => x.TypeOrder) // Ordenação crescente por Type
                    .Select(MapToClassificationRealResponse)
                    .ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> Create(CreateAccountPlanClassification dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // Verifica se já existem classificações cadastradas para evitar duplicação
                var existingClassifications = await _accountClassificationRepository.GetAllAsync(dto.AccountPlanId);
                if (existingClassifications != null && existingClassifications.Any())
                    return ErrorResponse("Já existem classificações cadastradas para este plano de contas.");

                await CreateBalancosReclassificadosAsync(dto.AccountPlanId);
                await CreateTotalizersAsync(dto.AccountPlanId);

                var classificationsTemplate = await _repository.GetAllAsNoTracking();

                var models = classificationsTemplate.Select(i => new AccountPlanClassification
                {
                    Name = i.Name,
                    TypeOrder = i.TypeOrder,
                    TypeClassification = i.TypeClassification,
                    AccountPlanId = dto.AccountPlanId,
                }).ToList();

                var reclassifications = await _balancoReclassificadoRepository.GetByAccountPlanId(dto.AccountPlanId);
                var totalizerClassifications = await _totalizerClassificationRepository.GetByAccountPlanId(dto.AccountPlanId);

                // Organização modular
                MapAtivos(models, reclassifications);
                MapAtivosTotalizer(models, totalizerClassifications);
                MapPassivos(models, reclassifications);
                MapPassivosTotalizer(models, totalizerClassifications);
                MapDRE(models, reclassifications);
                MapDRETotalizer(models, totalizerClassifications);

                await _accountClassificationRepository.AddRangeAsync(models);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        private void MapAtivos(List<AccountPlanClassification> models, List<BalancoReclassificadoModel> reclassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.Ativo)) // exemplo usando TypeClassification pra filtrar Ativo
            {
                switch (classification.Name)
                {
                    case "Caixas":
                    case "Bancos":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Caixa e Equivalente de Caixa")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Aplicações Financeiras":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Aplicação Financeira")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Clientes":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Clientes")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Outros Créditos":
                    case "Adiantamentos":
                    case "Impostos a Recuperar / Antecipações":
                    case "Empréstimos":
                    case "Despesas a Apropriar":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Outros Ativos Operacionais")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Estoques":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Estoques")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Consórcio":
                    case "Empréstimos a Coligadas e Controlada":
                    case "Depósitos Judiciais":
                    case "Outros Créditos LP":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Ativo Não Circulante Operacional")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Contas Transitórias":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Contas Transitórias")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Investimentos":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Investimentos")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Imobilizado":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Imobilizado")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Intangível":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Intangível")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Depreciação / Amortização Acumuladas":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Depreciação / Amort. Acumulada")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;
                }
            }
        }

        private void MapAtivosTotalizer(List<AccountPlanClassification> models, List<TotalizerClassificationModel> totalizerClassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.Ativo))
            {
                switch (classification.Name)
                {
                    case "Caixas":
                    case "Bancos":
                    case "Aplicações Financeiras":
                    case "Clientes":
                    case "Outros Créditos":
                    case "Adiantamentos":
                    case "Impostos a Recuperar / Antecipações":
                    case "Empréstimos":
                    case "Estoques":
                    case "Despesas a Apropriar":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Total Ativo Circulante")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Consórcio":
                    case "Empréstimos a Coligadas e Controlada":
                    case "Depósitos Judiciais":
                    case "Outros Créditos LP":
                    case "Contas Transitórias":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Realizavel Longo Prazo")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Investimentos":
                    case "Imobilizado":
                    case "Depreciação / Amortização Acumuladas":
                    case "Intangível":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "total Ativo Não Circulante")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;
                }
            }
        }
        private void MapPassivosTotalizer(List<AccountPlanClassification> models, List<TotalizerClassificationModel> totalizerClassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.Passivo))
            {
                switch (classification.Name)
                {
                    case "Fornecedores Fábrica":
                    case "Fornecedores Diversos":
                    case "Seguros a Pagar":
                    case "Outras Contas a Pagar":
                    case "Creditos de Clientes":
                    case "Empréstimos e Financiamentos":
                    case "(-) Devolução de Compras":
                    case "Obrigações Sociais a Pagar":
                    case "Obrigações Fiscais a Pagar":
                    case "Outras Exigibilidades":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Total Passivo Circulante")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Empréstimos e Financiamentos a Longo Prazo":
                    case "Empréstimos de Coligadas e Controladas":
                    case "Impostos Parcelados":
                    case "Passivos Contingentes":
                    case "Contas Transitórias":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Total Passivo Não Circulante")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Capital Social":
                    case "Reservas":
                    case "Lucros / Prejuízos Acumulados":
                    case "Distribuição de Lucro":
                    case "Resultado do Exercício Acumulado":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Patrimônio Liquido")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;
                }
            }
        }
        private void MapDRETotalizer(List<AccountPlanClassification> models, List<TotalizerClassificationModel> totalizerClassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.DRE))
            {
                switch (classification.Name)
                {

                    case "Vendas de Produtos":
                    case "Vendas de Mercadorias":
                    case "Prestação de Serviço":
                    case "Receita Com Locação":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Receita Operacional Bruta")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "(-) Devoluções de Vendas":
                    case "(-) Abatimentos":
                    case "(-) Impostos e Contribuições":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "(-) Deduções da Receita Bruta")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "(-) Custos das Mercadorias":
                    case "(-) Custos dos Serviços Prestados":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "(=) Receita Líquida de Vendas")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Despesas Variáveis":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Lucro Bruto")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Despesas com Vendas":
                    case "Despesas com Pessoal e Encargos":
                    case "Despesas Administrativas e Gerais":
                    case "Outros  Resultados Operacionais":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "(-) Despesas Operacionais")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Ganhos e Perdas de Capital":
                    case "Outras Receitas não Operacionais":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Lucro Operacional")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Receitas Financeiras":
                    case "Despesas Financeiras":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Lucro Antes do Resultado Financeiro")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Provisão para CSLL":
                    case "Provisão para IRPJ":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Resultado do Exercício Antes do Imposto")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;

                    case "Despesas com Depreciação":
                        classification.TotalizerClassificationId = totalizerClassifications
                            .Where(r => r.Name == "Lucro Líquido do Periodo")
                            .Select(r => r.Id)
                            .FirstOrDefault();
                        break;
                }
            }
        }

        private void MapPassivos(List<AccountPlanClassification> models, List<BalancoReclassificadoModel> reclassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.Passivo)) // exemplo filtro Passivo
            {
                switch (classification.Name)
                {

                    case "Empréstimos e Financiamentos":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Empréstimos e Financiamentos")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Fornecedores Fábrica":
                    case "Fornecedores Diversos":
                    case "(-) Devolução de Compras":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Fornecedores")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Obrigações Fiscais a Pagar":
                    case "Obrigações Sociais a Pagar":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Obrigações Tributárias e Trabalhistas")
                            .Select(r =>     (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Outras Exigibilidades":
                    case "Seguros a Pagar":
                    case "Outras Contas a Pagar":
                    case "Creditos de Clientes":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Outros Passivos Operacionais")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Empréstimos e Financiamentos a Longo Prazo":
                    case "Impostos Parcelados":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Passivo não Circulante Financeiro")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Empréstimos de Coligadas e Controladas":
                    case "Passivos Contingentes":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Passivo não Circulante Operacional")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Contas Transitórias":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Contas Transitórias")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;


                    case "Capital Social":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Capital Social")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Reservas":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Reservas")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Lucros / Prejuízos Acumulados":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Lucros / Prejuízos Acumulados")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Distribuição de Lucro":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Distribuição de Lucro")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Resultado do Exercício Acumulado":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Resultado Acumulado")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                }
            }
        }
        private void MapDRE(List<AccountPlanClassification> models, List<BalancoReclassificadoModel> reclassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.DRE)) // exemplo filtro Passivo
            {
             classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "DRE")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();

            }
        }
        private async Task CreateTotalizersAsync(int accountPlanId)
        {
            var totalizerTemplate = await _totalizerClassificationTemplateRepository.GetAllAsync();

            var totalizerModels = totalizerTemplate.Select(t => new TotalizerClassificationModel
            {
                AccountPlanId = accountPlanId,
                Name = t.Name,
                TypeOrder = t.TypeOrder
            }).ToList();

            await _totalizerClassificationRepository.AddRangeAsync(totalizerModels);



        }

        private async Task CreateBalancosReclassificadosAsync(int accountPlanId)
        {
            var balancoTemplate = await _balancoReclassificadoTemplateRepository.GetAllAsync();

            var balancoModels = balancoTemplate.Select(br => new BalancoReclassificadoModel
            {
                AccountPlanId = accountPlanId,
                Name = br.Name,
                TypeOrder = br.TypeOrder
            }).ToList();

            await _balancoReclassificadoRepository.AddRangeAsync(balancoModels);
        }


        public async Task<ResultValue> GetPainelMonths(int accountPlanId, int year, int type)
        {
            if (type == 1) // Ativo
                return await GerarPainelAtivo(accountPlanId, year);
            else if (type == 2) // Passivo
                return await GerarPainelPassivo(accountPlanId, year);
            else
                return ErrorResponse("Tipo inválido.");
        }
        

        public async Task<ResultValue> CreateItemClassification(int accountplanId, CreateItemClassification dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // Reorganiza antes de adicionar o novo item
                await ReorganizeTypeOrders(accountplanId, dto.TypeClassification, dto.TypeOrder);

                var model = new AccountPlanClassification
                {
                    Name = dto.Name,
                    TypeClassification = (ETypeClassification)dto.TypeClassification,
                    TypeOrder = dto.TypeOrder,
                    AccountPlanId = accountplanId
                };

                await _accountClassificationRepository.AddAsync(model);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> Update(int accountplanId, int id, UpdateItemClassification dto)
        {
            try
            {
                var user = GetCurrentUserId();

                var accountPlanClassification = await _accountClassificationRepository.GetByAccountIdAndId(accountplanId, id);

                if (accountPlanClassification == null)
                    return ErrorResponse("Item não encontrado");


                // Atualiza os dados do item
                accountPlanClassification.Name = dto.Name;
                accountPlanClassification.TypeClassification = (ETypeClassification)dto.TypeClassification;


                await _accountClassificationRepository.Update(accountPlanClassification);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAccountPlanClassification(int accountPlanId)
        {
            try
            {
                var exists = await _accountClassificationRepository.ExistsAccountPlanClassification(accountPlanId);

                return SuccessResponse(exists);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        #region Vinculos

        public async Task<ResultValue> CreateBond(int accountPlanClassificationId, BalanceteDataAccountPlanClassificationCreate dto)
        {
            try
            {
                var models = dto.CostCenters.Select(cc => new BalanceteDataAccountPlanClassification
                {
                    AccountPlanClassificationId = accountPlanClassificationId,
                    CostCenter = cc.CostCenter
                }).ToList();

                await _accountClassificationRepository.CreateBond(models);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> CreateBondList(BalanceteDataAccountPlanClassificationCreateList dto)
        {
            try
            {
                var models = dto.BondList.SelectMany(bond => bond.CostCenters.Select(cc => new BalanceteDataAccountPlanClassification
                {
                    AccountPlanClassificationId = bond.AccountPlanClassificationId,
                    CostCenter = cc.CostCenter
                })).ToList();

                await _accountClassificationRepository.CreateBond(models);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> GetBond(int accountPlanId, int typeClassification)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();
                var balancete = await _balanceteDataRepository.GetAgrupadoPorCostCenterListAsync(costCenters);

                var response = model
                    .GroupBy(x => new { x.AccountPlanClassificationId, x.AccountPlanClassification.Name })
                    .Select(group =>
                    {
                        var groupCostCenters = model
                            .Where(m => m.AccountPlanClassificationId == group.Key.AccountPlanClassificationId)
                            .Select(m => m.CostCenter)
                            .ToList();

                        var totalValue = balancete
                            .Where(b => groupCostCenters.Contains(b.CostCenter))
                            .Sum(b => b.FinalValue);

                        return new BalanceteDataAccountPlanClassificationResponse
                        {
                            Id = group.Key.AccountPlanClassificationId,
                            Name = group.Key.Name,
                            Value = totalValue
                        };
                    })
                    .ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }







       


        public async Task<PainelBalancoContabilRespone> GetPainelBalancoAsync(int accountPlanId, int year, int typeClassification)
        {
            return typeClassification switch
            {
                1 => await BuildPainelAtivo(accountPlanId, year),
               // 2 => await BuildPainelPassivo(accountPlanId, year),
              //  3 => await BuildPainelDRE(accountPlanId, year),
                _ => throw new ArgumentException("Tipo de classificação inválido.")
            };
        }





        private async Task<PainelBalancoContabilRespone> BuildPainelAtivo(int accountPlanId, int year)
        {
            return await BuildPainelByTypeAtivo(accountPlanId, year, 1);
        }


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












        public async Task<PainelBalancoContabilRespone> GetPainelBalancoAsyncc(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _balanceteRepository.GetBalancetesByCostCenter(accountPlanId, year, typeClassification);

            // 1. Filtra as classificações pelo typeClassification desejado.
            var classifications = (await _accountClassificationRepository.GetAllAsync(accountPlanId))
                .Where(c => (int)c.TypeClassification == typeClassification) // Já é int, sem precisar de cast (int)
                .ToList();

            // 2. Identifica os IDs dos totalizadores que possuem classificações do tipo desejado.
            var totalizerIdsValidos = classifications
                .Where(c => c.TotalizerClassificationId.HasValue)
                .Select(c => c.TotalizerClassificationId.Value)
                .Distinct()
                .ToList();

            // 3. Pega todos os totalizadores e, em seguida, filtra apenas os que têm classificações do tipo desejado.
            var totalizers = (await _totalizerClassificationRepository.GetByAccountPlanId(accountPlanId))
                .Where(t => totalizerIdsValidos.Contains(t.Id))
                .ToList();

            // Pré-carrega dados necessários para balanceteData e balanceteDataClassifications
            var balanceteIds = balancetes.Select(b => b.Id).ToList();
            var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification); // Se este método retorna BalanceteDataAccountPlanClassification
            var costCenters = model.Select(a => a.CostCenter).ToList();

            // Pega todos os balanceteData relevantes
            var allBalanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);
            var allBalanceteDataClassifications = await _balanceteDataRepository.GetByAccountPlanClassificationId(accountPlanId);

            // Otimização 1: Criar um dicionário para lookup rápido de balanceteData
            // Chave: (BalanceteId, CostCenter), Valor: Lista de BalanceteData
            var balanceteDataLookup = allBalanceteData
                .GroupBy(bd => (bd.BalanceteId, bd.CostCenter))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Otimização 2: Criar um dicionário para lookup rápido de balanceteDataClassifications
            // Chave: AccountPlanClassificationId, Valor: Lista de BalanceteDataAccountPlanClassification
            var balanceteDataClassificationsLookup = allBalanceteDataClassifications
                .GroupBy(x => x.AccountPlanClassificationId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var months = balancetes.Select(balancete => new MonthPainelContabilRespone
            {
                Id = balancete.Id,
                Name = balancete.DateMonth.GetDescription(),
                DateMonth = (int)balancete.DateMonth,

                Totalizer = totalizers.Select(totalizer =>
                {
                    // Esta 'classifications' já está pré-filtrada por typeClassification
                    var relatedClassifications = classifications
                        .Where(c => c.TotalizerClassificationId == totalizer.Id)
                        .ToList();

                    var classificationsResp = relatedClassifications.Select(classification =>
                    {
                        var datas = new List<BalanceteDataResponse>();

                        // Obter as entradas BalanceteDataAccountPlanClassification para a classificação atual
                        if (balanceteDataClassificationsLookup.TryGetValue(classification.Id, out var bdcEntries))
                        {
                            foreach (var bdcEntry in bdcEntries)
                            {
                                // Usar o lookup otimizado para balanceteData
                                if (balanceteDataLookup.TryGetValue((balancete.Id, bdcEntry.CostCenter), out var dataForCostCenter))
                                {
                                    datas.AddRange(dataForCostCenter.Select(bd => new BalanceteDataResponse
                                    {
                                        Id = bd.Id,
                                        CostCenter = bd.CostCenter,
                                        Name = bd.Name,
                                        Value = bd.FinalValue
                                    }));
                                }
                            }
                        }

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

                }).OrderBy(t => t.TypeOrder).ToList() // Ordenar totalizadores por TypeOrder

            }).OrderBy(a => a.DateMonth).ToList(); // Ordenar meses por DateMonth

            return new PainelBalancoContabilRespone { Months = months };
        }
    













public async Task<ResultValue> GetBondMonth(int accountPlanId, int balanceteId, int typeClassification)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();
                var balancete = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMonthAsync(costCenters, balanceteId);
                var balanceteMonth = await _balanceteRepository.GetBalanceteById(balanceteId);

                var factor = typeClassification == 2 ? -1 : 1;

                var classifications = model
                    .GroupBy(x => new
                    {
                        x.AccountPlanClassificationId,
                        x.AccountPlanClassification.Name,
                        x.AccountPlanClassification.TypeOrder
                    })
                    .Select(group =>
                    {
                        var groupCostCenters = group.Select(m => m.CostCenter).ToList();

                        var totalValue = balancete
                            .Where(b => groupCostCenters.Contains(b.CostCenter))
                            .Sum(b => b.FinalValue) * factor;

                        return new BalanceteDataAccountPlanClassificationResponse
                        {
                            Id = group.Key.AccountPlanClassificationId,
                            Name = group.Key.Name,
                            Value = totalValue
                        };
                    })
                    .OrderBy(x => model.First(m => m.AccountPlanClassificationId == x.Id).AccountPlanClassification.TypeOrder)
                    .ToList();

                var response = new MonthBalanceteDataAccountPlanClassificationResponse
                {
                    Month = balanceteMonth.DateMonth.GetDescription(),
                    Year = balanceteMonth.DateYear,
                    Classifications = classifications
                };

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> GetBondMonths(int accountPlanId, int typeClassification)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();

                // Busca todos os balancetes relacionados aos cost centers
                var balancetes = await _balanceteRepository.GetBalancetesByCostCenters(costCenters);

                if (balancetes == null || !balancetes.Any())
                    return ErrorResponse("Nenhum balancete encontrado.");

                var balanceteIds = balancetes.Select(b => b.Id).ToList();

                // Busca dados do balancete agrupados por cost center e balanceteId
                var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);

                var factor = typeClassification == 2 ? -1 : 1;

                // Montar resposta agrupando por balancete (mês e ano)
                var response = balancetes
                    .OrderBy(b => b.DateYear)
                    .ThenBy(b => b.DateMonth)
                    .Select(bal =>
                    {
                        var classifications = model
                            .GroupBy(x => new
                            {
                                x.AccountPlanClassificationId,
                                x.AccountPlanClassification.Name,
                                x.AccountPlanClassification.TypeOrder
                            })
                            .Select(group =>
                            {
                                var groupCostCenters = group.Select(m => m.CostCenter).ToList();

                                var totalValue = balanceteData
                                    .Where(bd => bd.BalanceteId == bal.Id && groupCostCenters.Contains(bd.CostCenter))
                                    .Sum(bd => bd.FinalValue) * factor;

                                return new BalanceteDataAccountPlanClassificationResponse
                                {
                                    Id = group.Key.AccountPlanClassificationId,
                                    Name = group.Key.Name,
                                    Value = totalValue
                                };
                            })
                            .OrderBy(x => model.First(m => m.AccountPlanClassificationId == x.Id).AccountPlanClassification.TypeOrder)
                            .ToList();

                        return new MonthBalanceteDataAccountPlanClassificationResponse
                        {
                            Month = bal.DateMonth.GetDescription(),
                            Year = bal.DateYear,
                            Classifications = classifications
                        };
                    })
                    .ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> GetPainelMonths(int accountPlanId, int year)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, 1);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();
                var balancetes = await _balanceteRepository.GetBalancetesByCostCenterAtivo(accountPlanId, year);
                if (balancetes == null || !balancetes.Any())
                    return ErrorResponse("Nenhum balancete encontrado.");

                var balanceteIds = balancetes.Select(b => b.Id).ToList();
                var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);

                var response = new PainelMensalAtivoResponse
                {
                    Year = year,
                    Meses = balancetes.OrderBy(b => b.DateMonth).Select(bal =>
                    {
                        // Agrupar classificações para o mês atual (bal)
                        var classificationsMonth = model
                            .GroupBy(x => new
                            {
                                x.AccountPlanClassificationId,
                                x.AccountPlanClassification.Name
                            })
                            .Select(group =>
                            {
                                var groupCostCenters = group.Select(m => m.CostCenter).ToList();

                                var totalValue = balanceteData
                                    .Where(bd => bd.BalanceteId == bal.Id && groupCostCenters.Contains(bd.CostCenter))
                                    .Sum(bd => bd.FinalValue);

                                return new BalanceteDataAccountPlanClassificationResponseteste
                                {
                                    Id = group.Key.AccountPlanClassificationId,
                                    Name = group.Key.Name,
                                    Value = totalValue
                                };
                            })
                            .ToList();

                        // Filtrar classificações para cada categoria principal - ajuste os filtros conforme sua regra
                        var totalAtivoCirculante = classificationsMonth.Where(c =>
                            c.Name == "Caixa e Equivalente de Caixa" ||
                            c.Name == "Aplicação Financeira" ||
                            c.Name == "Clientes" ||
                            c.Name == "Outros Creditos" ||
                            c.Name == "Adiantamentos" ||
                            c.Name == "Tributos a Recuperar" ||
                            c.Name == "Estoques").ToList();

                        var totalLongoPrazo = classificationsMonth.Where(c =>
                            c.Name == "Empréstimos a Coligadas e Controladas" ||
                            c.Name == "Depósitos Judiciais"
                            ).ToList();
                        var totalPermanente = classificationsMonth.Where(c =>
                            c.Name == "Investimentos" ||
                            c.Name == "Imobilizado" ||
                            c.Name == "( - ) Depreciação Acumuladas" ||
                            c.Name == "Intangivel / Diferido").ToList();
                        var totalAtivoNaoCirculante = classificationsMonth.Where(c =>
                            c.Name == "Ativo Compensado" ||
                            c.Name == "Contas Transitórias Ativo"
                            ).ToList();

                        return new MesAtivoPainel
                        {
                            Month = bal.DateMonth.GetDescription(),
                            TotalAtivoCirculante = new TotalAtivoCirculante
                            {
                                Classifications = totalAtivoCirculante,
                                value = totalAtivoCirculante.Sum(c => c.Value)
                            },
                            TotalLongoPrazo = new TotalLongoPrazo
                            {
                                Classifications = totalLongoPrazo,
                                value = totalLongoPrazo.Sum(c => c.Value)
                            },
                            TotalPermanente = new TotalPermanente
                            {
                                Classifications = totalPermanente,
                                value = totalPermanente.Sum(c => c.Value)
                            },
                            TotalAtivoNaoCirculante = new TotalAtivoNaoCirculante
                            {
                                Classifications = totalAtivoNaoCirculante,
                                value = totalAtivoNaoCirculante.Sum(c => c.Value)
                            },
                            TotalGeralDoAtivo = new TotalGeralDoAtivo
                            {
                                value = totalAtivoCirculante.Sum(c => c.Value)
                                      + totalLongoPrazo.Sum(c => c.Value)
                                      + totalPermanente.Sum(c => c.Value)
                                      + totalAtivoNaoCirculante.Sum(c => c.Value)
                            }
                        };
                    }).ToList()
                };

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetPainelTypeMonths(int accountPlanId, int year, int type)
        {
            if (type == 1) // Ativo
                return await GerarPainelAtivo(accountPlanId, year);
            else if (type == 2) // Passivo
                return await GerarPainelPassivo(accountPlanId, year);
            else
                return ErrorResponse("Tipo inválido.");
        }
        private async Task<ResultValue> GerarPainelAtivo(int accountPlanId, int year)
        {
            return await GerarPainelPorBond(accountPlanId, year, 1);
        }
        private async Task<ResultValue> GerarPainelPorBond(int accountPlanId, int year, int bondType)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, bondType);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);
               
                return SuccessResponse(model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        private async Task<ResultValue> GerarPainelPassivo(int accountPlanId, int year)
        {
            return await GerarPainelPorBondPassivo(accountPlanId, year, 2);
        }

        private async Task<ResultValue> GerarPainelPorBondPassivo(int accountPlanId, int year, int bondType)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, bondType);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();
                var balancetes = await _balanceteRepository.GetBalancetesByCostCenter(accountPlanId, year, bondType);
                if (balancetes == null || !balancetes.Any())
                    return ErrorResponse("Nenhum balancete encontrado.");

                var balanceteIds = balancetes.Select(b => b.Id).ToList();
                var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);

                var response = new PainelMensalPassivoResponse
                {
                    Year = year,
                    Meses = balancetes.OrderBy(b => b.DateMonth).Select(bal =>
                    {
                        var classificationsMonth = model
                            .GroupBy(x => new
                            {
                                x.AccountPlanClassificationId,
                                x.AccountPlanClassification.Name
                            })
                            .Select(group =>
                            {
                                var groupCostCenters = group.Select(m => m.CostCenter).ToList();
                                var totalValue = balanceteData
                                    .Where(bd => bd.BalanceteId == bal.Id && groupCostCenters.Contains(bd.CostCenter))
                                    .Sum(bd => bd.FinalValue) * -1; // Inversão aplicada aqui

                                return new BalanceteDataAccountPlanClassificationResponseteste
                                {
                                    Id = group.Key.AccountPlanClassificationId,
                                    Name = group.Key.Name,
                                    Value = totalValue
                                };
                            })
                            .ToList();

                        var (passivoCirculante, passivoNaoCirculante, patrimonioLiquido, passivoCompensado) =
                            CategorizarPassivo(classificationsMonth);

                        return new MesPassivoPainel
                        {
                            Month = bal.DateMonth.GetDescription(),
                            TotalPassivoCirculante = new TotalPassivoCirculante
                            {
                                Classifications = passivoCirculante,
                                Value = passivoCirculante.Sum(c => c.Value)
                            },
                            TotalPassivoNaoCirculante = new TotalPassivoNaoCirculante
                            {
                                Classifications = passivoNaoCirculante,
                                Value = passivoNaoCirculante.Sum(c => c.Value)
                            },
                            TotalPatrimonioLiquido = new TotalPatrimonioLiquido
                            {
                                Classifications = patrimonioLiquido,
                                Value = patrimonioLiquido.Sum(c => c.Value)
                            },
                            TotalPassivoCompensado = new TotalPassivoCompensado
                            {
                                Classifications = passivoCompensado,
                                Value = passivoCompensado.Sum(c => c.Value)
                            },
                            TotalGeralDoPassivo = new TotalGeralDoPassivo
                            {
                                Value = passivoCirculante.Sum(c => c.Value)
                                        + passivoNaoCirculante.Sum(c => c.Value)
                                        + patrimonioLiquido.Sum(c => c.Value)
                                        + passivoCompensado.Sum(c => c.Value)
                            }
                        };
                    }).ToList()
                };

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        private (List<BalanceteDataAccountPlanClassificationResponseteste> passivoCirculante,
         List<BalanceteDataAccountPlanClassificationResponseteste> passivoNaoCirculante,
         List<BalanceteDataAccountPlanClassificationResponseteste> patrimonioLiquido,
         List<BalanceteDataAccountPlanClassificationResponseteste> passivoCompensado)
        CategorizarPassivo(List<BalanceteDataAccountPlanClassificationResponseteste> classifications)
        {
            var passivoCirculante = classifications.Where(c =>
                new[] {
                    "Fornecedores",
                    "Outras Contas a Pagar",
                    "Empréstimos e Financiamentos", // Créditos de Clientes
                    "( - ) Devolução de Compras", // não achei
                    "Obrigações Trabalhistas",//Obrigações Sociais a Pagar
                    "Obrigações Tributárias",//Obrigações Fiscais a Pagar,
                    "Outros Passivos Operacionais"//Outras Exigibilidades
                }.Contains(c.Name)).ToList();

            var passivoNaoCirculante = classifications.Where(c =>
                new[] {
                        "Instituições Financeiras",
                         "Empréstimos de Coligadas e Controladas",
                        "Outras Contas a Pagar - LP",
                        "Outros Passivos Operacionais",
                        "Passivo Não Circulante Operacional",
                        "Passivo Não Circulante Financeiro",

                }.Contains(c.Name)).ToList();

            var patrimonioLiquido = classifications.Where(c =>
                new[] {
                    "Capital Social",
                    "Reservas De Capital",
                    "Lucros Ou Prejuizos Acumulados",
                    "Distribuição De Lucro",
                    "Resultado Do Exercício"
                }.Contains(c.Name)).ToList();

            var passivoCompensado = classifications.Where(c =>
                new[] {
                    "Passivo Compensado",
                    "Contas Transitórias Passivo",
                    "Apuração e Encerramento",
                    "Custo com Depreciação"
                }.Contains(c.Name)).ToList();

            return (passivoCirculante, passivoNaoCirculante, patrimonioLiquido, passivoCompensado);
        }



        


        private (List<BalanceteDataAccountPlanClassificationResponseteste> circulante,
         List<BalanceteDataAccountPlanClassificationResponseteste> longoPrazo,
         List<BalanceteDataAccountPlanClassificationResponseteste> permanente,
         List<BalanceteDataAccountPlanClassificationResponseteste> naoCirculante)


    CategorizarPorBond(List<BalanceteDataAccountPlanClassificationResponseteste> classifications, int bondType)
        {
            if (bondType == 1) // Ativo
            {
                var totalCirculante = classifications.Where(c =>
                    new[] { "Caixa e Equivalente de Caixa", "Aplicação Financeira", "Clientes", "Outros Ativos Operacionais", "Adiantamentos", "Tributos a Recuperar", "Estoques" }
                    .Contains(c.Name)).ToList();

                var totalLongoPrazo = classifications.Where(c =>
                    new[] { "Empréstimos a Coligadas e Controladas", "Depósitos Judiciais" }
                    .Contains(c.Name)).ToList();

                var totalPermanente = classifications.Where(c =>
                    new[] { "Investimentos", "Imobilizado", "( - ) Depreciação Acumuladas", "Intangível / Diferido", "( - ) Amortização Acumuladas" }
                    .Contains(c.Name)).ToList();

                var totalNaoCirculante = classifications.Where(c =>
                    new[] { "Ativo Compensado", "Contas Transitórias", "Ativo Não Circulante Financeiro", "Ativo Não Circulante Operacional" }
                    .Contains(c.Name)).ToList();

                return (totalCirculante, totalLongoPrazo, totalPermanente, totalNaoCirculante);
            }
            else if (bondType == 2) // Passivo (exemplo)
            {
                // Quando for passivo você cria outras regras
                var totalCirculante = classifications.Where(c =>
                    new[] { "Fornecedores", "Empréstimos Bancários", "Salários a Pagar" }
                    .Contains(c.Name)).ToList();

                var totalLongoPrazo = classifications.Where(c =>
                    new[] { "Financiamentos de Longo Prazo", "Provisões" }
                    .Contains(c.Name)).ToList();

                var totalPermanente = new List<BalanceteDataAccountPlanClassificationResponseteste>();
                var totalNaoCirculante = new List<BalanceteDataAccountPlanClassificationResponseteste>();

                return (totalCirculante, totalLongoPrazo, totalPermanente, totalNaoCirculante);
            }

            // Default vazio
            return (new List<BalanceteDataAccountPlanClassificationResponseteste>(),
                    new List<BalanceteDataAccountPlanClassificationResponseteste>(),
                    new List<BalanceteDataAccountPlanClassificationResponseteste>(),
                    new List<BalanceteDataAccountPlanClassificationResponseteste>());
        }





        public async Task<ResultValue> GetBondDREMonth(int accountPlanId, int balanceteId, int typeClassification)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();
                var balancete = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMonthAsync(costCenters, balanceteId);
                var balanceteMonth = await _balanceteRepository.GetBalanceteById(balanceteId);

                var classifications = model
                                 .GroupBy(x => new { x.AccountPlanClassificationId, x.AccountPlanClassification.Name, x.AccountPlanClassification.TypeOrder })
                                 .Select(group =>
                                 {
                                     var groupCostCenters = group.Select(m => m.CostCenter).ToList();

                                     var totalValue = balancete
                                         .Where(b => groupCostCenters.Contains(b.CostCenter))
                                         .Sum(b => b.FinalValue - b.InitialValue);

                                     return new BalanceteDataAccountPlanClassificationResponse
                                     {
                                         Id = group.Key.AccountPlanClassificationId,
                                         Name = group.Key.Name,
                                         Value = totalValue
                                     };
                                 })
                                 .OrderBy(x => model.First(m => m.AccountPlanClassificationId == x.Id).AccountPlanClassification.TypeOrder)
                                 .ToList();

                var response = new MonthBalanceteDataAccountPlanClassificationResponse
                {
                    Month = balanceteMonth.DateMonth.GetDescription(),
                    Year = balanceteMonth.DateYear,
                    Classifications = classifications
                };

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> GetBondDREMonths(int accountPlanId, int typeClassification)
        {
            try
            {
                var model = await _accountClassificationRepository.GetBond(accountPlanId, typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var costCenters = model.Select(a => a.CostCenter).ToList();

                // Busca todos os balancetes relacionados aos cost centers
                var balancetes = await _balanceteRepository.GetBalancetesByCostCenters(costCenters);

                if (balancetes == null || !balancetes.Any())
                    return ErrorResponse("Nenhum balancete encontrado.");

                var balanceteIds = balancetes.Select(b => b.Id).ToList();

                // Busca dados do balancete agrupados por cost center e balanceteId
                var balanceteData = await _balanceteDataRepository.GetAgrupadoPorCostCenterListMultiBalancete(costCenters, balanceteIds);

                // Montar resposta agrupando por balancete (mês e ano)
                var response = balancetes
                    .OrderBy(b => b.DateYear)
                    .ThenBy(b => b.DateMonth)
                    .Select(bal =>
                    {
                        var classifications = model
                            .GroupBy(x => new { x.AccountPlanClassificationId, x.AccountPlanClassification.Name, x.AccountPlanClassification.TypeOrder })
                            .Select(group =>
                            {
                                var groupCostCenters = group.Select(m => m.CostCenter).ToList();

                                var totalValue = balanceteData
                                    .Where(bd => bd.BalanceteId == bal.Id && groupCostCenters.Contains(bd.CostCenter))
                                    .Sum(b => b.FinalValue - b.InitialValue);

                                return new BalanceteDataAccountPlanClassificationResponse
                                {
                                    Id = group.Key.AccountPlanClassificationId,
                                    Name = group.Key.Name,
                                    Value = totalValue
                                };
                            })
                            .OrderBy(x => model.First(m => m.AccountPlanClassificationId == x.Id).AccountPlanClassification.TypeOrder)
                            .ToList();

                        return new MonthBalanceteDataAccountPlanClassificationResponse
                        {
                            Month = bal.DateMonth.GetDescription(),
                            Year = bal.DateYear,
                            Classifications = classifications
                        };
                    })
                    .ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        #endregion


        #region Private 
        private async Task ReorganizeTypeOrders(int accountPlanId, int typeClassification, int typeOrder)
        {
            var classifications = await _accountClassificationRepository
                .GetAllAfterTypeOrderAsync(accountPlanId, typeClassification, typeOrder);

            if (classifications.Any())
            {
                foreach (var item in classifications)
                {
                    item.TypeOrder += 1;
                }

                await _accountClassificationRepository.UpdateRange(classifications);
            }
        }

        private AccountPlanClassificationResponse MapToClassificationRealResponse(AccountPlanClassification model)
        {
            return new AccountPlanClassificationResponse
            {
                Id = model.Id,
                AccountPlanId = model.AccountPlanId,
                Name = model.Name,
                TypeOrder = model.TypeOrder,
                TypeClassification = model.TypeClassification.GetDescription()
            };
        }

        #endregion

        #endregion
        #endregion
    }
}
