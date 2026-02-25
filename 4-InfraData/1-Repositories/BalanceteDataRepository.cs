using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace _4_InfraData._1_Repositories
{
    public class BalanceteDataRepository : GenericRepository<BalanceteDataModel>
    {
        private readonly CoreServiceDbContext _context;
        public BalanceteDataRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<BalanceteDataModel>> GetByBalanceteId(int balanceteId)
        {
            return await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x => x.BalanceteId == balanceteId)
                .ToListAsync();
        }


        public async Task<List<BalanceteDataAccountPlanClassification>> GetByAccountPlanClassificationId(int accountPlanId)
        {
            return await _context.BalanceteDataAccountPlanClassification
                .Include(a => a.AccountPlanClassification)
                .Where(x => x.AccountPlanClassification.AccountPlanId == accountPlanId)
                .ToListAsync();
        }
        public async Task<List<BalanceteDataModel>> GetByBalanceteIdDate(int accountPlanId, int year, int month)
        {
            return await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x => x.Balancete.AccountPlansId == accountPlanId && x.Balancete.DateYear == year && (int)x.Balancete.DateMonth == month)
                .ToListAsync();
        }

        public async Task<List<BalanceteDataModel>> GetByBalanceteDataByCostCenter(int balanceteId, string? search)
        {
            return await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x => x.BalanceteId == balanceteId &&
                            (string.IsNullOrEmpty(search) || x.CostCenter.StartsWith(search)))
                .ToListAsync();
        }


        public async Task<List<BalanceteDataModel>> GetAgrupadoPorCostCenter(int balanceteId)
        {
            var data = await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x => x.BalanceteId == balanceteId)
                .ToListAsync();

            return AgruparPorCostCenterPai(data);
        }
        public async Task<List<BalanceteDataModel>> GetAgrupadoPorCostCenterListMultiBalancete(List<string> costCenters, List<int> balanceteIds)
        {
            return await _context.BalanceteData
                .Where(bd => balanceteIds.Contains(bd.BalanceteId) && costCenters.Contains(bd.CostCenter))
                .ToListAsync();
        }
        public async Task<List<BalanceteDataModel>> GetAgrupadoPorCostCenterListMonthAsync(List<string> costCenters, int balanceteId)
        {
            return await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x => x.BalanceteId == balanceteId && costCenters.Contains(x.CostCenter))
                .ToListAsync();
        }
        public async Task<List<BalanceteDataModel>> GetAgrupadoPorCostCenterListAsync(List<string> costCenters)
        {
            return await _context.BalanceteData
                .Include(x => x.Balancete)
                .Where(x =>  costCenters.Contains(x.CostCenter))
                .ToListAsync();
        }


        public List<BalanceteDataModel> AgruparPorCostCenterPai(List<BalanceteDataModel> data)
        {
            var grupos = data
                .GroupBy(x => GetCostCenterPai(x.CostCenter))
                .Select(g =>
                {
                    // Tenta pegar o Name do item que é exatamente o pai (ex.: CostCenter == "1")
                    var itemPai = g.FirstOrDefault(x => x.CostCenter == g.Key);

                    return new BalanceteDataModel
                    {
                        CostCenter = g.Key,
                        Name = itemPai != null ? itemPai.Name : $"Grupo {g.Key}",
                        InitialValue = g.Sum(x => x.InitialValue),
                        Credit = g.Sum(x => x.Credit),
                        Debit = g.Sum(x => x.Debit),
                        FinalValue = g.Sum(x => x.FinalValue),
                        BudgetedAmount = g.Any(x => x.BudgetedAmount),
                        BalanceteId = g.First().BalanceteId,
                        Balancete = g.First().Balancete,
                     
                    };
                })
                .ToList();

            return grupos;
        }


        private string GetCostCenterPai(string costCenter)
        {
            var partes = costCenter.Split('.');
            return partes.Length > 0 ? partes[0] : costCenter;
        }



public async Task BulkInsertAsync(List<BalanceteDataModel> list)
    {
        if (list == null || !list.Any())
            return;

        var table = new DataTable();

        table.Columns.Add("BalanceteId", typeof(int));
        table.Columns.Add("CostCenter", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("InitialValue", typeof(decimal));
        table.Columns.Add("Debit", typeof(decimal));
        table.Columns.Add("Credit", typeof(decimal));
        table.Columns.Add("FinalValue", typeof(decimal));
        table.Columns.Add("CreatedAt", typeof(DateTime));

        foreach (var item in list)
        {
            table.Rows.Add(
                item.BalanceteId,
                item.CostCenter,
                item.Name,
                item.InitialValue,
                item.Debit,
                item.Credit,
                item.FinalValue,
                DateTime.Now
            );
        }

        var connection = (SqlConnection)_context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "BalanceteData",
            BatchSize = 5000,
            BulkCopyTimeout = 0 // sem limite
        };

        bulkCopy.ColumnMappings.Add("BalanceteId", "BalanceteId");
        bulkCopy.ColumnMappings.Add("CostCenter", "CostCenter");
        bulkCopy.ColumnMappings.Add("Name", "Name");
        bulkCopy.ColumnMappings.Add("InitialValue", "InitialValue");
        bulkCopy.ColumnMappings.Add("Debit", "Debit");
        bulkCopy.ColumnMappings.Add("Credit", "Credit");
        bulkCopy.ColumnMappings.Add("FinalValue", "FinalValue");
        bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");

        await bulkCopy.WriteToServerAsync(table);
    }
}
}
