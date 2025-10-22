using _3_Domain._1_Entities;
using _4_InfraData._6_Dto_sSQL;
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
        public DbSet<GroupModel> Groups { get; set; }
        public DbSet<BusinessEntity> BusinessEntity { get; set; }
        public DbSet<GroupCompanyDeletedDto> GroupCompanyDeletedDto { get; set; }
        public DbSet<GroupSubCompanyDeletedDto> GroupSubCompanyDeletedDto { get; set; }
        public DbSet<AccountPlansModel> AccountPlans { get; set; }
        public DbSet<BalanceteModel> Balancete { get; set; }
        public DbSet<PlansAccountUsersModel> PlansAccountUsers { get; set; }
        public DbSet<BalanceteDataModel> BalanceteData { get; set; }
        public DbSet<InteractionModel> Interaction { get; set; }
        public DbSet<ClassificationModel> Classification { get; set; }
        public DbSet<AccountPlanClassification> AccountPlanClassification { get; set; }
        public DbSet<BalanceteDataAccountPlanClassification> BalanceteDataAccountPlanClassification { get; set; }
        public DbSet<TotalizerClassificationModel> TotalizerClassification { get; set; }
        public DbSet<TotalizerClassificationTemplate> TotalizerClassificationTemplate { get; set; }
        public DbSet<BalancoReclassificadoTemplate> BalancoReclassificadoTemplate { get; set; }
        public DbSet<BalancoReclassificadoModel> BalancoReclassificado { get; set; }
        public DbSet<ParameterModel> Parameter { get; set; }
        public DbSet<BudgetModel> Budget { get; set; }
        public DbSet<BudgetDataModel> BudgetData { get; set; }
    


        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<GroupCompanyDeletedDto>().HasNoKey(); // ← ESSENCIAL!
            modelBuilder.Entity<GroupSubCompanyDeletedDto>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // Opcional: se não for view, evita erro no EF Core
            }); // ← ESSENCIAL!
        }

    }
}