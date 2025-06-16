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
    public class DRERepository : GenericRepository<DREModel>
    {
        private readonly CoreServiceDbContext _context;
        public DRERepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<DREModel>> GetByAccountPlanId(int accountPlansId)
        {
            return await _context.DRE
                .Include(x => x.Classification)
                .Include(x => x.AccountPlan)
                .Where(x => x.AccountPlanId == accountPlansId)
                .OrderBy(x => x.Classification.Type)
                .ToListAsync();
        }


        public async Task<DREModel> GetByDREId(int id)
        {
            return await _context.DRE
                 .Where(x => x.Id == id)
                .Include(x => x.Classification)
                .Include(x => x.AccountPlan)
                .FirstOrDefaultAsync();
        }
        
    }
}