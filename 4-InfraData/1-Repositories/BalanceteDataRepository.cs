using _3_Domain._1_Entities;
using _4_InfraData._1_Context;


namespace _4_InfraData._1_Repositories
{
    public class BalanceteDataRepository : GenericRepository<BalanceteDataModel>
    {
        public BalanceteDataRepository(CoreServiceDbContext context) : base(context)
        {
        }
    }
}
