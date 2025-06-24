using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using CsvHelper;
using ClosedXML.Excel;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Group;
using _4_InfraData._5_ConfigEnum;

namespace _2___Application._1_Services.AccountPlans.Balancete
{
    public class BalanceteService : BaseService
    {
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly BalanceteRepository _repository;
        private readonly BalanceteDataRepository _balanceteDataRepository;


        public BalanceteService(
            AccountPlansRepository accountPlansRepository,
            BalanceteRepository repository,
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

                var model = new BalanceteModel
                {
                    DateMonth = (EMonth)dto.DateMonth,
                    DateYear = dto.DateYear,
                    AccountPlansId = dto.AccountPlansId,
                    Status = ESituationBalancete.Pending
                };

                await _repository.AddAsync(model);

                return SuccessResponse( model);
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

        public async Task<ResultValue> Delete(int id)
        {
            try
            {
                var balancete = await _repository.GetByIdDelete(id);

                if (balancete == null )
                    return ErrorResponse(Message.NotFound);

                await _repository.DeletePermanently(id);

                return SuccessResponse(Message.DeletedSuccess);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetAccountPlanWithBalancetes(int accountPlanId)
        {
            var balancetes = await _repository.GetAccountPlanWithBalancetesAsync(accountPlanId);

            if (balancetes == null || !balancetes.Any())
                return ErrorResponse("AccountPlan não encontrado ou sem balancetes.");

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
              Status = b.Status.GetDescription(),
              DateCreate = b.DateCreate
          })
          .ToList()
            };
            return SuccessResponse(response);
        }




        #region Private Balancete
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
        #endregion

        #region Balancete Data

        public async Task<ResultValue> ImportBalanceteData(IFormFile file, int balanceteId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ErrorResponse("Arquivo inválido.");

                var balancete = await _repository.GetBalanceteById(balanceteId);
                if (balancete == null)
                    return ErrorResponse(Message.NotFound);

                var list = new List<BalanceteDataModel>();
                var extension = Path.GetExtension(file.FileName).ToLower();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                if (extension == ".csv")
                {
                    list = ReadFromCsv(stream, balanceteId);
                }
                else if (extension == ".xlsx")
                {
                    list = ReadFromXlsx(stream, balanceteId);
                }
                else
                {
                    return ErrorResponse("Formato de arquivo não suportado. Envie um CSV ou XLSX.");
                }

                await _balanceteDataRepository.AddRangeAsync(list);

                return SuccessResponse("Dados importados com sucesso.");
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
                var balancete = await _balanceteDataRepository.GetByBalanceteId(balanceteId);

                if (balancete == null || !balancete.Any())
                    return ErrorResponse(Message.NotFound);

                var result = MapToBalanceteDataDto(balancete); 

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
                var data = await _balanceteDataRepository.GetAgrupadoPorCostCenter(balanceteId);

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
        private List<BalanceteDataModel> ReadFromXlsx(Stream stream, int balanceteId)
        {
            var list = new List<BalanceteDataModel>();
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var firstRowUsed = worksheet.FirstRowUsed();
            var row = firstRowUsed.RowUsed().RowBelow();

            while (!row.IsEmpty())
            {
                var costCenter = row.Cell(0).GetFormattedString().Trim();
                var name = row.Cell(1).GetFormattedString().Trim();

                // Ignorar linhas vazias ou de cabeçalho no meio
                if (string.IsNullOrWhiteSpace(costCenter) && string.IsNullOrWhiteSpace(name))
                {
                    row = row.RowBelow();
                    continue;
                }

                if (name != null && name.ToUpper().Contains("DESCRIÇÃO"))
                {
                    row = row.RowBelow();
                    continue;
                }

                var model = new BalanceteDataModel
                {
                    BalanceteId = balanceteId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimal(row.Cell(2).GetFormattedString()),
                    Debit = ParseDecimal(row.Cell(3).GetFormattedString()),
                    Credit = ParseDecimal(row.Cell(4).GetFormattedString()),
                    FinalValue = ParseDecimal(row.Cell(5).GetFormattedString()),
                    BudgetedAmount = false
                };

                list.Add(model);
                row = row.RowBelow();
            }

            return list;
        }
        private List<BalanceteDataModel> ReadFromCsv(Stream stream, int balanceteId)
        {
            var list = new List<BalanceteDataModel>();
            stream.Position = 0;

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                BadDataFound = null,
                MissingFieldFound = null,
                IgnoreBlankLines = true
            });

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var costCenter = csv.GetField(0)?.Trim();
                var name = csv.GetField(1)?.Trim();

                // Ignorar linhas vazias ou cabeçalho no meio
                if (string.IsNullOrWhiteSpace(costCenter) && string.IsNullOrWhiteSpace(name))
                    continue;

                if (name != null && name.ToUpper().Contains("DESCRIÇÃO"))
                    continue;

                var model = new BalanceteDataModel
                {
                    BalanceteId = balanceteId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimal(csv.GetField(2)),
                    Debit = ParseDecimal(csv.GetField(3)),
                    Credit = ParseDecimal(csv.GetField(4)),
                    FinalValue = ParseDecimal(csv.GetField(5)),
                    BudgetedAmount = false
                };

                list.Add(model);
            }

            return list;
        }

        private static BalanceteDataDto MapToBalanceteDataDto(List<BalanceteDataModel> data)

        {
            var first = data.First();

            return new BalanceteDataDto
            {
                Balancete = new BalanceteDto
                {
                    Id = first.Balancete.Id,
                    DateMonth = first.Balancete.DateMonth,
                    DateYear = first.Balancete.DateYear,
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