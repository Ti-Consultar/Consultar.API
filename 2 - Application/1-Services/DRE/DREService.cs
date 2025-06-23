using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.DRE;
using _2___Application._2_Dto_s.DRE.BalanceteDRE;
using _2___Application._2_Dto_s.Group;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.DRE
{
    public class DREService : BaseService
    {
        private readonly DRERepository _repository;
        private readonly ClassificationRepository _Classificationrepository;
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly DREBalanceteDataRepository _dREBalanceteDataRepository;

        public DREService(
            DRERepository repository,
            ClassificationRepository classificationrepository,
            AccountPlansRepository accountPlansRepository,
            DREBalanceteDataRepository dREBalanceteDataRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _Classificationrepository = classificationrepository;
            _repository = repository;
            _accountPlansRepository = accountPlansRepository;
            _dREBalanceteDataRepository = dREBalanceteDataRepository;
        }

        #region Métodos

        public async Task<ResultValue> Create(InsertDRE dto)
        {
            try
            {
                var classification = await _Classificationrepository.GetById(dto.ClassificationId);
                if (classification == null)
                    return ErrorResponse(Message.NotFound);

                var accountPlan = await _accountPlansRepository.GetById(dto.AccountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                var nextSequential = await GetNextSequentialAsync(dto.AccountPlanId);

                var dreModel = BuildDreModel(dto, nextSequential);

                await _repository.AddAsync(dreModel);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAll(int accountplanId)
        {
            try
            {
                var model = await _repository.GetByAccountPlanId(accountplanId);
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .OrderBy(x => x.Sequential) // Ordenação crescente por Sequential
                    .Select(MapToDREResponse)
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
                var model = await _repository.GetByDREId(id);
                if (model == null)
                    return ErrorResponse(Message.NotFound);

                var response = MapToDREResponse(model);

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<List<AccountPlanResponseWithDREs>> GetDREGroupedByAccountPlanAsync(int accountplanId)
        {
            var dres = await _repository.GetByAccountPlanId(accountplanId);

            var result = dres
                .GroupBy(d => d.AccountPlan)
                .Select(apGroup => new AccountPlanResponseWithDREs
                {
                    Id = apGroup.Key.Id,
                    Classifications = apGroup
                        .GroupBy(d => new { d.Classification.Id, d.Classification.Name, d.Classification.Type })
                        .Select(classGroup => new ClassificationWithDREsResponse
                        {
                            Id = classGroup.Key.Id,
                            Name = classGroup.Key.Name,
                            Type = classGroup.Key.Type,
                            DREs = classGroup.Select(d => new DREResponseSimple
                            {
                                Id = d.Id,
                                Name = d.Name,
                                Sequential = d.Sequential
                            }).ToList()
                        }).ToList()
                }).ToList();

            return result;
        }

        #region Vinculo de DRE com cada  Item do Balancete

        // Criar método para vincular o dre criado pelo usuario a varias linhas de um unico balancete e depois criar a forme de obter esses dados com o id do DRE e do BalanceteID


        public async Task<ResultValue> Vincular(BalanceteDRE dto)
        {
            try
            {
                var dre = await _repository.GetById(dto.DREId);
                if (dre == null)
                    return ErrorResponse(Message.NotFound);

                // Validação extra (opcional)
                if (dto.Items == null || !dto.Items.Any())
                    return ErrorResponse("Nenhum item de Balancete foi informado.");
                
                // Mapeia os itens da lista para os modelos que serão salvos
                var vinculos = dto.Items.Select(item => new DREBalanceteData
                {
                    DREId = dto.DREId,
                    BalanceteId = dto.BalanceteId,
                    BalanceteDataId = item.BalanceteDataId,
                    
                }).ToList();

                // Persiste os vínculos
                await _dREBalanceteDataRepository.AddRangeAsync(vinculos); // Supondo que você tenha esse método

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }




        #endregion

        #region Private
        private DREResponse MapToDREResponse(DREModel model)
        {
            return new DREResponse
            {
                Id = model.Id,
                Name = model.Name,
                Sequential = model.Sequential,
                Classification = MapToClassificationResponse(model.Classification),
                AccountPlan = new AccountPlanResponseSimple
                {
                    Id = model.AccountPlanId,
                
                }
            };
        }
        private ClassificationResponse MapToClassificationResponse(ClassificationModel model)
        {
            return new ClassificationResponse
            {
                Id = model.Id,
                Name = model.Name,
                Type = model.Type
            };
        }
        private async Task<int> GetNextSequentialAsync(int accountPlanId)
        {
            var dreList = await _repository.GetByAccountPlanId(accountPlanId);
            return dreList
                .OrderBy(d => d.Sequential)
                .LastOrDefault()?.Sequential + 1 ?? 1;
        }
        private DREModel BuildDreModel(InsertDRE dto, int sequential)
        {
            return new DREModel
            {
                Name = dto.Name,
                Sequential = sequential,
                ClassificationId = dto.ClassificationId,
                AccountPlanId = dto.AccountPlanId
            };
        }
        #endregion
        #endregion
    }
}
