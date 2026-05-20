using System.Text.Json;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

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

// Register Application Services
builder.Services.Configure<IceBackend.Application.Options.AuthOptions>(
    builder.Configuration.GetSection(IceBackend.Application.Options.AuthOptions.SectionName));
builder.Services.Configure<IceBackend.Application.Options.StripeOptions>(
    builder.Configuration.GetSection(IceBackend.Application.Options.StripeOptions.SectionName));
builder.Services.Configure<IceBackend.Application.Options.OAuthOptions>(
    builder.Configuration.GetSection(IceBackend.Application.Options.OAuthOptions.SectionName));
builder.Services.Configure<IceBackend.Application.Options.CdnOptions>(
    builder.Configuration.GetSection("Cdn"));
builder.Services.AddHttpClient("oauth"); // Named HttpClient para llamadas a providers OAuth2
builder.Services.AddScoped<IceBackend.Application.Interfaces.ISessionCache, IceBackend.Infrastructure.Services.RedisSessionCache>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IAuthService, IceBackend.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IStripeWebhookValidator, IceBackend.Infrastructure.Services.StripeWebhookValidator>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IStripeWebhookService, IceBackend.Infrastructure.Services.StripeWebhookService>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IExternalAuthService, IceBackend.Infrastructure.Services.ExternalAuthService>();
builder.Services.AddSingleton<IceBackend.Application.Interfaces.ICdnUrlSigner, IceBackend.Infrastructure.Services.CdnUrlSigner>();
builder.Services.AddScoped<IceBackend.Application.Interfaces.IAssetTokenService, IceBackend.Infrastructure.Services.AssetTokenService>();

// Register Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("db_check");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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