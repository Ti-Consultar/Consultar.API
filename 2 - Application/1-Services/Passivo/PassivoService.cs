using _2___Application._2_Dto_s.Classification;
using _2___Application._2_Dto_s.DRE.BalanceteDRE;
using _2___Application._2_Dto_s.Passivo;
using _2___Application.Base;
using _2___Application.Passivo;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.Passivo
{
    public class PassivoService : BaseService
    {
        private readonly PassivoRepository _repository;
        private readonly PassivoBalanceteDataRepository _passivoBalanceteDataRepository;

        public PassivoService(
            PassivoRepository repository,
            IAppSettings appSettings,
            PassivoBalanceteDataRepository passivoBalanceteDataRepository) : base(appSettings)
        {
            _repository = repository;
            _passivoBalanceteDataRepository = passivoBalanceteDataRepository;
        }

        #region Métodos

        public async Task<ResultValue> GetAll()
        {
            try
            {
                var model = await _repository.GetAllAsync();
                if (model == null || !model.Any())
                    return ErrorResponse(Message.NotFound);

                var response = model
                    .OrderBy(x => x.Type) // Ordenação crescente por Type
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

        public async Task<ResultValue> Vincular(BondPassivoBalanceteData dto)
        {
            try
            {
                // Validação extra (opcional)
                if (dto.ItensBalanceteData == null || !dto.ItensBalanceteData.Any())
                    return ErrorResponse("Nenhum item de Balancete foi informado.");

                // Mapeia os itens da lista para os modelos que serão salvos
                var vinculos = dto.ItensBalanceteData.Select(item => new PassivoBalanceteDataModel
                {
                    AccountPlansId = dto.AccountPlansId,
                    BalanceteId = dto.BalanceteId,
                    PassivoId = dto.PassivoId,
                    BalanceteDataId = item.BalanceteDataId,

                }).ToList();

                // Persiste os vínculos
                await _passivoBalanceteDataRepository.AddRangeAsync(vinculos); // Supondo que você tenha esse método

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }



        #region Private
        private ClassificationPassivosResponse MapToClassificationResponse(ClassificationPassivoModel model)
        {
            return new ClassificationPassivosResponse
            {
                Id = model.Id,
                Name = model.Name,
                Type = model.Type
            };
        }

        #endregion
        #endregion
    }
}
