using Microsoft.OpenApi.Models;
using _2___Application._4__DependencyInjectionConfig; // Importa��o da configura��o de depend�ncias
using _2___Application._1_Services.User;
using _4_InfraData._1_Repositories; // Certifique-se de usar o namespace correto

var builder = WebApplication.CreateBuilder(args);

// Adiciona os servi�os ao cont�iner
builder.Services.AddControllers();

// Obt�m a connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Chama o m�todo para configurar depend�ncias gerais
DependencyInjectionConfig.Configure(builder.Services, connectionString);

// Adiciona o UserService diretamente
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();

// Configura��o de CORS (permitindo todas as origens, ajuste conforme necess�rio)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configura��o do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConsultarMRP API",
        Version = "v1",
        Description = "API MRP"
    });
});

var app = builder.Build();

// Habilita o CORS para a aplica��o
app.UseCors("AllowAll");

// Sempre habilita o Swagger, independentemente do ambiente
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConsultarMRP API v1");
    c.RoutePrefix = "swagger"; // O Swagger ser� acess�vel em http://localhost:7270/swagger
});

app.UseHttpsRedirection();
app.UseAuthentication(); // Adiciona autentica��o (caso JWT esteja sendo usado)
app.UseAuthorization();
app.MapControllers();

app.Run();
