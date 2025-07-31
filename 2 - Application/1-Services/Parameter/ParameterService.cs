using _2___Application._2_Dto_s.Parameter;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.Parameter
{
    public class ParameterService : BaseService
    {
        private readonly ParameterRepository _repository;
        private readonly AccountPlansRepository _accountPlansRepository;

        public ParameterService(
            ParameterRepository repository,
            AccountPlansRepository accountPlansRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository;
            _accountPlansRepository = accountPlansRepository;
        }

        #region Métodos

        public async Task<ResultValue> Create(InsertParameterDto dto)
        {
            try
            {

                var accountPlan = await _accountPlansRepository.GetByaccountPlanId(dto.AccountPlanId);

                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                var user = GetCurrentUserId();

                var model = new ParameterModel
                {
                    Name = dto.Name,
                    AccountPlansId = accountPlan.Id,
                    ParameterYear = dto.ParameterYear,
                    ParameterValue = dto.ParameterValue,
                };

                await _repository.AddAsync(model);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAll(int accountPlanId)
        {
            try
            {
                var parameters = await _repository.GetAllAsync();

                if (parameters == null || !parameters.Any())
                    return ErrorResponse(Message.NotFound);

                var result = parameters
                    .Where(p => p.AccountPlansId == accountPlanId)
                    .Select(p => new ParameterResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ParameterValue = p.ParameterValue,
                        ParameterYear = p.ParameterYear,
                    })
                    .ToList();

                return SuccessResponse(result);
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
                var parameter = await _repository.GetById(id);

                if (parameter == null )
                    return ErrorResponse(Message.NotFound);

                var result = new ParameterResponseDto
                {
                    Id = parameter.Id,
                    Name = parameter.Name,
                    ParameterValue = parameter.ParameterValue,
                    ParameterYear = parameter.ParameterYear,
                };

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> Update(UpdateParameterDto dto)
        {
            try
            {
                var parameter = await _repository.GetById(dto.Id);

                if (parameter == null)
                    return ErrorResponse(Message.NotFound);

                parameter.Name = dto.Name;
                parameter.ParameterValue = dto.ParameterValue;
                parameter.ParameterYear = dto.ParameterYear;

                await _repository.Update(parameter);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> Delete(int id)
        {
            try
            {
                var parameter = await _repository.GetById(id);

                if (parameter == null)
                    return ErrorResponse(Message.NotFound);

                
                await _repository.DeletePermanently(parameter.Id);

                return SuccessResponse(Message.DeleteSuccess);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        #endregion
    }
}
