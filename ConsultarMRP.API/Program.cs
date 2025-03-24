using Microsoft.OpenApi.Models;
using _2___Application._4__DependencyInjectionConfig; // Importação da configuração de dependências
using _2___Application._1_Services.User;
using _4_InfraData._1_Repositories; // Certifique-se de usar o namespace correto

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços ao contêiner
builder.Services.AddControllers();

// Obtém a connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Chama o método para configurar dependências gerais
DependencyInjectionConfig.Configure(builder.Services, connectionString);

// Adiciona o UserService diretamente
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();

// Configuração de CORS (permitindo todas as origens, ajuste conforme necessário)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuração do Swagger/OpenAPI
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

// Habilita o CORS para a aplicação
app.UseCors("AllowAll");

// Sempre habilita o Swagger, independentemente do ambiente
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConsultarMRP API v1");
    c.RoutePrefix = "swagger"; // O Swagger será acessível em http://localhost:7270/swagger
});

app.UseHttpsRedirection();
app.UseAuthentication(); // Adiciona autenticação (caso JWT esteja sendo usado)
app.UseAuthorization();
app.MapControllers();

app.Run();
