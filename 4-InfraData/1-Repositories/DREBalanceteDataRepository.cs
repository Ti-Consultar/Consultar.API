using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._1_Repositories
{
    public class DREBalanceteDataRepository : GenericRepository<DREBalanceteData>
    {
        private readonly CoreServiceDbContext _context;
        public DREBalanceteDataRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<DREBalanceteData>> GetByDREId(int dREId)
        {
            return await _context.DREBalanceteData
                .Include(x => x.Dre)
                .Include(x => x.Balancete)
                .Include(x => x.BalanceteData)
                .Where(x => x.DREId == dREId)
                .ToListAsync();
        }

        public async Task<List<DREBalanceteData>> GetByBalanceteId(int balanceteId)
        {
            return await _context.DREBalanceteData
                .Include(x => x.Dre)
                .Include(x => x.Balancete)
                .Include(x => x.BalanceteData)
                .Where(x => x.BalanceteId == balanceteId)
                .ToListAsync();
        }

    }
}
