using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;


namespace _4_InfraData._1_Repositories
{
    public class ParameterRepository : GenericRepository<ParameterModel>
    {
        private readonly CoreServiceDbContext _context;
        public ParameterRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ParameterModel>> GetByAccountPlanIdYear(int accountPlansId, int year)
        {
            return await _context.Parameter
                .Include(x => x.AccountPlans)
                .Where(x => x.AccountPlansId == accountPlansId && x.ParameterYear == year)
                .ToListAsync();
        }

        public async Task AddAsync(ParameterModel entity)
        {
            _context.Parameter.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}
