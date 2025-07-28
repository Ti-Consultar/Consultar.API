using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.TotalizerClassification;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.TotalizerClassification
{

    public class TotalizerClassificationService : BaseService
    {
        private readonly ClassificationRepository _classificationRepository;
        private readonly AccountPlanClassificationRepository _accountClassificationRepository;
        private readonly BalanceteDataRepository _balanceteDataRepository;
        private readonly TotalizerClassificationRepository _repository;
        private readonly BalanceteRepository _balanceteRepository;
        private readonly TotalizerClassificationTemplateRepository _totalizerClassificationRepository;

        public TotalizerClassificationService(
            ClassificationRepository classificationRepository,
            AccountPlanClassificationRepository accountClassificationRepository,
            BalanceteDataRepository balanceteDataRepository,
            BalanceteRepository balanceteRepository,
            TotalizerClassificationRepository repository,
            TotalizerClassificationTemplateRepository totalizerClassificationRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _classificationRepository = classificationRepository;
            _accountClassificationRepository = accountClassificationRepository;
            _balanceteDataRepository = balanceteDataRepository;
            _balanceteRepository = balanceteRepository;
            _repository = repository;
            _totalizerClassificationRepository = totalizerClassificationRepository;
        }

        #region Métodos
        public async Task<ResultValue> GetAll()
        {
            try
            {
                var model = await _totalizerClassificationRepository.GetAllAsync();
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .OrderBy(x => x.TypeOrder) // Ordenação crescente por Type
                    .Select(MapToTotalizerClassificationTemplateResponse)
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
                var model = await _totalizerClassificationRepository.GetById(id);
                if (model == null)
                    return ErrorResponse(Message.NotFound);

                var response = MapToTotalizerClassificationTemplateResponse(model);

                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        #region Private
        private TotalizerClassificationTemplateResponse MapToTotalizerClassificationTemplateResponse(TotalizerClassificationTemplate model)
        {
            return new TotalizerClassificationTemplateResponse
            {
                Id = model.Id,
                Name = model.Name,
                TypeOrder = model.TypeOrder,
 
            };
        }
        #endregion

        #endregion
    }
}
