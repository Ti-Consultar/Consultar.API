using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2___Application._1_Services.Budget
{
    public class BudgetService : BaseService
    {
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly BudgetRepository _repository;
        private readonly BalanceteDataRepository _balanceteDataRepository;


        public BudgetService(
            AccountPlansRepository accountPlansRepository,
            BudgetRepository repository,
            BalanceteDataRepository balanceteDataRepository,



            IAppSettings appSettings) : base(appSettings)
        {
            _accountPlansRepository = accountPlansRepository;
            _repository = repository;
            _balanceteDataRepository = balanceteDataRepository;


            _currentUserId = GetCurrentUserId();

        }
        #region Métodos
        #region Balancete
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

                var balanceteExists = await _repository.GetExistsParams(dto.AccountPlansId, dto.DateMonth, dto.DateYear);

                if (balanceteExists is true)
                {
                    return SuccessResponse(Message.ExistsBalancete);
                }

                var model = new BudgetModel
                {
                    DateMonth = (EMonth)dto.DateMonth,
                    DateYear = dto.DateYear,
                    AccountPlansId = dto.AccountPlansId,
                };

                await _repository.AddAsync(model);

                return SuccessResponse(model);
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

        public async Task<ResultValue> GetByDate(int accountplanId, int year, int month)
        {
            try
            {
                var accountPlans = await _repository.GetByDate(accountplanId, year, month);

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

        public async Task<ResultValue> Delete(int id)
        {
            try
            {
                var balancete = await _repository.GetByIdDelete(id);

                if (balancete == null)
                    return ErrorResponse(Message.NotFound);

                await _repository.DeletePermanently(id);

                return SuccessResponse(Message.DeletedSuccess);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAccountPlanWithBalancetesMonth(int accountPlanId)
        {

            var balancetes = await _repository.GetAccountPlanWithBalancetesMonthAsync(accountPlanId);

            if (balancetes == null || !balancetes.Any())
                return SuccessResponse(new List<AccountPlanWithBalancetesDto>());

            var response = new AccountPlanWithBalancetesDto
            {
                Id = accountPlanId,
                Balancetes = balancetes
                    .OrderByDescending(b => b.DateYear)
                    .ThenByDescending(b => b.DateMonth)
                    .Select(b => new BalanceteSimpleDto
                    {
                        Id = b.Id,
                        DateMonth = b.DateMonth.GetDescription(),
                        DateYear = b.DateYear,
                        DateCreate = b.DateCreate

                    })
                    .ToList()
            };

            return SuccessResponse(response);
        }



       




        #region Private Balancete
        private static BalanceteDto MapToBalanceteDto(BudgetModel x) => new()
        {
            Id = x.Id,
            DateCreate = x.DateCreate,
            DateMonth = x.DateMonth,
            DateYear = x.DateYear,
            AccountPlans = new AccountPlanResponse
            {
                Id = x.AccountPlans.Id,
            }
        };
        #endregion
        #endregion


        #endregion



    }
}