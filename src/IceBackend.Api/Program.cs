using System;
using System.IO;
using System.Text.Json;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// 1. Cargar archivo .env local para desarrollo
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (!File.Exists(envPath)) 
{
    envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env"); // Fallback
}

if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(); // Ensure env vars override appsettings

// 2. Validación estricta de entorno (Fail-Fast)
var postgresConn = builder.Configuration.GetConnectionString("PostgresConnection");
if (string.IsNullOrWhiteSpace(postgresConn) || postgresConn.Contains("YOUR_POSTGRES_HOST"))
{
    throw new InvalidOperationException("CRITICAL ERROR: 'PostgresConnection' is missing or has a placeholder. Please configure your .env file.");
}

var redisConn = builder.Configuration.GetConnectionString("RedisConnection");
if (string.IsNullOrWhiteSpace(redisConn) || redisConn.Contains("YOUR_REDIS_CONNECTION_STRING"))
{
    throw new InvalidOperationException("CRITICAL ERROR: 'RedisConnection' is missing or has a placeholder. Please configure your .env file.");
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SessionToken";
    options.DefaultChallengeScheme = "SessionToken";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, IceBackend.Api.Authentication.SessionTokenAuthenticationHandler>("SessionToken", null);

builder.Services.AddAuthorization();

// Configure Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Redis (IDistributedCache) — la capa Domain y Application no dependen de Redis directamente.
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");

// Registro manual de ConnectionMultiplexer para comandos avanzados (Sets, SISMEMBER)
var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection!);
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(redis);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "IceLauncher:";
});

// Register Application Services with Fail-Fast Validation
builder.Services.AddOptions<IceBackend.Application.Options.AuthOptions>()
    .BindConfiguration(IceBackend.Application.Options.AuthOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<IceBackend.Application.Options.StripeOptions>()
    .BindConfiguration(IceBackend.Application.Options.StripeOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<IceBackend.Application.Options.OAuthOptions>()
    .BindConfiguration(IceBackend.Application.Options.OAuthOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<IceBackend.Application.Options.CdnOptions>()
    .BindConfiguration(IceBackend.Application.Options.CdnOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient("oauth"); // Named HttpClient para llamadas a providers OAuth2
builder.Services.AddScoped<IceBackend.Application.Interfaces.ISessionCache, IceBackend.Infrastructure.Services.RedisSessionCache>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IPasswordHasher, IceBackend.Infrastructure.Services.BcryptPasswordHasher>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IAuthService, IceBackend.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IStripeWebhookValidator, IceBackend.Infrastructure.Services.StripeWebhookValidator>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IStripeWebhookService, IceBackend.Infrastructure.Services.StripeWebhookService>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IExternalAuthService, IceBackend.Infrastructure.Services.ExternalAuthService>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.ICdnUrlSigner, IceBackend.Infrastructure.Services.CdnUrlSigner>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.ICosmeticAssetQueryService, IceBackend.Infrastructure.Queries.CosmeticAssetQueryService>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IAssetTokenService, IceBackend.Infrastructure.Services.AssetTokenService>();

// Servicios de Dominio y Firmadores Criptográficos (Clean Architecture / DDD)
builder.Services.AddHttpClient<IceBackend.Domain.Services.IMojangSessionValidator, IceBackend.Infrastructure.Services.MojangSessionValidator>();
builder.Services.AddScoped<IceBackend.Domain.Services.ICosmeticTokenSigner, IceBackend.Infrastructure.Services.CosmeticTokenSigner>();
builder.Services.AddScoped<IceBackend.Domain.Services.CosmeticEquipmentPolicy>();


// Client Context: Scoped service populated by middleware from X-Client-Architecture header
builder.Services.AddScoped<IceBackend.Api.Middleware.ClientContext>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IClientContext>(sp => sp.GetRequiredService<IceBackend.Api.Middleware.ClientContext>());

// Inventory: Repository + Use Cases
builder.Services.AddScoped<IceBackend.Application.Interfaces.IInventoryRepository, IceBackend.Infrastructure.Repositories.InventoryRepository>();
builder.Services.AddScoped<IceBackend.Application.UseCases.Inventory.EquipCosmeticUseCase>();
builder.Services.AddScoped<IceBackend.Application.UseCases.Inventory.UnequipCosmeticUseCase>();

// Register Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("db_check");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<IceBackend.Api.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<IceBackend.Api.Middleware.ClientContextMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                component = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description
            }),
            duration = report.TotalDuration
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.Run();