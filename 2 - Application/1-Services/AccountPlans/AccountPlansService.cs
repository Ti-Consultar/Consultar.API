using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
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

namespace _2___Application._1_Services.AccountPlans
{
    public class AccountPlansService : BaseService
    {
        private readonly AccountPlansRepository _repository;
        private readonly GroupRepository _groupRepository;
        private readonly CompanyRepository _companyRepository;
        private readonly UserRepository _userRepository;
        private readonly int _currentUserId;

        public AccountPlansService(
            AccountPlansRepository repository,
            GroupRepository groupRepository,
            CompanyRepository companyRepository,
            UserRepository userRepository,


            IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;

            _currentUserId = GetCurrentUserId();

        }
        #region Métodos

        public async Task<ResultValue> Create(InsertAccountPlan dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // Verifica se o plano de contas já existe
                var exists = await _repository.ExistsAccountPlanAsync(dto.GroupId, dto.CompanyId, dto.SubCompanyId);

                if (exists)
                {
                    return ErrorResponse(Message.ExistsAccountPlans);
                }

                var group = await _groupRepository.GetById(dto.GroupId);
                if (group == null)
                {
                    return ErrorResponse(Message.NotFound);
                }

                var model = new AccountPlansModel
                {
                    GroupId = dto.GroupId,
                    CompanyId = dto.CompanyId,
                    SubCompanyId = dto.SubCompanyId,
                };

                await _repository.AddAsync(model);       

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAccountPlans(int groupId, int? companyId, int? subCompanyId)
        {
            try
            {
                var accountPlans = await _repository.GetByFilters(groupId, companyId, subCompanyId);

                if (accountPlans == null || !accountPlans.Any())
                    return ErrorResponse(Message.NotFound);

                var result = accountPlans.Select(MapToAccountPlanDto).ToList();

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

                var result = accountPlans.Select(MapToAccountPlanDto).ToList();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        private static AccountPlanResponse MapToAccountPlanDto(AccountPlansModel x) => new()
        {
            Id = x.Id,
            Group = x.Group == null ? null : new GroupSimpleDto
            {
                Id = x.Group.Id,
                Name = x.Group.Name
            },
            Company = x.Company == null ? null : new CompanySimpleDto
            {
                Id = x.Company.Id,
                Name = x.Company.Name
            },
            SubCompany = x.SubCompany == null ? null : new SubCompanySimpleDto
            {
                Id = x.SubCompany.Id,
                Name = x.SubCompany.Name
            }
        };

        #endregion


    }
}