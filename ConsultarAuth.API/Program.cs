using Microsoft.OpenApi.Models;
using _2___Application._4__DependencyInjectionConfig;
using _2___Application._1_Services.User;
using _4_InfraData._1_Repositories;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================
// 🔧 CONFIGURAÇÕES INICIAIS
// =========================

// Log para debug no Azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// =========================
// 📦 SERVICES
// =========================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 🔥 Pega connection string (Azure ou local)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ❌ Se não vier do Azure → quebra com erro claro
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("❌ Connection string NÃO configurada. Verifique Azure (ConnectionStrings__DefaultConnection)");
}

// 🗄️ DbContext
builder.Services.AddDbContext<CoreServiceDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🔗 Injeção de dependência customizada
DependencyInjectionConfig.Configure(builder.Services, connectionString);

// Serviços
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthorizationService>();
builder.Services.AddScoped<UserRepository>();

// 🌐 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 📄 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Consultar Auth API",
        Version = "v1",
        Description = "API de Autenticação do sistema ConsultarAuth"
    });
});

// =========================
// 🚀 APP
// =========================

var app = builder.Build();

// Log pra debug
app.Logger.LogInformation("🔥 Aplicação iniciando...");

// 🌐 CORS
app.UseCors("AllowAll");

// 📄 Swagger (sempre ativo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConsultarAuth API v1");
    c.RoutePrefix = "swagger";
});

// 🔐 HTTPS
app.UseHttpsRedirection();

// ⚠️ Só usa se tiver autenticação configurada
// app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// 🔁 Redireciona raiz para Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// =========================
// 🧨 RUN COM TRATAMENTO
// =========================

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("💥 ERRO AO INICIAR A APLICAÇÃO:");
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
    throw;
}