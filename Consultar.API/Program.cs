using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os servi�os ao cont�iner
builder.Services.AddControllers();

// Configura��o do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConsultarAuth API",
        Version = "v1",
        Description = "API de Autentica��o do sistema ConsultarAuth"
    });
});

var app = builder.Build();

// Configura��o do pipeline de requisi��es HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Configura��o para acessar o Swagger via a URL raiz
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConsultarAuth API v1");
        c.RoutePrefix = string.Empty;  // Deixa o Swagger na raiz (http://localhost:5001/)
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
