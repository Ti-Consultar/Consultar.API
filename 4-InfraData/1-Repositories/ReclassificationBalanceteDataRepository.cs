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
    public class ReclassificationBalanceteDataRepository : GenericRepository<ReclassificationBalanceteModel>
    {
        private readonly CoreServiceDbContext _context;
        public ReclassificationBalanceteDataRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<ReclassificationBalanceteModel>> GetByDREId(int dREId)
        {
            return await _context.DREBalanceteData
                .Include(x => x.Dre)
                .Include(x => x.Balancete)
                .Include(x => x.BalanceteData)
                .Where(x => x.DREId == dREId)
                .ToListAsync();
        }

        public async Task<List<ReclassificationBalanceteModel>> GetByBalanceteId(int balanceteId)
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
