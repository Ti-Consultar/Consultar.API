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
        public DbSet<CompanyUserModel> CompanyUsers { get; set; } // Tabela de relacionamento

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost;Database=ConsultarDB;Integrated Security=true;trustServerCertificate=true",
                    b => b.MigrationsAssembly("4-InfraData")); // Define o projeto onde as migrations serão criadas
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relacionamento N:N entre Company e User através de CompanyUser
            modelBuilder.Entity<CompanyUserModel>()
                .HasKey(cu => new { cu.UserId, cu.CompanyId }); // Chave composta

            modelBuilder.Entity<CompanyUserModel>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.CompanyUsers)
                .HasForeignKey(cu => cu.UserId);

            modelBuilder.Entity<CompanyUserModel>()
                .HasOne(cu => cu.Company)
                .WithMany(c => c.CompanyUsers)
                .HasForeignKey(cu => cu.CompanyId);

            // Relacionamento 1:N entre Company e SubCompany
            modelBuilder.Entity<SubCompanyModel>()
                .HasOne(s => s.Company)
                .WithMany(c => c.SubCompanies)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
