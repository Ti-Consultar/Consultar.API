using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;

namespace _4_InfraData._1_Repositories
{
    public class BalanceteRepository : GenericRepository<BalanceteModel>
    {
        private readonly CoreServiceDbContext _context;
        public BalanceteRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<BalanceteModel>> GetByAccountPlanId(int accountPlansId)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.AccountPlansId == accountPlansId)
                .ToListAsync();
        }
        public async Task<List<BalanceteModel>> GetById(int id)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.Id == id)
                .ToListAsync();
        }
        public async Task<List<BalanceteModel>> GetByIdDelete(int id)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Include(x => x.BalancetesData)
                .Where(x => x.Id == id)
                .ToListAsync();
        }
        public async Task<BalanceteModel> GetBalanceteById(int id)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }


    }
}
