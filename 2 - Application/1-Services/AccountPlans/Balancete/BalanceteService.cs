using _2___Application._2_Dto_s.AccountPlan;
using _2___Application._2_Dto_s.AccountPlan.Balancete;
using _2___Application._2_Dto_s.Company;
using _2___Application._2_Dto_s.Company.SubCompany;
using _2___Application._2_Dto_s.Group;
using _2___Application.Base;
using _3_Domain._1_Entities;
using _3_Domain._2_Enum_s;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._5_ConfigEnum;
using ClosedXML.Excel;
using CsvHelper;
using DocumentFormat.OpenXml.Office.Word;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.WebEncoders.Testing;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static _2___Application._1_Services.AccountPlans.Balancete.BalanceteService;

namespace _2___Application._1_Services.AccountPlans.Balancete
{
    public class BalanceteService : BaseService
    {
        private readonly AccountPlansRepository _accountPlansRepository;
        private readonly BalanceteRepository _repository;
        private readonly BalanceteDataRepository _balanceteDataRepository;
        private readonly BalanceteImportConfigRepository _importConfigRepo;


        public BalanceteService(
            AccountPlansRepository accountPlansRepository,
            BalanceteRepository repository,
            BalanceteDataRepository balanceteDataRepository,
            BalanceteImportConfigRepository importConfigRepo,



            IAppSettings appSettings) : base(appSettings)
        {
            _accountPlansRepository = accountPlansRepository;
            _repository = repository;
            _balanceteDataRepository = balanceteDataRepository;
            _importConfigRepo = importConfigRepo;


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
                        Status = b.Status.GetDescription(),
                        DateCreate = b.DateCreate

                    })
                    .ToList()
            };

            return SuccessResponse(response);
        }



        public async Task<ResultValue> GetAccountPlanWithBalancetes(int accountPlanId, char tipo)
        {
            if (tipo == 0)
            {
                tipo = '1';
            }

            var balancetes = await _repository.GetAccountPlanWithBalancetesAsync(accountPlanId);

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
                        Status = b.Status.GetDescription(),
                        DateCreate = b.DateCreate,
                        BalanceteData = new List<BalanceteDataDtoSimple>
                        {
                    new BalanceteDataDtoSimple
                    {
                        DataDto = AgruparBalanceteData(b.BalancetesData.ToList(), tipo) // muda pra '1' se quiser ativos
                    }
                        }
                    })
                    .ToList()
            };

            return SuccessResponse(response);
        }


        private List<DataDto> AgruparBalanceteData(List<BalanceteDataModel> data, char tipoInicial)
        {
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

            // ⬇️ Aqui muda: agora retorna todos que comecem com o dígito selecionado (ex: '1' para Ativo)
            var todosComPrefixo = lookup.Values
                .Where(x => x.CostCenter.StartsWith(tipoInicial.ToString()))
                .OrderBy(x => x.CostCenter)
                .ToList();

            return todosComPrefixo;
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

                // Remover duplicados na lista importada (CostCenter único)
                list = list
                    .GroupBy(x => x.CostCenter)
                    .Select(g => g.First())
                    .ToList();


                await _balanceteDataRepository.AddRangeAsync(list);

                return SuccessResponse("Dados importados com sucesso.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> ImportBalanceteDataDinamic(int balanceteId, BalanceteColumnMap dto)
        {
            try
            {
                if (dto.File == null || dto.File.Length == 0)
                    return ErrorResponse("Arquivo inválido.");

                var balancete = await _repository.GetBalanceteById(balanceteId);
                if (balancete == null)
                    return ErrorResponse(Message.NotFound);

                // 🔥 1. Ver se já existe configuração salva
                var savedConfig = await _importConfigRepo.GetByAccountPlanIdAsync(balancete.AccountPlansId);

                BalanceteColumnMap mapToUse = dto;

                if (savedConfig != null)
                {


                    // Monta map a partir do salvo
                    mapToUse = new BalanceteColumnMap
                    {
                        StartRow = savedConfig.StartRow,
                        CostCenter = savedConfig.CostCenterCol,
                        Name = savedConfig.NameCol,
                        InitialValue = savedConfig.InitialValueCol,
                        Debit = savedConfig.DebitCol,
                        Credit = savedConfig.CreditCol,
                        FinalValue = savedConfig.FinalValueCol,
                        File = dto.File
                    };
                }
                else
                {
                    // 🔥 1ª vez → salvar configuração
                    if (savedConfig == null)
                    {
                        await _importConfigRepo.AddAsync(new BalanceteImportConfig
                        {
                            AccountPlanId = balancete.AccountPlansId,

                            StartRow = dto.StartRow,
                            CostCenterCol = dto.CostCenter,
                            NameCol = dto.Name,
                            InitialValueCol = dto.InitialValue,
                            DebitCol = dto.Debit,
                            CreditCol = dto.Credit,
                            FinalValueCol = dto.FinalValue,
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                // 🔥 2. Processa igual antes
                var extension = Path.GetExtension(dto.File.FileName).ToLower();
                var list = new List<BalanceteDataModel>();

                using var stream = new MemoryStream();
                await dto.File.CopyToAsync(stream);

                if (extension == ".csv")
                    list = ReadFromCsvDinamic(stream, balanceteId, mapToUse);
                else if (extension == ".xlsx")
                    list = ReadFromXlsxDinamic(stream, balanceteId, mapToUse);
                else
                    return ErrorResponse("Formato inválido.");

                list = list
                    .GroupBy(x => x.CostCenter)
                    .Select(g => g.First())
                    .ToList();

                await _balanceteDataRepository.AddRangeAsync(list);

                return SuccessResponse("Dados importados com sucesso.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        public async Task<ResultValue> CreateConfigBalanceteImport(InsertBalanceteImportConfig dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // Verifica se o plano de contas já existe
                var exists = await _importConfigRepo.ExistsAccountPlanAsync(dto.AccountPlanId);

                if (exists)
                {
                    return ErrorResponse(Message.ExistsAccountPlans);
                }


                var model = new BalanceteImportConfig
                {
                    AccountPlanId = dto.AccountPlanId,
                    StartRow = dto.StartRow,
                    CostCenterCol = dto.CostCenterCol,
                    NameCol = dto.NameCol,
                    InitialValueCol = dto.InitialValueCol,
                    DebitCol = dto.DebitCol,
                    CreditCol = dto.CreditCol,
                    FinalValueCol = dto.FinalValueCol,
                    CreatedAt = DateTime.Now
                };

                await _importConfigRepo.AddAsync(model);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> UpdateConfigBalanceteImport(UpdateBalanceteImportConfig dto)
        {
            try
            {
                var user = GetCurrentUserId();

                // 1. Busca configuração existente
                var existingConfig = await _importConfigRepo
                    .GetByAccountPlanIdAsync(dto.AccountPlanId);

                if (existingConfig == null)
                {
                    return ErrorResponse(Message.NotFound);
                }

                // 2. Remove configuração atual
                await _importConfigRepo.DeletePermanently(existingConfig.Id);

                // 3. Cria novo modelo
                var model = new BalanceteImportConfig
                {
                    AccountPlanId = dto.AccountPlanId,
                    StartRow = dto.StartRow,
                    CostCenterCol = dto.CostCenterCol,
                    NameCol = dto.NameCol,
                    InitialValueCol = dto.InitialValueCol,
                    DebitCol = dto.DebitCol,
                    CreditCol = dto.CreditCol,
                    FinalValueCol = dto.FinalValueCol,
                    CreatedAt = DateTime.Now
                };

                await _importConfigRepo.AddAsync(model);

                return SuccessResponse(Message.Success);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetConfigBalanceteImportByAccountPlanId(int accountPlanId)
        {
            try
            {
                var user = GetCurrentUserId();

                var config = await _importConfigRepo
                    .GetByAccountPlanIdAsync(accountPlanId);

                if (config == null)
                {
                    return ErrorResponse(Message.NotFound);
                }

                var result = new
                {
                    config.Id,
                    config.AccountPlanId,
                    config.StartRow,
                    config.CostCenterCol,
                    config.NameCol,
                    config.InitialValueCol,
                    config.DebitCol,
                    config.CreditCol,
                    config.FinalValueCol
                };

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        public async Task<ResultValue> ExistsConfigBalanceteImport(int accountPlanId)
        {
            try
            {
                var user = GetCurrentUserId();

                var exists = await _importConfigRepo
                    .ExistsAccountPlanAsync(accountPlanId);

                return SuccessResponse(exists);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        public async Task<ResultValue> GetByBalanceteIdDate(int accountplanId, int year, int month)
        {
            try
            {
                var balancete = await _balanceteDataRepository.GetByBalanceteIdDate(accountplanId, year, month);

                if (balancete == null || !balancete.Any())
                    return SuccessResponse(new BalanceteDataDto());

                var result = MapToBalanceteDataDto(balancete);

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

        public async Task<ResultValue> GetAgrupadoByCostCenter(int balanceteId, string? search)
        {
            try
            {


                var data = await _balanceteDataRepository.GetByBalanceteDataByCostCenter(balanceteId, search);

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
                var data = await _balanceteDataRepository.GetByBalanceteId(balanceteId);

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
                var data = await _balanceteDataRepository.GetByBalanceteId(balanceteId);

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
        private List<BalanceteDataModel> ReadFromXlsx(Stream stream, int balanceteId)
        {
            var list = new List<BalanceteDataModel>();
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

                var model = new BalanceteDataModel
                {
                    BalanceteId = balanceteId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimal(row.Cell(3).GetFormattedString()), // Coluna C
                    Debit = ParseDecimal(row.Cell(4).GetFormattedString()),        // Coluna D
                    Credit = ParseDecimal(row.Cell(5).GetFormattedString()),       // Coluna E
                    FinalValue = ParseDecimal(row.Cell(6).GetFormattedString()),   // Coluna F
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

        private List<BalanceteDataModel> ReadFromXlsxDinamic(Stream stream, int balanceteId, BalanceteColumnMap map)
        {
            var list = new List<BalanceteDataModel>();
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            int rowNumber = map.StartRow;
            var row = worksheet.Row(rowNumber);

            int emptyRowCount = 0; // contador de linhas vazias seguidas

            while (true)
            {
                // 🔹 Se linha inteira está vazia
                if (row.IsEmpty())
                {
                    emptyRowCount++;

                    // 🔥 se encontrou 3 linhas vazias seguidas, é o fim real do arquivo
                    if (emptyRowCount >= 3)
                        break;

                    row = row.RowBelow();
                    continue;
                }

                // reset porque achou linha com dados
                emptyRowCount = 0;

                var costCenter = row.Cell(map.CostCenter).GetFormattedString().Trim();
                var name = row.Cell(map.Name).GetFormattedString().Trim();

                // 🔹 Se as colunas principais estão vazias, ignora linha
                if (string.IsNullOrWhiteSpace(costCenter) && string.IsNullOrWhiteSpace(name))
                {
                    row = row.RowBelow();
                    continue;
                }

                var model = new BalanceteDataModel
                {
                    BalanceteId = balanceteId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimalWithDC(row.Cell(map.InitialValue).GetFormattedString()),
                    Debit = ParseDecimalWithDC(row.Cell(map.Debit).GetFormattedString()),
                    Credit = ParseDecimalWithDC(row.Cell(map.Credit).GetFormattedString()),
                    FinalValue = ParseDecimalWithDC(row.Cell(map.FinalValue).GetFormattedString()),
                    BudgetedAmount = false
                };

                list.Add(model);

                row = row.RowBelow();
            }

            return list;
        }




        private List<BalanceteDataModel> ReadFromCsvDinamic(Stream stream, int balanceteId, BalanceteColumnMap map)
        {
            var list = new List<BalanceteDataModel>();
            stream.Position = 0;

            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            });

            int currentRow = 0;
            int emptyRowCount = 0; // contador de linhas vazias seguidas

            while (csv.Read())
            {
                currentRow++;

                // só começa a partir da linha selecionada
                if (currentRow < map.StartRow)
                    continue;

                var costCenter = csv.GetField(map.CostCenter - 1)?.Trim();
                var name = csv.GetField(map.Name - 1)?.Trim();

                bool emptyLine = string.IsNullOrWhiteSpace(costCenter) && string.IsNullOrWhiteSpace(name);

                // 🔹 Linha vazia
                if (emptyLine)
                {
                    emptyRowCount++;

                    // 🔥 Para após 3 vazias seguidas
                    if (emptyRowCount >= 3)
                        break;

                    continue;
                }

                // 🔹 Achou linha com conteúdo → reseta
                emptyRowCount = 0;

                var model = new BalanceteDataModel
                {
                    BalanceteId = balanceteId,
                    CostCenter = costCenter,
                    Name = name,
                    InitialValue = ParseDecimalWithDC(csv.GetField(map.InitialValue - 1)),
                    Debit = ParseDecimalWithDC(csv.GetField(map.Debit - 1)),
                    Credit = ParseDecimalWithDC(csv.GetField(map.Credit - 1)),
                    FinalValue = ParseDecimalWithDC(csv.GetField(map.FinalValue - 1)),
                    BudgetedAmount = false
                };

                list.Add(model);
            }

            return list;
        }



        private decimal ParseDecimalWithDC(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0m;

            input = input.Trim();

            char last = input[^1];
            bool hasDC = last is 'D' or 'd' or 'C' or 'c';

            string numeric = hasDC ? input[..^1].Trim() : input;

            // Remove separador de milhar
            numeric = numeric.Replace(".", "");

            // Troca vírgula por ponto
            numeric = numeric.Replace(",", ".");

            if (!decimal.TryParse(
                    numeric,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal value))
            {
                return 0m;
            }

            // Crédito vira negativo
            if (hasDC && (last == 'C' || last == 'c'))
                value *= -1;

            return value;
        }


        public class ExternalBranchDto
        {
            public int ExternalCode { get; set; }
            public string Name { get; set; }
        }


        public IEnumerable<ExternalBranchDto> ExtractBranchesFromBalancete(
    IEnumerable<string> linhas)
        {
            var branches = new Dictionary<int, ExternalBranchDto>();

            foreach (var linha in linhas)
            {
                if (!linha.Contains("FILIAL:", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Exemplo: FILIAL: 9 - FIAT - UNAI
                var match = Regex.Match(
                    linha,
                    @"FILIAL:\s*(\d+)\s*-\s*.*?\s*-\s*(.+)$",
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                    continue;

                var externalCode = int.Parse(match.Groups[1].Value);
                var name = match.Groups[2].Value.Trim();

                if (!branches.ContainsKey(externalCode))
                {
                    branches.Add(externalCode, new ExternalBranchDto
                    {
                        ExternalCode = externalCode,
                        Name = name
                    });
                }
            }

            return branches.Values;
        }



        public async Task<IEnumerable<ExternalBranchDto>> ExtractBranchesAsync(
    Stream stream,
    string fileName,
    BalanceteColumnMap map)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".csv" => await ExtractBranchesFromCsvAsync(stream, map),
                ".xlsx" => await Task.Run(() => ExtractBranchesFromXlsxDinamic(stream, map)),

                _ => throw new NotSupportedException(
                    $"Formato de arquivo não suportado: {extension}")
            };
        }

        public async Task<IEnumerable<ExternalBranchDto>> ExtractBranchesFromCsvAsync(
    Stream stream,
    BalanceteColumnMap map)
        {
            return await Task.Run(() =>
            {
                var branches = new Dictionary<int, ExternalBranchDto>();

                stream.Position = 0;

                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    Delimiter = ";"
                });

                int currentRow = 0;
                int emptyRowCount = 0;

                while (csv.Read())
                {
                    currentRow++;

                    if (currentRow < map.StartRow)
                        continue;

                    var descricao = csv.GetField(map.Name - 1)?.Trim();

                    if (string.IsNullOrWhiteSpace(descricao))
                    {
                        emptyRowCount++;

                        if (emptyRowCount >= 3)
                            break;

                        continue;
                    }

                    emptyRowCount = 0;

                    if (!descricao.Contains("FILIAL", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var match = Regex.Match(
                        descricao,
                        @"FILIAL\s*[:\-]\s*(\d+)\s*-\s*(.+)$",
                        RegexOptions.IgnoreCase);

                    if (!match.Success)
                        continue;

                    var externalCode = int.Parse(match.Groups[1].Value);
                    var branchName = match.Groups[2].Value.Trim();

                    if (!branches.ContainsKey(externalCode))
                    {
                        branches.Add(externalCode, new ExternalBranchDto
                        {
                            ExternalCode = externalCode,
                            Name = branchName
                        });
                    }
                }

                return branches.Values;
            });
        }



        private IEnumerable<ExternalBranchDto> ExtractBranchesFromXlsxDinamic(
            Stream stream,
            BalanceteColumnMap map)
        {
            var branches = new Dictionary<int, ExternalBranchDto>();

            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            int emptyRowCount = 0;

            var row = worksheet.Row(map.StartRow);

            while (true)
            {
                // 🔹 Linha completamente vazia
                if (row.IsEmpty())
                {
                    emptyRowCount++;

                    if (emptyRowCount >= 3)
                        break;

                    row = row.RowBelow();
                    continue;
                }

                // achou conteúdo → reseta
                emptyRowCount = 0;

                // 🔹 Lê apenas a coluna da descrição
                var descricao = row.Cell(map.Name)
                                   .GetFormattedString()
                                   ?.Trim();

                if (string.IsNullOrWhiteSpace(descricao))
                {
                    row = row.RowBelow();
                    continue;
                }

                // 🔍 Procura FILIAL
                if (!descricao.Contains("FILIAL", StringComparison.OrdinalIgnoreCase))
                {
                    row = row.RowBelow();
                    continue;
                }

                var match = Regex.Match(
                    descricao,
                    @"FILIAL\s*[:\-]\s*(\d+)\s*-\s*(.+)$",
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    row = row.RowBelow();
                    continue;
                }

                var externalCode = int.Parse(match.Groups[1].Value);
                var branchName = match.Groups[2].Value.Trim();

                if (!branches.ContainsKey(externalCode))
                {
                    branches.Add(externalCode, new ExternalBranchDto
                    {
                        ExternalCode = externalCode,
                        Name = branchName
                    });
                }

                row = row.RowBelow();
            }

            return branches.Values;
        }






    }
}