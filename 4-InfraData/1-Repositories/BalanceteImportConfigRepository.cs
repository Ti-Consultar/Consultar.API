using _3_Domain._1_Entities;
using _4_InfraData._1_Context;


namespace _4_InfraData._1_Repositories
{
    public class BalanceteImportConfigRepository : GenericRepository<BalanceteImportConfig>
    {
        private readonly CoreServiceDbContext _context;
        public BalanceteImportConfigRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

    }
}
