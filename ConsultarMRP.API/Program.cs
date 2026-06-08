using Microsoft.OpenApi.Models;
using _2___Application._4__DependencyInjectionConfig;
using _2___Application._1_Services.User;
using _4_InfraData._1_Repositories;

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName;
const string CorsPolicyName = "ConfiguredCors";

builder.Services.AddControllers();

var connectionString = GetRequiredConnectionString(builder);
var allowedOrigins = GetRequiredCorsOrigins(builder.Configuration, environmentName);

DependencyInjectionConfig.Configure(builder.Services, builder.Configuration, connectionString);

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
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
        Title = "Consultar MRP API",
        Version = "v1",
        Description = "API MRP"
    });
});

var app = builder.Build();
var swaggerEnabled = bool.TryParse(app.Configuration["Swagger:Enabled"], out var configuredSwaggerEnabled) && configuredSwaggerEnabled;

app.Logger.LogInformation("Starting ConsultarMRP.API in {EnvironmentName}.", app.Environment.EnvironmentName);

app.UseCors(CorsPolicyName);

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConsultarMRP API v1");
        c.RoutePrefix = "swagger";
    });

    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

static string GetRequiredConnectionString(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException($"ConnectionStrings:DefaultConnection must be configured for {builder.Environment.EnvironmentName}.");

    return connectionString;
}

static string[] GetRequiredCorsOrigins(IConfiguration configuration, string environmentName)
{
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")
        .GetChildren()
        .Select(origin => origin.Value)
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin!)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    if (allowedOrigins.Length == 0)
        throw new InvalidOperationException($"Cors:AllowedOrigins must be configured for {environmentName}.");

    if (allowedOrigins.Any(origin => origin == "*"))
        throw new InvalidOperationException($"Cors:AllowedOrigins must list explicit origins for {environmentName}.");

    return allowedOrigins;
}
