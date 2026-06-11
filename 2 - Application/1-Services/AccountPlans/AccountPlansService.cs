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
using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly int _currentUserId;

        public AccountPlansService(
            AccountPlansRepository repository,
            GroupRepository groupRepository,
            CompanyRepository companyRepository,
            UserRepository userRepository,
            AccountPlanAccountRepository accountPlanAccountRepository,


            IAppSettings appSettings) : base(appSettings)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _accountPlanAccountRepository = accountPlanAccountRepository;

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

        public async Task<ResultValue> ImportAccountsFromExcel(int accountPlanId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ErrorResponse("Arquivo inválido.");

                var accountPlan = await _repository.GetByIdSingleAsync(accountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".xlsx" && extension != ".csv")
                    return ErrorResponse("Formato inválido. Envie um arquivo XLSX ou CSV.");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var accounts = extension == ".xlsx"
                    ? ReadAccountPlanAccountsFromXlsx(stream, accountPlanId)
                    : ReadAccountPlanAccountsFromCsv(stream, accountPlanId);

                var validationError = ValidateAccountPlanAccountsImport(accounts);
                if (validationError != null)
                    return ErrorResponse(validationError);

                var upsertResult = await _accountPlanAccountRepository
                    .UpsertOfficialAccountsAsync(accountPlanId, accounts);

                accountPlan.SourceMode = EAccountPlanSourceMode.UploadedAccountPlan;
                await _repository.Update(accountPlan);

                return SuccessResponse(new ImportAccountPlanAccountsResponse
                {
                    Message = "Plano de contas importado com sucesso.",
                    ImportedAccountsCount = accounts
                        .Select(x => x.CostCenter?.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count(),
                    NewAccountsCount = upsertResult.NewAccounts.Count,
                    UpdatedAccountsCount = upsertResult.UpdatedAccountsCount,
                    SourceMode = accountPlan.SourceMode.ToString(),
                    NewAccounts = upsertResult.NewAccounts.Select(MapToAccountPlanAccountResponse).ToList()
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
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

        private static List<AccountPlanAccount> ReadAccountPlanAccountsFromXlsx(Stream stream, int accountPlanId)
        {
            var accounts = new List<AccountPlanAccount>();
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var row = worksheet.FirstRowUsed();

            while (row != null && !row.IsEmpty())
            {
                AddAccountPlanAccountIfValid(
                    accounts,
                    accountPlanId,
                    row.Cell(1).GetFormattedString(),
                    row.Cell(2).GetFormattedString());

                row = row.RowBelow();
            }

            return accounts;
        }

        private static List<AccountPlanAccount> ReadAccountPlanAccountsFromCsv(Stream stream, int accountPlanId)
        {
            var accounts = new List<AccountPlanAccount>();
            stream.Position = 0;

            using var delimiterReader = CsvImportTextReader.CreateReader(stream);
            var firstLine = delimiterReader.ReadLine() ?? string.Empty;
            var delimiter = firstLine.Count(c => c == ';') >= firstLine.Count(c => c == ',') ? ";" : ",";

            using var reader = CsvImportTextReader.CreateReader(stream);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = delimiter,
                BadDataFound = null,
                MissingFieldFound = null
            });

            while (csv.Read())
            {
                AddAccountPlanAccountIfValid(
                    accounts,
                    accountPlanId,
                    csv.GetField(0),
                    csv.GetField(1));
            }

            return accounts;
        }

        private static string? ValidateAccountPlanAccountsImport(List<AccountPlanAccount> accounts)
        {
            foreach (var account in accounts)
            {
                var validationError = ValidateAccountPlanAccountText(account.CostCenter, account.Name);
                if (validationError != null)
                    return validationError;
            }

            return null;
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

        private static void AddAccountPlanAccountIfValid(
            List<AccountPlanAccount> accounts,
            int accountPlanId,
            string? costCenter,
            string? name)
        {
            costCenter = costCenter?.Trim();
            name = name?.Trim();

            if (string.IsNullOrWhiteSpace(costCenter) ||
                string.IsNullOrWhiteSpace(name) ||
                costCenter.Contains("conta", StringComparison.OrdinalIgnoreCase))
                return;

            accounts.Add(new AccountPlanAccount
            {
                AccountPlanId = accountPlanId,
                CostCenter = costCenter,
                Name = name,
                Origin = EAccountPlanAccountOrigin.ExcelUpload
            });
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

        #endregion


    }
}
