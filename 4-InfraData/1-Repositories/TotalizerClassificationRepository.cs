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
    public class TotalizerClassificationRepository : GenericRepository<TotalizerClassificationModel>
    {
        private readonly CoreServiceDbContext _context;
        public TotalizerClassificationRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<TotalizerClassificationModel>> GetByAccountPlanId(int accountPlanId)
        {
            return await _context.TotalizerClassification
                .Where(x => x.AccountPlanId == accountPlanId)
                .ToListAsync();
        }

        public async Task<List<TotalizerClassificationModel>> GetByAccountPlanIdList(int accountPlanId, List<int> ids)
        {
            return await _context.TotalizerClassification
                .Where(x => x.AccountPlanId == accountPlanId && ids.Contains(x.Id))
                .ToListAsync();
        }

    }
}
