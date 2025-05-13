using _3_Domain._1_Entities;
using _4_InfraData._1_Context;


namespace _4_InfraData._1_Repositories
{
    public class InteractionRepository : GenericRepository<InteractionModel>
    {
        public InteractionRepository(CoreServiceDbContext context) : base(context)
        {
        }
    }
}
