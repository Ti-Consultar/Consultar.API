using _3_Domain._1_Entities;
using _4_InfraData._6_Dto_sSQL;
using Microsoft.EntityFrameworkCore;
using static _4_InfraData._1_Repositories.GroupRepository;

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
        public DbSet<GroupModel> Groups { get; set; }
        public DbSet<BusinessEntity> BusinessEntity { get; set; }
        public DbSet<GroupCompanyDeletedDto> GroupCompanyDeletedDto { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<GroupCompanyDeletedDto>().HasNoKey(); // ← ESSENCIAL!
}

    }
}