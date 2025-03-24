using _3_Domain._1_Entities;
using Microsoft.EntityFrameworkCore;


namespace _4_InfraData._1_Context
{
    public class CoreServiceDbContext : DbContext
    {
        public CoreServiceDbContext(DbContextOptions<CoreServiceDbContext> options) : base(options)
        {

        }
        public DbSet<UserModel> Users { get; set; }
      
    }
}
