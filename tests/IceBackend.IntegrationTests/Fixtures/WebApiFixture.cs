using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
namespace IceBackend.IntegrationTests.Fixtures
{
    [CollectionDefinition("IntegrationTests")]
    public class IntegrationTestsCollection : ICollectionFixture<WebApiFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class WebApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public PostgreSqlFixture DbFixture { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    { "ConnectionStrings:PostgresConnection", DbFixture.PostgresConnectionString },
                    { "ConnectionStrings:RedisConnection", DbFixture.RedisConnectionString },
                    { "Stripe:WebhookSecret", "whsec_test_secret" },
                    { "Stripe:SecretKey", "sk_test_fake" },
                    { "ASPNETCORE_ENVIRONMENT", "Testing" }
                };

                config.AddInMemoryCollection(dict);
            });
            
            // Setting the environment to testing avoids running some development only code
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IStripeWebhookValidator>();
                services.AddSingleton<IStripeWebhookValidator, FakeStripeWebhookValidator>();
            });
        }

        public async Task InitializeAsync()
        {
            await DbFixture.InitializeAsync();

            // Establecer variables de entorno de proceso para pasar validaciones Fail-Fast en Program.cs
            Environment.SetEnvironmentVariable("ConnectionStrings__PostgresConnection", DbFixture.PostgresConnectionString);
            Environment.SetEnvironmentVariable("ConnectionStrings__RedisConnection", DbFixture.RedisConnectionString);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        public new async Task DisposeAsync()
        {
            // Limpiar variables de entorno
            Environment.SetEnvironmentVariable("ConnectionStrings__PostgresConnection", null);
            Environment.SetEnvironmentVariable("ConnectionStrings__RedisConnection", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

            await DbFixture.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    public class FakeStripeWebhookValidator : IStripeWebhookValidator
    {
        public IceBackend.Application.DTOs.WebhookEventDto? ValidateAndConstruct(string rawBody, string signatureHeader)
        {
            var stripeEvent = Stripe.EventUtility.ParseEvent(rawBody, throwOnApiVersionMismatch: false);
            return new IceBackend.Application.DTOs.WebhookEventDto
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                DataObjectJson = System.Text.Json.JsonSerializer.Serialize(stripeEvent.Data.Object),
                Created = stripeEvent.Created
            };
        }
    }
}
