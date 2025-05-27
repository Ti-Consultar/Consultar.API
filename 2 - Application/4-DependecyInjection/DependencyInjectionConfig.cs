
using _4_InfraData._1_Context;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._2_JWT;
using _2___Application.Base;
using _4_InfraData._3_Utils.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _2___Application._1_Services.AccountPlans;
using _4_Application._1_Services;
using _2___Application._1_Services.AccountPlans.Balancete;


namespace _2___Application._4__DependencyInjectionConfig
{
    public static class DependencyInjectionConfig
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            // Registra IHttpContextAccessor para ser usado por outros serviços
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configuração da conexão com o banco de dados
            services.AddDbContext<CoreServiceDbContext>(options => options.UseSqlServer(connectionString));

            // Configuração do serviço de autenticação JWT
            var key = Encoding.ASCII.GetBytes(Settings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("Gestor", policy => policy.RequireRole("Gestor"));
                options.AddPolicy("Usuario", policy => policy.RequireRole("Usuário"));
                options.AddPolicy("Consultor", policy => policy.RequireRole("Consultor"));
                options.AddPolicy("Comercial", policy => policy.RequireRole("Comercial"));
                options.AddPolicy("Desenvolvedor", policy => policy.RequireRole("Desenvolvedor"));
                options.AddPolicy("Designer", policy => policy.RequireRole("Designer"));
            });

            services.AddCors();

            #region dependências 

            #region Services
            services.AddScoped<IAppSettings, AppSettings>(); // Certifique-se de que `AppSettings` não tem dependências não registradas
            services.AddScoped<BaseService>();
            services.AddScoped<EmailService>();
            services.AddScoped<CompanyService>();
            services.AddScoped<PermissionService>();
            services.AddScoped<InvitationService>();
            services.AddScoped<GroupService>();
            services.AddScoped<CnpjService>();
            services.AddScoped<CepService>();
            services.AddScoped<BreadcrumbService>();

            services.AddScoped<AccountPlansService>();
            services.AddScoped<BalanceteService>();
            services.AddScoped<BalanceteDataService>();
            services.AddScoped<InteractionService>();


            #endregion

            #region Repositories
            // Adicione repositórios, se necessário

            services.AddScoped<CompanyRepository>();
            services.AddScoped<PermissionRepository>();
            services.AddScoped<InvitationRepository>();
            services.AddScoped<GroupRepository>();
            services.AddScoped<BusinessEntityRepository>();

            services.AddScoped<AccountPlansRepository>();
            services.AddScoped<BalanceteRepository>();
            services.AddScoped<BalanceteDataRepository>();
            services.AddScoped<InteractionRepository>();
            services.AddScoped<PlansAccountUserRepository>();
            #endregion

            #endregion
        }
    }
}
