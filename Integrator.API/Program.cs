using _4_InfraData._2_AppSettings;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "AllowConfiguredOrigins";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAppSettings, AppSettings>();
builder.Services.AddScoped<CepService>();
builder.Services.AddScoped<CnpjService>();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Integrator API",
        Version = "v1",
        Description = "API Integrator"
    });
});

var app = builder.Build();

app.UseCors(CorsPolicyName);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Integrator API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});
app.Run();
