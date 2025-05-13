using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using System;

namespace _4_InfraData._1_Repositories
{
    public class AccountPlansRepository : GenericRepository<AccountPlansModel>
    {
        public AccountPlansRepository(CoreServiceDbContext context) : base(context)
        {
        }
    }
}
