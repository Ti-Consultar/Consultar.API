using _3_Domain._1_Entities;
using _4_InfraData._1_Context;


namespace _4_InfraData._1_Repositories
{
    public class TotalizerClassificationTemplateRepository : GenericRepository<TotalizerClassificationTemplate>
    {
        private readonly CoreServiceDbContext _context;
        public TotalizerClassificationTemplateRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

    }
}
