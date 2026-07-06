using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application._3_Utils;
using _2___Application._1_Services.Scope;
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
        private readonly BudgetDataRepository _budgetDataRepository;
        private readonly AccountPlanAccountRepository _accountPlanAccountRepository;
        private readonly IAccountPlanScopeResolver _accountPlanScopeResolver;


        public BudgetService(
            AccountPlansRepository accountPlansRepository,
            BudgetRepository repository,
            BudgetDataRepository budgetDataRepository,
            AccountPlanAccountRepository accountPlanAccountRepository,
            IAccountPlanScopeResolver accountPlanScopeResolver,



            IAppSettings appSettings) : base(appSettings)
        {
            _accountPlansRepository = accountPlansRepository;
            _repository = repository;
            _budgetDataRepository = budgetDataRepository;
            _accountPlanAccountRepository = accountPlanAccountRepository;
            _accountPlanScopeResolver = accountPlanScopeResolver;


            _currentUserId = GetCurrentUserId();

        }
        #region Métodos
        #region Balancete
        public async Task<ResultValue> Create(InsertBalanceteDto dto)
        {
            try
            {
                var user = GetCurrentUserId();

                var accountPlan = await ResolveCanonicalAccountPlanForScopeAsync(dto);

                if (accountPlan is null)
                {
                    return ErrorResponse("Plano de contas canônico do grupo não encontrado.");
                }

                var groupId = dto.GroupId ?? accountPlan.GroupId;
                var balanceteExists = await _repository.GetExistsParams(
                    accountPlan.Id,
                    groupId,
                    dto.CompanyId,
                    dto.SubCompanyId,
                    dto.DateMonth,
                    dto.DateYear);

                if (balanceteExists is true)
                {
                    return SuccessResponse(Message.ExistsBalancete);
                }

                var model = new BudgetModel
                {
                    DateMonth = (EMonth)dto.DateMonth,
                    DateYear = dto.DateYear,
                    AccountPlansId = accountPlan.Id,
                    GroupId = groupId,
                    CompanyId = dto.CompanyId,
                    SubCompanyId = dto.SubCompanyId,
                };

                await _repository.AddAsync(model);

                return SuccessResponse(MapToBalanceteDto(model));
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
                var model = await _repository.GetBudgetById(id);

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

        public async Task<ResultValue> GetAccountPlanWithBalancetesMonth(
            int accountPlanId,
            int? groupId = null,
            int? companyId = null,
            int? subCompanyId = null)
        {
            var scope = await ResolveFinancialScopeAsync(accountPlanId, groupId, companyId, subCompanyId);
            if (scope == null)
                return ErrorResponse(Message.NotFound);

            var balancetes = await _repository.GetAccountPlanWithBalancetesMonthAsync(
                scope.AccountPlanId,
                scope.GroupId,
                scope.CompanyId,
                scope.SubCompanyId);

            if (balancetes == null || !balancetes.Any())
                return SuccessResponse(new List<AccountPlanWithBalancetesDto>());

            var response = new AccountPlanWithBalancetesDto
            {
                Id = scope.AccountPlanId,
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
        private Task<AccountPlansModel?> ResolveCanonicalAccountPlanForScopeAsync(InsertBalanceteDto dto)
        {
            return ResolveCanonicalAccountPlanForScopeAsync(
                dto.AccountPlansId,
                dto.GroupId,
                dto.CompanyId,
                dto.SubCompanyId);
        }

        private async Task<AccountPlansModel?> ResolveCanonicalAccountPlanForScopeAsync(
            int accountPlansId,
            int? groupId,
            int? companyId,
            int? subCompanyId)
        {
            var scope = await ResolveFinancialScopeAsync(accountPlansId, groupId, companyId, subCompanyId);

            return scope == null
                ? null
                : await _accountPlansRepository.GetByIdSingleAsync(scope.AccountPlanId);
        }

        private async Task<FinancialScopeResolution?> ResolveFinancialScopeAsync(
            int accountPlanId,
            int? groupId,
            int? companyId,
            int? subCompanyId)
        {
            if (groupId.HasValue || companyId.HasValue || subCompanyId.HasValue)
            {
                var scopeGroupId = groupId;
                var scopeCompanyId = companyId;

                if (subCompanyId.HasValue)
                {
                    var scopedPlan = await _accountPlansRepository.GetSubCompanyAccountPlan(subCompanyId.Value);
                    if (scopedPlan == null && !scopeGroupId.HasValue)
                        return null;

                    if (scopedPlan != null)
                    {
                        scopeGroupId ??= scopedPlan.GroupId;
                        scopeCompanyId ??= scopedPlan.CompanyId;
                    }
                }
                else if (companyId.HasValue)
                {
                    var scopedPlan = await _accountPlansRepository.GetCompanyAccountPlanByCompanyId(companyId.Value);
                    if (scopedPlan == null && !scopeGroupId.HasValue)
                        return null;

                    if (scopedPlan != null)
                        scopeGroupId ??= scopedPlan.GroupId;
                }

                if (!scopeGroupId.HasValue)
                    return null;

                var canonicalAccountPlan = await _accountPlanScopeResolver.ResolveCanonicalAccountPlanAsync(scopeGroupId.Value);
                return canonicalAccountPlan == null
                    ? null
                    : new FinancialScopeResolution
                    {
                        AccountPlanId = canonicalAccountPlan.Id,
                        GroupId = scopeGroupId.Value,
                        CompanyId = scopeCompanyId,
                        SubCompanyId = subCompanyId
                    };
            }

            var legacyAccountPlan = await _accountPlansRepository.GetByIdSingleAsync(accountPlanId);
            if (legacyAccountPlan == null)
                return null;

            var canonicalLegacyAccountPlan = await _accountPlanScopeResolver
                .ResolveCanonicalAccountPlanAsync(legacyAccountPlan.GroupId);

            return canonicalLegacyAccountPlan == null
                ? null
                : new FinancialScopeResolution
                {
                    AccountPlanId = canonicalLegacyAccountPlan.Id,
                    GroupId = legacyAccountPlan.GroupId,
                    CompanyId = legacyAccountPlan.CompanyId,
                    SubCompanyId = legacyAccountPlan.SubCompanyId
                };
        }

        private class FinancialScopeResolution
        {
            public int AccountPlanId { get; set; }
            public int GroupId { get; set; }
            public int? CompanyId { get; set; }
            public int? SubCompanyId { get; set; }
        }

        private static BalanceteDto MapToBalanceteDto(BudgetModel x) => new()
        {
            Id = x.Id,
            DateCreate = x.DateCreate,
            DateMonth = x.DateMonth,
            DateYear = x.DateYear,
            GroupId = x.GroupId,
            CompanyId = x.CompanyId,
            SubCompanyId = x.SubCompanyId,
            AccountPlans = new AccountPlanResponse
            {
                Id = x.AccountPlans.Id,
            }
        };
        #endregion
        #endregion

        #region Balancete Data

        public async Task<ResultValue> ImportBalanceteData(IFormFile file, int budgetId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ErrorResponse("Arquivo inválido.");

                var budget = await _repository.GetBudgetById(budgetId);
                if (budget == null)
                    return ErrorResponse(Message.NotFound);

                var list = new List<BudgetDataModel>();
                var extension = Path.GetExtension(file.FileName).ToLower();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                if (extension == ".csv")
                {
                    list = ReadFromCsv(stream, budgetId);
                }
                else if (extension == ".xlsx")
                {
                    list = ReadFromXlsx(stream, budgetId);
                }
                else
                {
                    return ErrorResponse("Formato de arquivo não suportado. Envie um CSV ou XLSX.");
                }

                // Remover duplicados na lista importada (CostCenter único)
                list = list
                    .GroupBy(x => x.CostCenter)
                    .Select(g => g.First())
                    .ToList();

                var validationError = ValidateBudgetDataImport(list);
                if (validationError != null)
                    return ErrorResponse(validationError);

                await _accountPlanAccountRepository.UpsertFromBudgetDataAsync(budget.AccountPlansId, list);
                await _budgetDataRepository.AddRangeAsync(list);

                return SuccessResponse("Dados importados com sucesso.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> GetByBudgetIdDate(int accountplanId, int year, int month)
        {
            try
            {
                var budget = await _budgetDataRepository.GetByBalanceteIdDate(accountplanId, year, month);

                if (budget == null || !budget.Any())
                    return SuccessResponse(new BudgetDataDto());

                var result = MapToBudgetDataDto(budget);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> GetByBalanceteId(int balanceteId)
        {
            try
            {
                var balancete = await _budgetDataRepository.GetByBalanceteId(balanceteId);

                if (balancete == null || !balancete.Any())
                    return ErrorResponse(Message.NotFound);

                var result = MapToBudgetDataDto(balancete);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAgrupadoPorCostCenter(int balanceteId)
        {
            try
            {
                var data = await _budgetDataRepository.GetAgrupadoPorCostCenter(balanceteId);

                if (data == null || !data.Any())
                    return SuccessResponse(Message.NotFound);

                var result = data.Select(x => new DataDto
                {
                    Id = x.Id,
                    CostCenter = x.CostCenter,
                    Name = x.Name,
                    InitialValue = x.InitialValue,
                    Credit = x.Credit,
                    Debit = x.Debit,
                    FinalValue = x.FinalValue,
                    BudgetedAmount = x.BudgetedAmount,
                }).ToList();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAgrupadoByCostCenter(int balanceteId, string? search)
        {
            try
            {


                var data = await _budgetDataRepository.GetByBalanceteDataByCostCenter(balanceteId, search);

                if (data == null || !data.Any())
                    return SuccessResponse(Message.NotFound);

                var result = data.Select(x => new DataDto
                {
                    Id = x.Id,
                    CostCenter = x.CostCenter,
                    Name = x.Name,
                    InitialValue = x.InitialValue,
                    Credit = x.Credit,
                    Debit = x.Debit,
                    FinalValue = x.FinalValue,
                    BudgetedAmount = x.BudgetedAmount,
                }).ToList();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAgrupadoSomenteAtivos(int balanceteId)
        {
            try
            {
                var data = await _budgetDataRepository.GetByBalanceteId(balanceteId);

                if (data == null || !data.Any())
                    return SuccessResponse(Message.NotFound);

                var lookup = data.ToDictionary(x => x.CostCenter, x => new DataDto
                {
                    Id = x.Id,
                    CostCenter = x.CostCenter,
                    Name = x.Name,
                    InitialValue = x.InitialValue,
                    Credit = x.Credit,
                    Debit = x.Debit,
                    FinalValue = x.FinalValue,
                    BudgetedAmount = x.BudgetedAmount
                });

                foreach (var item in data)
                {
                    var parts = item.CostCenter.Split('.');
                    if (parts.Length <= 1) continue;

                    var parentCostCenter = string.Join('.', parts.Take(parts.Length - 1));

                    if (lookup.TryGetValue(parentCostCenter, out var parent))
                    {
                        parent.InitialValue += item.InitialValue;
                        parent.Credit += item.Credit;
                        parent.Debit += item.Debit;
                        parent.FinalValue += item.FinalValue;
                    }
                }

                var pais = lookup.Values
                    .Where(x => x.CostCenter.StartsWith("1")) // Apenas ativos
                    .Where(x => data.Any(d => d.CostCenter.StartsWith(x.CostCenter + "."))) // tem filhos
                    .OrderBy(x => x.CostCenter)
                    .ToList();

                return SuccessResponse(pais);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> GetAgrupadoPorTipo(int balanceteId, char tipoInicial)
        {
            try
            {
                var data = await _budgetDataRepository.GetByBalanceteId(balanceteId);

                if (data == null || !data.Any())
                    return SuccessResponse(Message.NotFound);

                var lookup = data.ToDictionary(x => x.CostCenter, x => new DataDto
                {
                    Id = x.Id,
                    CostCenter = x.CostCenter,
                    Name = x.Name,
                    InitialValue = x.InitialValue,
                    Credit = x.Credit,
                    Debit = x.Debit,
                    FinalValue = x.FinalValue,
                    BudgetedAmount = x.BudgetedAmount
                });

                foreach (var item in data)
                {
                    var parts = item.CostCenter.Split('.');
                    if (parts.Length <= 1) continue;

                    var parentCostCenter = string.Join('.', parts.Take(parts.Length - 1));

                    if (lookup.TryGetValue(parentCostCenter, out var parent))
                    {
                        parent.InitialValue += item.InitialValue;
                        parent.Credit += item.Credit;
                        parent.Debit += item.Debit;
                        parent.FinalValue += item.FinalValue;
                    }
                }

                var tipo = tipoInicial.ToString();

                var filtrados = lookup.Values
                    .Where(x => x.CostCenter.StartsWith(tipo)) // Começa com tipo + ponto (ex: 1.1, 1.2)
                    .OrderBy(x => x.CostCenter)
                    .ToList();

                return SuccessResponse(filtrados);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }





        public async Task<ResultValue> DeleteBalanceteData(int balanceteId)
        {
            try
            {
                var balancete = await _repository.GetByIdDelete(balanceteId);

                if (balancete == null)
                    return ErrorResponse(Message.NotFound);

                await _repository.DeleteBalanceteData(balanceteId);

                return SuccessResponse(Message.DeletedSuccess);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        #region Private
        private List<BudgetDataModel> ReadFromXlsxVersaoAntiga(Stream stream, int budgetId)
        {
            var list = new List<BudgetDataModel>();
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var firstRowUsed = worksheet.FirstRowUsed();
            var row = firstRowUsed.RowUsed().RowBelow();

            int emptyRowCount = 0; // contador de linhas vazias consecutivas

            while (true)
            {
                // Se chegou ao fim da planilha, para
                if (row == null)
                    break;

                if (row.IsEmpty())
                {
                    emptyRowCount++;

                    // se encontrou 3 linhas vazias seguidas, considera fim do arquivo
                    if (emptyRowCount >= 3)
                        break;

                    row = row.RowBelow();
                    continue;
                }

                emptyRowCount = 0; // reset se linha válida

                var costCenter = row.Cell(1).GetFormattedString().Trim(); // Coluna A
                var name = row.Cell(2).GetFormattedString().Trim();       // Coluna B

                // Ignorar cabeçalhos no meio
                if (string.IsNullOrWhiteSpace(costCenter) && string.IsNullOrWhiteSpace(name))
                {
                    row = row.RowBelow();
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(name) && name.ToUpper().Contains("DESCRIÇÃO"))
                {
                    row = row.RowBelow();
                    continue;
                }

                var model = new BudgetDataModel
                {
                    Id = budgetId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimal(row.Cell(3).GetFormattedString()), // Coluna C
                    Debit = ParseDecimal(row.Cell(4).GetFormattedString()),        // Coluna D
                    Credit = ParseDecimal(row.Cell(5).GetFormattedString()),       // Coluna E
                    FinalValue = ParseDecimal(row.Cell(6).GetFormattedString()),   // Coluna F
                    BudgetedAmount = true
                };

                list.Add(model);
                row = row.RowBelow();
            }

            return list;
        }


        private List<BudgetDataModel> ReadFromCsv(Stream stream, int budgetId)
        {
            var list = new List<BudgetDataModel>();
            stream.Position = 0;

            using var reader = CsvImportTextReader.CreateReader(stream);
            var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                BadDataFound = null,
                MissingFieldFound = null,
                IgnoreBlankLines = false // ⚠️ vamos controlar manualmente
            });

            csv.Read();
            csv.ReadHeader();

            int emptyRowCount = 0;

            while (csv.Read())
            {
                var costCenter = csv.GetField(0)?.Trim();
                var name = csv.GetField(1)?.Trim();

                // Se linha for totalmente vazia
                if (string.IsNullOrWhiteSpace(costCenter) && string.IsNullOrWhiteSpace(name))
                {
                    emptyRowCount++;

                    if (emptyRowCount >= 3) // 3 linhas vazias seguidas → fim
                        break;

                    continue;
                }

                emptyRowCount = 0; // reseta contador se linha válida

                // Ignorar cabeçalho no meio do arquivo
                if (!string.IsNullOrWhiteSpace(name) && name.ToUpper().Contains("DESCRIÇÃO"))
                    continue;

                var model = new BudgetDataModel
                {
                    BudgetId = budgetId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimal(csv.GetField(2)),
                    Debit = ParseDecimal(csv.GetField(3)),
                    Credit = ParseDecimal(csv.GetField(4)),
                    FinalValue = ParseDecimal(csv.GetField(5)),
                    BudgetedAmount = true
                };

                list.Add(model);
            }

            return list;
        }

        private static string? ValidateBudgetDataImport(List<BudgetDataModel> list)
        {
            var invalidCostCenter = list.FirstOrDefault(x =>
                CsvImportTextReader.ContainsReplacementCharacter(x.CostCenter));

            if (invalidCostCenter != null)
            {
                return CsvImportTextReader.BuildReplacementCharacterError(
                    "centro de custo",
                    invalidCostCenter.CostCenter);
            }

            var invalidName = list.FirstOrDefault(x =>
                CsvImportTextReader.ContainsReplacementCharacter(x.Name));

            if (invalidName != null)
            {
                return CsvImportTextReader.BuildReplacementCharacterError(
                    "descricao",
                    invalidName.CostCenter);
            }

            return null;
        }

        private List<BudgetDataModel> ReadFromXlsx(Stream stream,int budgetId)
        {
            var list = new List<BudgetDataModel>();
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            int rowNumber = 2;
            var row = worksheet.Row(rowNumber);

            int emptyRowCount = 0;

            while (true)
            {
                // encerra após 3 linhas vazias seguidas
                if (row.IsEmpty())
                {
                    emptyRowCount++;
                    if (emptyRowCount >= 3)
                        break;

                    row = row.RowBelow();
                    continue;
                }

                emptyRowCount = 0;

                var costCenter = row.Cell(1).GetString().Trim();
                var name = row.Cell(2).GetString().Trim();

                if (string.IsNullOrWhiteSpace(costCenter) &&
                    string.IsNullOrWhiteSpace(name))
                {
                    row = row.RowBelow();
                    continue;
                }

                var model = new BudgetDataModel
                {
                    BudgetId = budgetId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = GetDecimalFromCellRaw(row.Cell(3)),
                    Debit = GetDecimalFromCellRaw(row.Cell(4)),
                    Credit = GetDecimalFromCellRaw(row.Cell(5)),
                    FinalValue = GetDecimalFromCellRaw(row.Cell(6)),
                    BudgetedAmount = false
                };

                list.Add(model);
                row = row.RowBelow();
            }

            return list;
        }
        private decimal GetDecimalFromCellRaw(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return 0m;

            try
            {
                // 1️⃣ Se o Excel entende como número → pega direto
                if (cell.DataType == XLDataType.Number)
                    return cell.GetValue<decimal>();

                // 2️⃣ Se vier como texto → força leitura do texto bruto
                var raw = cell.GetString();

                if (string.IsNullOrWhiteSpace(raw))
                    return 0m;

                // remove tudo que não é número, vírgula, ponto ou sinal
                raw = raw.Trim();
                raw = raw.Replace(".", "");
                raw = raw.Replace(",", ".");

                if (decimal.TryParse(raw, CultureInfo.InvariantCulture, out var value))
                    return value;

                return 0m;
            }
            catch
            {
                return 0m;
            }
        }
        private static BudgetDataDto MapToBudgetDataDto(List<BudgetDataModel> data)

        {
            var first = data.First();

            return new BudgetDataDto
            {
                Budget = new BudgetDto
                {
                    Id = first.Budget.Id,
                    DateMonth = first.Budget.DateMonth,
                    DateYear = first.Budget.DateYear,
                },
                DataDto = data.Select(x => new DataDto
                {
                    Id = x.Id,
                    CostCenter = x.CostCenter,
                    Name = x.Name,
                    InitialValue = x.InitialValue,
                    Credit = x.Credit,
                    Debit = x.Debit,
                    FinalValue = x.FinalValue,
                    BudgetedAmount = x.BudgetedAmount
                }).ToList()
            };
        }


        #endregion

        #endregion
        #endregion



    }
}
