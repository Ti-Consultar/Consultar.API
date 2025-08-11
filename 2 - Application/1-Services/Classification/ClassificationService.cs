using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.Classification.AccountPlanClassification;
using _2___Application._2_Dto_s.Painel;
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
                //MapDRE(models, reclassifications);
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
                            .Where(r => r.Name == "Contas Transitórias Ativo")
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
                            .Where(r => r.Name == "Total Ativo Não Circulante")
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
                            .Select(r => (int?)r.Id)
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
                            .Where(r => r.Name == "Passivo Não Circulante Financeiro")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Empréstimos de Coligadas e Controladas":
                    case "Passivos Contingentes":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Passivo Não Circulante Operacional")
                            .Select(r => (int?)r.Id)
                            .FirstOrDefault();
                        break;

                    case "Contas Transitórias":
                        classification.BalancoReclassificadoId = reclassifications
                            .Where(r => r.Name == "Contas Transitórias Passivo")
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

                var response = model
                    .GroupBy(x => new {
                        x.AccountPlanClassificationId,
                        x.AccountPlanClassification.Name
                    })
                    .Select(group => new BalanceteDataAccountPlanClassificationResponse
                    {
                        Id = group.Key.AccountPlanClassificationId,
                        Name = group.Key.Name,
                        CostCenters = group.Select(g => g.CostCenter).Distinct().ToList()
                    })
                    .ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> GetPainelBalancoAsync(int accountPlanId, int year, int typeClassification)
        {
            var result = typeClassification switch
            {
                1 => await BuildPainelAtivo(accountPlanId, year),
                2 => await BuildPainelPassivo(accountPlanId, year),
                3 => await BuildPainelDRE(accountPlanId, year),
                _ => throw new ArgumentException("Tipo de classificação inválido.")
            };

            return SuccessResponse(result); // Aqui retorna a estrutura padronizada
        }


        private async Task<PainelBalancoContabilRespone> BuildPainelAtivo(int accountPlanId, int year)
        {
            return await BuildPainelByTypeAtivo(accountPlanId, year, 1);
        }
        private async Task<PainelBalancoContabilRespone> BuildPainelPassivo(int accountPlanId, int year)
        {
            return await BuildPainelByTypePassivo(accountPlanId, year, 2);
        }

        private async Task<PainelBalancoContabilRespone> BuildPainelDRE(int accountPlanId, int year)
        {
            return await BuildPainelByTypeeDRE(accountPlanId, year, 3);
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

            var painelDRE = await BuildPainelByTypeeDRE(accountPlanId, year, 3); // Painel da DRE para pegar o lucro líquido



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

                
                var totalPassivoCirculante = totalizerResponses
                    .FirstOrDefault(c => c.Name == "Total Passivo Circulante")?.TotalValue ?? 0;

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

        private async Task<PainelBalancoContabilRespone> BuildPainelByTypeDRE(int accountPlanId, int year, int typeClassification)
        {
            var balancetes = await _balanceteRepository.GetByAccountPlanIdMonth(accountPlanId, year);
            var classifications = await _accountClassificationRepository.GetAllBytypeClassificationDREAsync(accountPlanId, typeClassification);
            var totalizers = await _totalizerClassificationRepository.GetByAccountPlansId(accountPlanId);
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

                // Map para facilitar lookup rápido
                var totalizerMap = totalizerResponses.ToDictionary(t => t.Name);
                var classificationMap = totalizerResponses
                    .SelectMany(t => t.Classifications)
                    .ToDictionary(c => c.Name);

                // Aplicar regras de totalização especiais (Lucros, NOPAT, etc)
                for (int i = 0; i < 3; i++) // Executar várias vezes para garantir dependências resolvidas
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
                    var percentage = ApplyDREPercentageRules(totalizer.Name, totalizerMap, totalizer.TotalValue);
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


        private decimal? ApplyDRETotalValueRules(
    string name,
    Dictionary<string, TotalizerParentRespone> totals,
    Dictionary<string, ClassificationRespone> classes)
        {
            decimal GetValue(string key) =>
                totals.TryGetValue(key, out var t) ? t.TotalValue :
                classes.TryGetValue(key, out var c) ? c.Value : 0;

            return name switch
            {
                "(=) Receita Líquida de Vendas" => GetValue("Receita Operacional Bruta") - GetValue("(-) Deduções da Receita Bruta"),
                "Lucro Bruto" => GetValue("(=) Receita Líquida de Vendas") - GetValue("(-) Custos das Mercadorias"),
                "Margem Contribuição" => GetValue("Lucro Bruto") - GetValue("Despesas Variáveis"),
                "Lucro Operacional" => GetValue("Lucro Bruto") - GetValue("(-) Despesas Operacionais") + GetValue("Outros Resultados Operacionais"),
                "Lucro Antes do Resultado Financeiro" => GetValue("Lucro Operacional") + GetValue("Outras Receitas Não Operacionais") + GetValue("Ganhos e Perdas de Capital"),
                "Resultado do Exercício Antes do Imposto" => GetValue("Lucro Antes do Resultado Financeiro") + GetValue("Receitas Financeiras") - GetValue("Despesas Financeiras"),
                "Lucro Líquido do Periodo" => GetValue("Resultado do Exercício Antes do Imposto") - GetValue("Provisão para CSLL") - GetValue("Provisão para IRPJ"),
                "EBITDA" => GetValue("Lucro Antes do Resultado Financeiro") + GetValue("Despesas com Depreciação"),
                "NOPAT" => GetValue("Lucro Antes do Resultado Financeiro") - GetValue("Provisão para CSLL") - GetValue("Provisão para IRPJ"),
                _ => null
            };
        }


        private decimal? ApplyDREPercentageRules(string name, Dictionary<string, TotalizerParentRespone> totals, decimal? totalValue)
        {
            decimal Get(string key) => totals.TryGetValue(key, out var t) ? t.TotalValue : 0;

            decimal SafeDivide(decimal numerator, decimal denominator) =>
                denominator != 0 ? Math.Round(numerator / denominator * 100, 2) : 0;

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









    
        private void MapDRETotalizer(List<AccountPlanClassification> models, List<TotalizerClassificationModel> totalizerClassifications)
        {
            foreach (var classification in models.Where(m => m.TypeClassification == ETypeClassification.DRE))
            {
                int? totalizerId = classification.Name switch
                {
                    "Vendas de Produtos" or "Vendas de Mercadorias" or "Prestação de Serviço" or "Receita Com Locação"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "Receita Operacional Bruta")?.Id,

                    "(-) Devoluções de Vendas" or "(-) Abatimentos" or "(-) Impostos e Contribuições"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "(-) Deduções da Receita Bruta")?.Id,

                    "(-) Custos das Mercadorias" or "(-) Custos dos Serviços Prestados"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "(=) Receita Líquida de Vendas")?.Id,

                    "Despesas Variáveis"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "Margem Bruta %")?.Id,

                    "Despesas com Vendas" or "Despesas com Pessoal e Encargos" or "Despesas Administrativas e Gerais" or "Outros  Resultados Operacionais"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "(-) Despesas Operacionais")?.Id,

                    "Ganhos e Perdas de Capital" or "Outras Receitas não Operacionais"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "Margem Operacional %")?.Id,

                    "Receitas Financeiras" or "Despesas Financeiras"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "Margem LAJIR %")?.Id,

                    "Provisão para CSLL" or "Provisão para IRPJ"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "Margem LAIR %")?.Id,

                    "Despesas com Depreciação"
                        => totalizerClassifications.FirstOrDefault(r => r.Name == "Margem Líquida %")?.Id,

                    _ => null
                };

                if (!totalizerId.HasValue)
                    continue;

                classification.TotalizerClassificationId = totalizerId.Value;
            }
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
        public async Task<ResultValue> GetPainelBalancoReclassificadoAsync(int accountPlanId, int year, int typeClassification)
        {
            var result = typeClassification switch
            {
                1 => await BuildPainelBalancoReclassificadoAtivo(accountPlanId, year),
               2 => await BuildPainelBalancoReclassificadoPassivo(accountPlanId, year),
                _ => throw new ArgumentException("Tipo de classificação inválido.")
            };

            return SuccessResponse(result); // Aqui retorna a estrutura padronizada
        }
        private async Task<PainelBalancoContabilRespone> BuildPainelBalancoReclassificadoAtivo(int accountPlanId, int year)
        {
            return await BuildPainelBalancoReclassificadoByTypeAtivo(accountPlanId, year, 1);
        }

        private async Task<PainelBalancoContabilRespone> BuildPainelBalancoReclassificadoPassivo(int accountPlanId, int year)
        {
            return await BuildPainelBalancoReclassificadoByTypePassivo(accountPlanId, year, 2);
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
            var painelBalancoContabilPassivo= await BuildPainelByTypePassivo(accountPlanId, year, 2);


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

        private async Task<PainelBalancoContabilRespone> BuildPainelByTypeeDRE(int accountPlanId, int year, int typeClassification)
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
                    lucroOperacional.TotalValue = (lucroBruto.TotalValue + despesasOperacionais.TotalValue + outrosResultadosOperacionais) * -1;

                if (lucroAntes != null && lucroOperacional != null)
                    lucroAntes.TotalValue = (lucroOperacional.TotalValue + outrosReceitas + ganhosEPerdas)* -1;

                if (resultadoAntes != null && lucroAntes != null)
                    resultadoAntes.TotalValue = (lucroAntes.TotalValue + receitasFinanceiras + despesasFinanceiras) * -1;

                if (lucroLiquido != null && resultadoAntes != null)
                    lucroLiquido.TotalValue = (resultadoAntes.TotalValue + provisaoCSLL + provisaoIRPJ) * -1;

                if (ebitda != null && lucroAntes != null)
                    ebitda.TotalValue = lucroAntes.TotalValue + despesasDepreciacao;

                if (nopat != null && lucroAntes != null)
                    nopat.TotalValue = (lucroAntes.TotalValue + provisaoCSLL + provisaoIRPJ) * -1;

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
