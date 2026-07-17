using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Group;
using _2___Application._3_Utils;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using Microsoft.AspNetCore.Http;
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
        private readonly AccountPlanAccountRepository _accountPlanAccountRepository;
        private readonly AccountPlanImportService _accountPlanImportService;
        private readonly int _currentUserId;

        public AccountPlansService(
            AccountPlansRepository repository,
            GroupRepository groupRepository,
            CompanyRepository companyRepository,
            UserRepository userRepository,
            AccountPlanAccountRepository accountPlanAccountRepository,
            AccountPlanImportService accountPlanImportService,


            IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _accountPlanAccountRepository = accountPlanAccountRepository;
            _accountPlanImportService = accountPlanImportService;

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

        public async Task<bool> DeleteAccountsAndClassificationsAsync(int accountPlanId)
        {
            return await _accountPlanAccountRepository
                .DeleteAccountsAndClassificationsAsync(accountPlanId);
        }

        public async Task<ResultValue> ImportAccountsFromExcel(int accountPlanId, IFormFile file)
        {
            return await _accountPlanImportService.UploadInitialAsync(accountPlanId, file);
        }

        public async Task<ResultValue> ReplaceAccountsFromExcel(int accountPlanId, IFormFile file)
        {
            return await _accountPlanImportService.ReplaceAsync(accountPlanId, file);
        }

        public async Task<ResultValue> GetAccounts(int accountPlanId)
        {
            try
            {
                var accountPlan = await _repository.GetByIdSingleAsync(accountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                if (accountPlan.SourceMode == EAccountPlanSourceMode.LegacyFromBalancete)
                    await _accountPlanAccountRepository.EnsureFromBalanceteDataAsync(accountPlanId);

                var accounts = await _accountPlanAccountRepository.GetByAccountPlanIdAsync(accountPlanId);

                return SuccessResponse(accounts.Select(MapToAccountPlanAccountResponse).ToList());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAccountsPaginated(
            int accountPlanId,
            int skip = 0,
            int take = 10,
            string? search = null)
        {
            try
            {
                if (skip < 0)
                    return ErrorResponse("O parâmetro skip não pode ser negativo.");

                if (take <= 0)
                    return ErrorResponse("O parâmetro take deve ser maior que zero.");

                var accountPlan = await _repository.GetByaccountPlanId(accountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                if (accountPlan.SourceMode == EAccountPlanSourceMode.LegacyFromBalancete)
                    await _accountPlanAccountRepository.EnsureFromBalanceteDataAsync(accountPlanId);

                var paginatedAccounts = await _accountPlanAccountRepository
                    .GetPaginatedByAccountPlanIdAsync(accountPlanId, skip, take, search);
                var pendingCount = await _accountPlanAccountRepository
                    .CountPendingByAccountPlanIdAsync(accountPlanId);

                return SuccessResponse(new AccountPlanAccountListResponse
                {
                    AccountPlanId = accountPlan.Id,
                    Name = accountPlan.SubCompany?.Name
                        ?? accountPlan.Company?.Name
                        ?? accountPlan.Group?.Name
                        ?? string.Empty,
                    SourceMode = accountPlan.SourceMode.ToString(),
                    TotalCount = paginatedAccounts.TotalCount,
                    Skip = skip,
                    Take = take,
                    HasPendingClassifications = pendingCount > 0,
                    PendingClassificationsCount = pendingCount,
                    Accounts = paginatedAccounts.Items
                        .Select(MapToAccountPlanAccountListItemResponse)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetPendingAccounts(int accountPlanId)
        {
            try
            {
                var accountPlan = await _repository.GetByIdSingleAsync(accountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                if (accountPlan.SourceMode == EAccountPlanSourceMode.LegacyFromBalancete)
                    await _accountPlanAccountRepository.EnsureFromBalanceteDataAsync(accountPlanId);

                var accounts = await _accountPlanAccountRepository.GetPendingByAccountPlanIdAsync(accountPlanId);

                return SuccessResponse(new
                {
                    HasPendingClassifications = accounts.Any(),
                    PendingClassificationsCount = accounts.Count,
                    Accounts = accounts.Select(MapToAccountPlanAccountResponse).ToList()
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> UpdateSourceMode(int accountPlanId, UpdateAccountPlanSourceModeDto dto)
        {
            try
            {
                var accountPlan = await _repository.GetByIdSingleAsync(accountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                if (!Enum.IsDefined(typeof(EAccountPlanSourceMode), dto.SourceMode))
                    return ErrorResponse("Modo de origem inválido.");

                accountPlan.SourceMode = (EAccountPlanSourceMode)dto.SourceMode;
                await _repository.Update(accountPlan);

                return SuccessResponse(new
                {
                    AccountPlanId = accountPlan.Id,
                    SourceMode = accountPlan.SourceMode.ToString()
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> CreateAccount(int accountPlanId, CreateAccountPlanAccountDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.CostCenter))
                    return ErrorResponse("Número da conta é obrigatório.");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return ErrorResponse("Descrição da conta é obrigatória.");

                var validationError = ValidateAccountPlanAccountText(dto.CostCenter, dto.Name);
                if (validationError != null)
                    return ErrorResponse(validationError);

                var exists = await _repository.ExistsAccountPlanByIdAsync(accountPlanId);
                if (!exists)
                    return ErrorResponse(Message.NotFound);

                var upsertResult = await _accountPlanAccountRepository.UpsertOfficialAccountsAsync(
                    accountPlanId,
                    new List<AccountPlanAccount>
                    {
                        new AccountPlanAccount
                        {
                            AccountPlanId = accountPlanId,
                            CostCenter = dto.CostCenter,
                            Name = dto.Name,
                            Origin = EAccountPlanAccountOrigin.Manual
                        }
                    });

                var account = upsertResult.NewAccounts.FirstOrDefault()
                    ?? await _accountPlanAccountRepository.GetByAccountPlanAndCostCenterAsync(accountPlanId, dto.CostCenter);

                return SuccessResponse(MapToAccountPlanAccountResponse(account));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        private static string? ValidateAccountPlanAccountText(string? costCenter, string? name)
        {
            if (CsvImportTextReader.ContainsReplacementCharacter(costCenter))
            {
                return CsvImportTextReader.BuildReplacementCharacterError("numero da conta", costCenter);
            }

            if (CsvImportTextReader.ContainsReplacementCharacter(name))
            {
                return CsvImportTextReader.BuildReplacementCharacterError("descricao da conta", costCenter);
            }

            return null;
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
            },
            SourceMode = x.SourceMode.ToString()
        };

        private static AccountPlanAccountResponse MapToAccountPlanAccountResponse(AccountPlanAccount x) => new()
        {
            Id = x.Id,
            AccountPlanId = x.AccountPlanId,
            CostCenter = x.CostCenter,
            Name = x.Name,
            AccountPlanClassificationId = x.AccountPlanClassificationId,
            ClassificationStatus = x.Status.ToString(),
            Origin = x.Origin.ToString(),
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

        private static AccountPlanAccountListItemResponse MapToAccountPlanAccountListItemResponse(AccountPlanAccount x) => new()
        {
            Id = x.Id,
            AccountPlanId = x.AccountPlanId,
            CostCenter = x.CostCenter,
            Name = x.Name,
            AccountPlanClassificationId = x.AccountPlanClassificationId,
            AccountPlanClassificationName = x.AccountPlanClassification?.Name,
            AccountPlanClassificationType = x.AccountPlanClassification?.TypeClassification.ToString(),
            ClassificationStatus = x.Status.ToString(),
            Origin = x.Origin.ToString(),
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

        #endregion


    }
}
