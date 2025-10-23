using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;


namespace _4_InfraData._1_Repositories
{
    public class BudgetDataRepository : GenericRepository<BudgetDataModel>
    {
        private readonly CoreServiceDbContext _context;
        public BudgetDataRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<BudgetDataModel>> GetByBalanceteId(int id)
        {
            return await _context.BudgetData
                .Include(x => x.Budget)
                .Where(x => x.BudgetId == id)
                .ToListAsync();
        }


        public async Task<List<BalanceteDataAccountPlanClassification>> GetByAccountPlanClassificationId(int accountPlanId)
        {
            return await _context.BalanceteDataAccountPlanClassification
                .Include(a => a.AccountPlanClassification)
                .Where(x => x.AccountPlanClassification.AccountPlanId == accountPlanId)
                .ToListAsync();
        }
        public async Task<List<BudgetDataModel>> GetByBalanceteIdDate(int accountPlanId, int year, int month)
        {
            return await _context.BudgetData
                .Include(x => x.Budget)
                .Where(x => x.Budget.AccountPlansId == accountPlanId && x.Budget.DateYear == year && (int)x.Budget.DateMonth == month)
                .ToListAsync();
        }

        public async Task<List<BudgetDataModel>> GetByBalanceteDataByCostCenter(int budgetId, string? search)
        {
            return await _context.BudgetData
                .Include(x => x.Budget)
                .Where(x => x.BudgetId == budgetId &&
                            (string.IsNullOrEmpty(search) || x.CostCenter.StartsWith(search)))
                .ToListAsync();
        }


        public async Task<List<BudgetDataModel>> GetAgrupadoPorCostCenter(int id)
        {
            var data = await _context.BudgetData
                .Include(x => x.Budget)
                .Where(x => x.BudgetId == id)
                .ToListAsync();

            return AgruparPorCostCenterPai(data);
        }
        public async Task<List<BudgetDataModel>> GetAgrupadoPorCostCenterListMultiBalancete(List<string> costCenters, List<int> budgetIds)
        {
            return await _context.BudgetData
                .Where(bd => budgetIds.Contains(bd.BudgetId) && costCenters.Contains(bd.CostCenter))
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


        public List<BudgetDataModel> AgruparPorCostCenterPai(List<BudgetDataModel> data)
        {
            var grupos = data
                .GroupBy(x => GetCostCenterPai(x.CostCenter))
                .Select(g =>
                {
                    // Tenta pegar o Name do item que é exatamente o pai (ex.: CostCenter == "1")
                    var itemPai = g.FirstOrDefault(x => x.CostCenter == g.Key);

                    return new BudgetDataModel
                    {
                        CostCenter = g.Key,
                        Name = itemPai != null ? itemPai.Name : $"Grupo {g.Key}",
                        InitialValue = g.Sum(x => x.InitialValue),
                        Credit = g.Sum(x => x.Credit),
                        Debit = g.Sum(x => x.Debit),
                        FinalValue = g.Sum(x => x.FinalValue),
                        BudgetedAmount = g.Any(x => x.BudgetedAmount),
                        BudgetId = g.First().BudgetId,
                        Budget = g.First().Budget,
                     
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


    }
}
