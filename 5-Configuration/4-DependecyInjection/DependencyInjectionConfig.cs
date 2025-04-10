﻿using _4_InfraData._2_AppSettings;
using _5_Configuration._1_Context;
using _5_Configuration._2_JWT;
using _5_Configuration._3_Utils.Base;
using _5_Configuration._3_Utils.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        options.AddPolicy("Admin", policy => policy.RequireRole("Administrador"));
        options.AddPolicy("Master", policy => policy.RequireRole("Master"));
        options.AddPolicy("Consultor", policy => policy.RequireRole("Consultor"));
    });

    services.AddCors();

    #region dependências 

    #region Services
    services.AddScoped<IAppSettings, AppSettings>(); // Certifique-se de que `AppSettings` não tem dependências não registradas
    services.AddScoped<BaseService>();
    services.AddScoped<UserService>();
    services.AddScoped<EmailService>();
    #endregion

    #region Repositories
    // Adicione repositórios, se necessário
    #endregion

    #endregion
}
