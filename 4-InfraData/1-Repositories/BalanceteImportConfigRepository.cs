using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;


namespace _4_InfraData._1_Repositories
{
    public class BalanceteImportConfigRepository : GenericRepository<BalanceteImportConfig>
    {
        private readonly CoreServiceDbContext _context;
        public BalanceteImportConfigRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<BalanceteImportConfig?> GetByAccountPlanIdAsync(int accountPlanId)
        {
            var model = await _context.BalanceteImportConfig
                .Where(c => c.AccountPlanId == accountPlanId)
                .FirstOrDefaultAsync();

            return model;
        }
        public async Task<bool> ExistsAccountPlanAsync(int accountPlanId)
        {
            return await _context.BalanceteImportConfig
                .AnyAsync(x => x.AccountPlanId == accountPlanId);
        }
    }
}
