using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços ao contêiner
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConsultarAuth API",
        Version = "v1",
        Description = "API de Autenticação do sistema ConsultarAuth"
    });
});

var app = builder.Build();

// Configuração do pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Configuração para acessar o Swagger via a URL raiz
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConsultarAuth API v1");
        c.RoutePrefix = string.Empty;  // Deixa o Swagger na raiz (http://localhost:5001/)
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
