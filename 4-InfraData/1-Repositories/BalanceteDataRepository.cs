using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;


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


    }
}
