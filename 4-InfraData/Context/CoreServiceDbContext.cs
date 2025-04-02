using _3_Domain._1_Entities;
using Microsoft.EntityFrameworkCore;

namespace _4_InfraData._1_Context
{
    public class CoreServiceDbContext : DbContext
    {
        public CoreServiceDbContext(DbContextOptions<CoreServiceDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<CompanyModel> Companies { get; set; }
        public DbSet<SubCompanyModel> SubCompanies { get; set; }
        public DbSet<CompanyUserModel> CompanyUsers { get; set; }
        public DbSet<PermissionModel> Permissions { get; set; }
        public DbSet<InvitationToCompany> InvitationToCompany { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvitationToCompany>()
                .Property(i => i.InvitedById)
                .HasColumnName("InvitedById");
        }


    }
}