using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Group;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.AccountPlans.Balancete
{
    public class BalanceteService : BaseService
    {
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly BalanceteRepository _repository;


        public BalanceteService(
            AccountPlansRepository accountPlansRepository,
            BalanceteRepository repository,



            IAppSettings appSettings) : base(appSettings)
        {
            _accountPlansRepository = accountPlansRepository;
            _repository = repository;


            _currentUserId = GetCurrentUserId();

        }
        #region Métodos

        public async Task<ResultValue> Create(InsertBalanceteDto dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // Verifica se o plano de contas já existe
                var exists = await _accountPlansRepository.ExistsAccountPlanByIdAsync(dto.AccountPlansId);

                if (exists is false)
                {
                    return ErrorResponse(Message.NotFound);
                }


                var model = new BalanceteModel
                {
                    DateMonth = (EMonth)dto.DateMonth,
                    DateYear = dto.DateYear,
                    AccountPlansId = dto.AccountPlansId,
                };

                await _repository.AddAsync(model);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> Update(int id, UpdateBalanceteDto dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // Verifica se o plano de contas já existe
                var model = await _repository.GetBalanceteById(id);

                if (model is null)
                {
                    return ErrorResponse(Message.NotFound);
                }


                model.DateMonth = (EMonth)dto.DateMonth;
                model.DateYear = dto.DateYear;

                await _repository.Update(model);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> GetBalancetes(int accountPlansId)
        {
            try
            {
                var balancetes = await _repository.GetByAccountPlanId(accountPlansId);

                if (balancetes == null || !balancetes.Any())
                    return ErrorResponse(Message.NotFound);

                var result = balancetes.Select(MapToBalanceteDto).ToList();

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
                var accountPlans = await _repository.GetById(id);

                if (accountPlans == null || !accountPlans.Any())
                    return ErrorResponse(Message.NotFound);

                var result = accountPlans.Select(MapToBalanceteDto).ToList();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        private static BalanceteDto MapToBalanceteDto(BalanceteModel x) => new()
        {
            Id = x.Id,
            DateCreate = x.DateCreate,
            DateMonth = x.DateMonth,
            DateYear = x.DateYear, 
            Status = x.Status,
            AccountPlans = new AccountPlanResponse
            {
                Id = x.AccountPlans.Id, 
            }
        };


        #endregion


    }
}