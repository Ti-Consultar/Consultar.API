
using _2___Application._1_Services;
using _2___Application._1_Services.AccountPlans;
using _2___Application._1_Services.AccountPlans.Balancete;
using _2___Application._1_Services.Budget;
using _2___Application._1_Services.CashFlow;
using _2___Application._1_Services.Parameter;
using _2___Application._1_Services.Results;
using _2___Application._1_Services.Results.CIL_e_EC;
using _2___Application._1_Services.Results.OperationalEfficiency;
using _2___Application._1_Services.TotalizerClassification;
using _2___Application._1_Services.ValueTree;
using _2___Application._1_Services.Scope;
using _2___Application.Base;
using _4_Application._1_Services;
using _4_InfraData._1_Context;
using _4_InfraData._1_Repositories;
using _4_InfraData._2_AppSettings;
using _4_InfraData._2_JWT;
using _4_InfraData._3_Utils.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;



namespace _2___Application._4__DependencyInjectionConfig
{
    public static class DependencyInjectionConfig
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration, string? connectionString)
        {
            // Registra IHttpContextAccessor para ser usado por outros serviços
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configuração da conexão com o banco de dados
            services.AddDbContext<CoreServiceDbContext>(options => options.UseSqlServer(connectionString));

            // Configuração do serviço de autenticação JWT
            var jwtSettings = CreateJwtSettings(configuration);
            services.AddSingleton(Options.Create(jwtSettings));
            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

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
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
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
            services.AddScoped<TokenService>();
            services.AddScoped<BaseService>();
            services.AddScoped<EmailService>();
            services.AddScoped<CompanyService>();
            services.AddScoped<PermissionService>();
            services.AddScoped<InvitationService>();
            services.AddScoped<GroupService>();
            services.AddScoped<CnpjService>();
            services.AddScoped<CepService>();
            services.AddScoped<BreadcrumbService>();
            services.AddScoped<ClassificationRepository>();
            services.AddScoped<AccountPlansService>();
            services.AddScoped<BalanceteService>();
            services.AddScoped<BalanceteDataService>();
            services.AddScoped<InteractionService>();
            services.AddScoped<ClassificationService>();
            services.AddScoped<TotalizerClassificationService>();
            services.AddScoped<CilECService>();
            services.AddScoped<EconomicIndicesService>();
            services.AddScoped<LiquidManagementService>();
            services.AddScoped<OperationalEfficiencyService>();
            services.AddScoped<ParameterService>();
            services.AddScoped<CashFlowService>();
            services.AddScoped<ValueTreeService>();
            services.AddScoped<BudgetService>();
            services.AddScoped<ConfigService>();
            services.AddScoped<IAccountPlanScopeResolver, AccountPlanScopeResolver>();




            #endregion

            #region Repositories
            // Adicione repositórios, se necessário

            services.AddScoped<CompanyRepository>();
            services.AddScoped<PermissionRepository>();
            services.AddScoped<InvitationRepository>();
            services.AddScoped<GroupRepository>();
            services.AddScoped<BusinessEntityRepository>();
            services.AddScoped<AccountPlansRepository>();
            services.AddScoped<AccountPlanAccountRepository>();
            services.AddScoped<BalanceteRepository>();
            services.AddScoped<BalanceteDataRepository>();
            services.AddScoped<InteractionRepository>();
            services.AddScoped<PlansAccountUserRepository>();
            services.AddScoped<ClassificationRepository>();
            services.AddScoped<AccountPlanClassificationRepository>();
            services.AddScoped<TotalizerClassificationRepository>();
            services.AddScoped<TotalizerClassificationTemplateRepository>();
            services.AddScoped<BalancoReclassificadoRepository>();
            services.AddScoped<BalancoReclassificadoTemplateRepository>();
            services.AddScoped<ParameterRepository>();
            services.AddScoped<BudgetRepository>();
            services.AddScoped<BudgetDataRepository>();
            services.AddScoped<BalanceteImportConfigRepository>();
            services.AddScoped<ConfigPrincipalRepository>();
     
            #endregion
             
            #endregion
        }

        private static JwtSettings CreateJwtSettings(IConfiguration configuration)
        {
            var expirationHours = int.TryParse(configuration["Jwt:ExpirationHours"], out var configuredExpirationHours)
                ? configuredExpirationHours
                : 2;

            var settings = new JwtSettings
            {
                SecretKey = configuration["Jwt:SecretKey"] ?? string.Empty,
                Issuer = configuration["Jwt:Issuer"] ?? string.Empty,
                Audience = configuration["Jwt:Audience"] ?? string.Empty,
                ExpirationHours = expirationHours
            };

            if (string.IsNullOrWhiteSpace(settings.SecretKey))
                throw new InvalidOperationException("Jwt:SecretKey must be configured.");

            if (Encoding.UTF8.GetByteCount(settings.SecretKey) < 32)
                throw new InvalidOperationException("Jwt:SecretKey must be at least 32 bytes.");

            if (string.IsNullOrWhiteSpace(settings.Issuer))
                throw new InvalidOperationException("Jwt:Issuer must be configured.");

            if (string.IsNullOrWhiteSpace(settings.Audience))
                throw new InvalidOperationException("Jwt:Audience must be configured.");

            if (settings.ExpirationHours <= 0)
                throw new InvalidOperationException("Jwt:ExpirationHours must be greater than zero.");

            return settings;
        }
    }
}
