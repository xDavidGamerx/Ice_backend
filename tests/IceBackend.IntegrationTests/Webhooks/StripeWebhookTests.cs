using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using IceBackend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;
using Polly;

namespace IceBackend.IntegrationTests.Webhooks
{
    [Collection("IntegrationTests")]
    public class StripeWebhookTests : IAsyncLifetime
    {
        private readonly WebApiFixture _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _db;
        private readonly IConnectionMultiplexer _redis;

        private readonly System.Collections.Generic.List<RedisKey> _trackedKeys = new();

        public StripeWebhookTests(WebApiFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _scope = factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _redis = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        }

        public async Task InitializeAsync()
        {
            // Migrar DB si no lo está (en caso de que Program.cs no lo haga)
            await _db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            var config = _scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            
            // Reset de Postgres
            var pgConn = config["ConnectionStrings:PostgresConnection"];
            await DatabaseRespawn.ResetAsync(pgConn!);

            // Reset de Redis
            var redis = _scope.ServiceProvider.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>();
            var endpoints = redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = redis.GetServer(endpoint);
                await server.FlushAllDatabasesAsync();
            }
            _scope.Dispose();
        }

        private async Task SendWebhookAsync(string payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Webhooks/stripe")
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Stripe-Signature", "t=123,v1=test_signature");
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}");
            }

            // CRÍTICO: Dado que el WebhooksController delega la persistencia a un Task.Run()
            // en background para retornar 200 OK rápido, debemos pausar el hilo del test
            // temporalmente para permitir que la BD se actualice antes del Assert.
            await Task.Delay(1500);
            _db.ChangeTracker.Clear();
        }

        [Fact]
        public async Task C1_InvoicePaid_SubscriptionCreate_ShouldActivateIcePlus()
        {
            // Arrange
            var playerId = new PlayerId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            
            // C1. Jugador con rango antiguo que expiró (simular)
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            player.AssignIcePlusSubscription("sub_old", false, 30, DateTime.UtcNow.AddDays(-40));
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "subscription_create", "invoice.paid");
            await SendWebhookAsync(payload);

            // Active Polling Wait
            bool isActive = false;
            for (int i = 0; i < 60; i++)
            {
                _db.ChangeTracker.Clear();
                var p = await _db.Players.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == playerId);
                if (p?.Subscription != null && p.Subscription.IsActive)
                {
                    isActive = true;
                    break;
                }
                await Task.Delay(500);
            }
            Assert.True(isActive);
            
            var paymentEvent = await _db.PaymentEvents.FirstOrDefaultAsync();
            Assert.NotNull(paymentEvent);
            Assert.Equal("paid", paymentEvent.Status);

            var dbRedis = _redis.GetDatabase();
            Assert.False(await dbRedis.KeyExistsAsync($"subscription:player:{player.Id}"));
            Assert.False(await dbRedis.KeyExistsAsync($"inventory:player:{player.Id}"));
        }

        [Fact]
        public async Task C2_InvoicePaid_SubscriptionCycle_ShouldIncrementMonths()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_002", null));
            player.AssignIcePlusSubscription("sub_test_002", true, 30, DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow); // llega a 1 mes
            player.IncrementIcePlusMonths(DateTime.UtcNow); // llega a 2 meses
            
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_002", "sub_test_002", "subscription_cycle", "invoice.paid");
            await SendWebhookAsync(payload);

            // Static Wait for Background Task
            await Task.Delay(10000);
            _db.ChangeTracker.Clear();
            var dbPlayer = await _db.Players.Include(p => p.Subscription).FirstAsync(p => p.Id == playerId);
            Assert.Equal(3, dbPlayer.Subscription?.AccumulatedMonths);
        }

        [Fact]
        public async Task C3_InvoicePaid_SubscriptionUpdate_ShouldNotIncrementMonths()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_003", null));
            player.AssignIcePlusSubscription("sub_test_003", true, 30, DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow); // llega a 1 mes
            
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_003", "sub_test_003", "subscription_update", "invoice.paid");
            await SendWebhookAsync(payload);

            // Active Polling Wait
            int months = 0;
            for (int i = 0; i < 60; i++)
            {
                _db.ChangeTracker.Clear();
                var p = await _db.Players.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == playerId);
                months = p?.Subscription?.AccumulatedMonths ?? 0;
                if (months == 1) break;
                await Task.Delay(500);
            }
            Assert.Equal(1, months);
        }

        [Fact]
        public async Task C4_InvoicePaymentFailed_ShouldDeactivateSubscription()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_004", null));
            player.AssignIcePlusSubscription("sub_test_004", true, 30, DateTime.UtcNow);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_004", "sub_test_004", "subscription_cycle", "invoice.payment_failed");
            await SendWebhookAsync(payload);

            // Active Polling Wait
            bool isActive = true;
            for (int i = 0; i < 60; i++)
            {
                _db.ChangeTracker.Clear();
                var p = await _db.Players.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == playerId);
                if (p?.Subscription == null || !p.Subscription.IsActive)
                {
                    isActive = false;
                    break;
                }
                await Task.Delay(500);
            }
            Assert.False(isActive);
        }

        [Fact]
        public async Task C5_SubscriptionDeleted_ShouldCancelSubscription()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_005", null));
            player.AssignIcePlusSubscription("sub_test_005", true, 30, DateTime.UtcNow);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_005", "sub_test_005", "cancellation", "customer.subscription.deleted");
            await SendWebhookAsync(payload);

            // Active Polling Wait for background task to complete
            bool isActive = true;
            for (int i = 0; i < 60; i++)
            {
                _db.ChangeTracker.Clear();
                var p = await _db.Players.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == playerId);
                if (p?.Subscription == null || !p.Subscription.IsActive)
                {
                    isActive = false;
                    break;
                }
                await Task.Delay(500);
            }
            Assert.False(isActive, "Subscription should be cancelled");
            
            var paymentEvent = await _db.PaymentEvents.FirstOrDefaultAsync(e => e.Status == "subscription_cancelled");
            Assert.NotNull(paymentEvent);
        }

        [Fact]
        public async Task C6_Idempotency_ShouldRejectDuplicateWebhook()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_006", null));
            player.AssignIcePlusSubscription("sub_old", false, 30, DateTime.UtcNow.AddDays(-40));
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            var explicitEventId = Guid.NewGuid().ToString("N");
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_006", "sub_test_006", "subscription_create", "invoice.paid", explicitEventId);
            
            // Act
            await SendWebhookAsync(payload);

            // Active Polling Wait for background task to persist the event
            int totalEvents = 0;
            for (int i = 0; i < 60; i++)
            {
                _db.ChangeTracker.Clear();
                var eventCount = await _db.PaymentEvents.CountAsync(e => e.ProviderEventId == $"evt_test_{explicitEventId}");
                var unresolvedCount = await _db.UnresolvedPaymentEvents.CountAsync(e => e.ProviderEventId == $"evt_test_{explicitEventId}");
                totalEvents = eventCount + unresolvedCount;
                if (totalEvents >= 1) break;
                await Task.Delay(500);
            }
            Assert.Equal(1, totalEvents);
        }

        [Fact]
        public async Task C7_UnknownCustomer_ShouldCreateUnresolvedEvent()
        {
            // Arrange
            var payload = StripeSignatureHelper.BuildStripePayload("cus_unknown", "sub_test_001", "subscription_create", "invoice.paid");
            await SendWebhookAsync(payload);

            await Task.Delay(10000);
            _db.ChangeTracker.Clear();

            // Active Polling Wait for Unresolved
            UnresolvedPaymentEvent? unresolved = null;
            for (int i = 0; i < 60; i++)
            {
                _db.ChangeTracker.Clear();
                unresolved = await _db.UnresolvedPaymentEvents.FirstOrDefaultAsync();
                if (unresolved != null) break;
                await Task.Delay(500);
            }
            Assert.NotNull(unresolved);
            Assert.Equal("PENDING_RESOLUTION", unresolved.Status);
        }
    }
}
