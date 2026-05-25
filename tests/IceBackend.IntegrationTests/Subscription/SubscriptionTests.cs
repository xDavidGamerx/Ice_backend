using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using IceBackend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace IceBackend.IntegrationTests.Subscription
{
    [Collection("IntegrationTests")]
    public class SubscriptionTests : IAsyncLifetime
    {
        private readonly WebApiFixture _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _db;
        private readonly IConnectionMultiplexer _redis;

        public SubscriptionTests(WebApiFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _scope = factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _redis = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        }

        public async Task InitializeAsync()
        {
            await _db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            var config = _scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            
            // Reset de Postgres
            var pgConn = config["ConnectionStrings:PostgresConnection"];
            await DatabaseRespawn.ResetAsync(pgConn!);

            // Reset de Redis
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                await server.FlushAllDatabasesAsync();
            }
            _scope.Dispose();
        }

        private async Task AuthenticateSessionAsync(Guid playerId, string token)
        {
            var sessionCache = _scope.ServiceProvider.GetRequiredService<IceBackend.Application.Interfaces.ISessionCache>();
            await sessionCache.SetSessionAsync(playerId.ToString(), token, TimeSpan.FromHours(1));
        }

        [Fact]
        public async Task GetMySubscription_ActiveSubscription_ShouldReturnActiveDetailsAndBenefits()
        {
            // Arrange
            var playerId = new PlayerId(Guid.Parse("00000000-0000-0000-0000-000000000101"));
            var player = new Player(playerId, "active_user", UuidType.ICE, null);
            player.AssignIcePlusSubscription("sub_active_test", false, 30, DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow);
            player.IncrementIcePlusMonths(DateTime.UtcNow); // 3 meses acumulados

            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            // Sembramos también los beneficios calculados en la caché (Redis)
            var sessionCache = _scope.ServiceProvider.GetRequiredService<IceBackend.Application.Interfaces.ISessionCache>();
            var benefitsBefore = await sessionCache.GetIcePlusBenefitsAsync(playerId.Value); // Dispara Cache-Aside y escribe en Redis

            var sessionToken = "session_token_active_user";
            await AuthenticateSessionAsync(playerId.Value, sessionToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);

            // Act
            var response = await _client.GetAsync("/api/v1/subscription/me");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            Assert.True(root.GetProperty("isActive").GetBoolean());
            Assert.Equal(3, root.GetProperty("accumulatedMonths").GetInt32());
            Assert.Equal("sub_active_test", root.GetProperty("stripeSubscriptionId").GetString());
            Assert.False(root.GetProperty("autoRenew").GetBoolean());
            Assert.NotNull(root.GetProperty("expiresAt").GetString());

            var benefitsProp = root.GetProperty("benefits");
            Assert.Equal(JsonValueKind.Object, benefitsProp.ValueKind);
            Assert.Equal("[ICE+]", benefitsProp.GetProperty("prefix").GetString());
            Assert.Equal("#FF69B4", benefitsProp.GetProperty("color").GetString());
            Assert.Equal("green", benefitsProp.GetProperty("iconColor").GetString());
            Assert.True(benefitsProp.GetProperty("noAds").GetBoolean());
            Assert.True(benefitsProp.GetProperty("unlimitedFriends").GetBoolean());
        }

        [Fact]
        public async Task GetMySubscription_NoSubscription_ShouldReturnInactiveDetailsAndNullBenefits()
        {
            // Arrange
            var playerId = new PlayerId(Guid.Parse("00000000-0000-0000-0000-000000000102"));
            var player = new Player(playerId, "inactive_user", UuidType.ICE, null);

            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            var sessionToken = "session_token_inactive_user";
            await AuthenticateSessionAsync(playerId.Value, sessionToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);

            // Act
            var response = await _client.GetAsync("/api/v1/subscription/me");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            Assert.False(root.GetProperty("isActive").GetBoolean());
            Assert.Equal(0, root.GetProperty("accumulatedMonths").GetInt32());
            Assert.Null(root.GetProperty("stripeSubscriptionId").GetString());
            Assert.False(root.GetProperty("autoRenew").GetBoolean());
            Assert.Null(root.GetProperty("expiresAt").GetString());
            Assert.Equal(JsonValueKind.Null, root.GetProperty("benefits").ValueKind);
        }

        [Fact]
        public async Task GetMySubscription_Unauthenticated_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/subscription/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
