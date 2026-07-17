using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._3_Utils;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace _2___Application._1_Services.AccountPlans
{
    public class AccountPlanImportService : BaseService
    {
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly AccountPlanAccountRepository _accountPlanAccountRepository;

        public AccountPlanImportService(
            AccountPlansRepository accountPlansRepository,
            AccountPlanAccountRepository accountPlanAccountRepository,
            IAppSettings appSettings) : base(appSettings)
        {
            _accountPlansRepository = accountPlansRepository;
            _accountPlanAccountRepository = accountPlanAccountRepository;
        }

        public Task<ResultValue> UploadInitialAsync(int accountPlanId, IFormFile file)
        {
            return ProcessAsync(accountPlanId, file, AccountPlanImportOperation.InitialUpload);
        }

        public Task<ResultValue> ReplaceAsync(int accountPlanId, IFormFile file)
        {
            return ProcessAsync(accountPlanId, file, AccountPlanImportOperation.Replace);
        }

        private async Task<ResultValue> ProcessAsync(
            int accountPlanId,
            IFormFile file,
            AccountPlanImportOperation operation)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ErrorResponse("Arquivo inválido.");

                var accountPlan = await _accountPlansRepository.GetByIdSingleAsync(accountPlanId);
                if (accountPlan == null)
                    return ErrorResponse(Message.NotFound);

                var hasAccounts = await _accountPlanAccountRepository
                    .AnyByAccountPlanIdAsync(accountPlanId);

                if (operation == AccountPlanImportOperation.InitialUpload && hasAccounts &&
                    accountPlan.SourceMode == EAccountPlanSourceMode.UploadedAccountPlan)
                {
                    return ErrorResponse("O Plano de Contas já possui uma carga inicial. Utilize o endpoint de substituição.");
                }

                if (operation == AccountPlanImportOperation.Replace &&
                    accountPlan.SourceMode != EAccountPlanSourceMode.UploadedAccountPlan)
                {
                    return ErrorResponse("O Plano de Contas ainda não possui uma carga inicial para ser substituída.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".xlsx" && extension != ".csv")
                    return ErrorResponse("Formato inválido. Envie um arquivo XLSX ou CSV.");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var accounts = extension == ".xlsx"
                    ? ReadAccountPlanAccountsFromXlsx(stream, accountPlanId)
                    : ReadAccountPlanAccountsFromCsv(stream, accountPlanId);

                if (!accounts.Any())
                    return ErrorResponse("O arquivo não possui contas válidas para importação.");

                var validationError = ValidateAccountPlanAccountsImport(accounts);
                if (validationError != null)
                    return ErrorResponse(validationError);

                AccountPlanAccountSynchronizationResult synchronizationResult;

                if (operation == AccountPlanImportOperation.Replace)
                {
                    synchronizationResult = await _accountPlanAccountRepository
                        .ReplaceOfficialAccountsAsync(accountPlanId, accounts);
                }
                else
                {
                    var upsertResult = await _accountPlanAccountRepository
                        .UpsertOfficialAccountsAsync(accountPlanId, accounts);

                    synchronizationResult = new AccountPlanAccountSynchronizationResult
                    {
                        NewAccounts = upsertResult.NewAccounts,
                        UpdatedAccountsCount = upsertResult.UpdatedAccountsCount
                    };
                }

                accountPlan.SourceMode = EAccountPlanSourceMode.UploadedAccountPlan;
                await _accountPlansRepository.Update(accountPlan);

                return SuccessResponse(new ImportAccountPlanAccountsResponse
                {
                    Message = operation == AccountPlanImportOperation.Replace
                        ? "Plano de contas substituído com sucesso."
                        : "Plano de contas importado com sucesso.",
                    ImportedAccountsCount = accounts
                        .Select(x => x.CostCenter?.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count(),
                    NewAccountsCount = synchronizationResult.NewAccounts.Count,
                    UpdatedAccountsCount = synchronizationResult.UpdatedAccountsCount,
                    RemovedAccountsCount = synchronizationResult.RemovedAccounts.Count,
                    SourceMode = accountPlan.SourceMode.ToString(),
                    NewAccounts = synchronizationResult.NewAccounts
                        .Select(MapToAccountPlanAccountResponse)
                        .ToList(),
                    RemovedAccounts = synchronizationResult.RemovedAccounts
                        .Select(MapToAccountPlanAccountResponse)
                        .ToList()
                });
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
                return CsvImportTextReader.BuildReplacementCharacterError("numero da conta", costCenter);

            if (CsvImportTextReader.ContainsReplacementCharacter(name))
                return CsvImportTextReader.BuildReplacementCharacterError("descricao da conta", costCenter);

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

        private static AccountPlanAccountResponse MapToAccountPlanAccountResponse(AccountPlanAccount account) => new()
        {
            Id = account.Id,
            AccountPlanId = account.AccountPlanId,
            CostCenter = account.CostCenter,
            Name = account.Name,
            AccountPlanClassificationId = account.AccountPlanClassificationId,
            ClassificationStatus = account.Status.ToString(),
            Origin = account.Origin.ToString(),
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };

        private enum AccountPlanImportOperation
        {
            InitialUpload,
            Replace
        }
    }
}
