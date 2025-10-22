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
        private readonly BudgetDataRepository _budgetDataRepository;


        public BudgetService(
            AccountPlansRepository accountPlansRepository,
            BudgetRepository repository,
            BudgetDataRepository budgetDataRepository,



            IAppSettings appSettings) : base(appSettings)
        {
            _accountPlansRepository = accountPlansRepository;
            _repository = repository;
            _budgetDataRepository = budgetDataRepository;


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
        private List<BudgetDataModel> ReadFromXlsx(Stream stream, int budgetId)
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

            using var reader = new StreamReader(stream, Encoding.UTF8);
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