using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.Classification.AccountPlanClassification;
using _2___Application._2_Dto_s.Permissions;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
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

        public ClassificationService(
            ClassificationRepository repository,
            AccountPlanClassificationRepository accountClassificationRepository,
            BalanceteDataRepository balanceteDataRepository,
            BalanceteRepository balanceteRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository;
            _accountClassificationRepository = accountClassificationRepository;
            _balanceteDataRepository = balanceteDataRepository;
            _balanceteRepository = balanceteRepository;
        }

        #region Métodos
        #region Template
        public async Task<ResultValue> GetAll()
        {
            try
            {
                var model = await _repository.GetAllAsync();
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .OrderBy(x => x.TypeOrder) // Ordenação crescente por Type
                    .Select(MapToClassificationResponse)
                    .ToList();

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> GetByTypeClassificationTemplate(ETypeClassification typeClassification)
        {
            try
            {
                var model = await _repository.GetByTypeClassification(typeClassification);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .OrderBy(x => x.TypeOrder) // Ordenação crescente por Type
                    .Select(MapToClassificationResponse)
                    .ToList();

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

                var response = MapToClassificationResponse(model);

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        #region Private
        private ClassificationResponse MapToClassificationResponse(ClassificationModel model)
        {
            return new ClassificationResponse
            {
                Id = model.Id,
                Name = model.Name,
                TypeOrder = model.TypeOrder,
                TypeClassification = model.TypeClassification.GetDescription()
            };
        }
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

                var classificationsTemplate = await _repository.GetAllAsync();

                var models = classificationsTemplate.Select(i => new AccountPlanClassification
                {
                    Name = i.Name,
                    TypeOrder = i.TypeOrder,
                    TypeClassification = i.TypeClassification,
                    AccountPlanId = dto.AccountPlanId
                }).ToList();

                await _accountClassificationRepository.AddRangeAsync(models);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
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
                var balancetes = await _balanceteRepository.GetBalancetesByCostCenterAtivo(accountPlanId,year);
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
                            c.Name == "Estoque").ToList();

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
