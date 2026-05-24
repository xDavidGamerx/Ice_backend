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
            // Limpiar BD y Redis tras cada test
            var config = _scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var pgConn = config["ConnectionStrings:PostgresConnection"];
            var redisConn = config["ConnectionStrings:RedisConnection"];
            await DatabaseRespawn.ResetAsync(pgConn!);
            await DatabaseRespawn.DeleteKeysAtomicsAsync(redisConn!, _trackedKeys.ToArray());
            _scope.Dispose();
        }

        private async Task SendWebhookAsync(string payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/webhooks/stripe");
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            // Nota: FakeStripeWebhookValidator asume el bypass criptográfico, no requiere Stripe-Signature real.
            
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task C1_InvoicePaid_SubscriptionCreate_ShouldActivateIcePlus()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            player.AssignIcePlusSubscription("sub_old", false, -1, DateTime.UtcNow.AddDays(-10)); // expirada/inactiva
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "subscription_create", "invoice.paid");
            await SendWebhookAsync(payload);

            // Assert
            var dbPlayer = await _db.Players.Include(p => p.Subscription).FirstAsync();
            Assert.True(dbPlayer.Subscription?.IsActive);
            Assert.Equal(1, dbPlayer.Subscription?.AccumulatedMonths);
            Assert.Equal("sub_test_001", dbPlayer.Subscription?.StripeSubscriptionId);
            
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
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            player.AssignIcePlusSubscription("sub_test_001", true, 30, DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow); // llega a 1 mes
            player.IncrementIcePlusMonths(DateTime.UtcNow); // llega a 2 meses
            
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "subscription_cycle", "invoice.paid");
            await SendWebhookAsync(payload);

            // Assert
            var dbPlayer = await _db.Players.Include(p => p.Subscription).FirstAsync();
            Assert.Equal(3, dbPlayer.Subscription?.AccumulatedMonths);
        }

        [Fact]
        public async Task C3_InvoicePaid_SubscriptionUpdate_ShouldNotIncrementMonths()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            player.AssignIcePlusSubscription("sub_test_001", true, 30, DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow); // llega a 1 mes
            
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "subscription_update", "invoice.paid");
            await SendWebhookAsync(payload);

            // Assert
            var dbPlayer = await _db.Players.Include(p => p.Subscription).FirstAsync();
            Assert.Equal(1, dbPlayer.Subscription?.AccumulatedMonths); // No incrementa
        }

        [Fact]
        public async Task C4_InvoicePaymentFailed_ShouldDeactivateSubscription()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            player.AssignIcePlusSubscription("sub_test_001", true, 30, DateTime.UtcNow);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "subscription_cycle", "invoice.payment_failed");
            await SendWebhookAsync(payload);

            // Assert
            var dbPlayer = await _db.Players.Include(p => p.Subscription).FirstAsync();
            Assert.False(dbPlayer.Subscription?.IsActive);
        }

        [Fact]
        public async Task C5_SubscriptionDeleted_ShouldCancelSubscription()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            _trackedKeys.AddRange(new RedisKey[] { $"subscription:player:{playerId}", $"player:sessions:{playerId}", $"inventory:player:{playerId}" });
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            player.AssignIcePlusSubscription("sub_test_001", true, 30, DateTime.UtcNow);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "cancellation", "customer.subscription.deleted");
            await SendWebhookAsync(payload);

            // Assert
            var dbPlayer = await _db.Players.Include(p => p.Subscription).FirstAsync();
            Assert.False(dbPlayer.Subscription?.IsActive);
            
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
            player.AddExternalAuth(new ExternalAuth(Guid.NewGuid(), playerId, AuthProvider.STRIPE, "cus_test_001", null));
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_001", "sub_test_001", "subscription_create", "invoice.paid");
            
            // Act
            await SendWebhookAsync(payload);
            await SendWebhookAsync(payload); // Envío duplicado

            // Assert
            var eventCount = await _db.PaymentEvents.CountAsync();
            Assert.Equal(1, eventCount); // Solo se persiste una vez
        }

        [Fact]
        public async Task C7_UnknownCustomer_ShouldCreateUnresolvedEvent()
        {
            // Act
            var payload = StripeSignatureHelper.BuildStripePayload("cus_unknown", "sub_test_001", "subscription_create", "invoice.paid");
            await SendWebhookAsync(payload);

            // Assert
            var unresolved = await _db.UnresolvedPaymentEvents.FirstOrDefaultAsync();
            Assert.NotNull(unresolved);
            Assert.Equal("player_not_found", unresolved.Status);
            
            var resolvedCount = await _db.PaymentEvents.CountAsync();
            Assert.Equal(0, resolvedCount);
        }
    }
}
