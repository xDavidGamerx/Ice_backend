using System;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using IceBackend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace IceBackend.IntegrationTests.Cache
{
    [Collection("IntegrationTests")]
    public class CacheConsistencyTests : IAsyncLifetime
    {
        private readonly WebApiFixture _factory;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _db;
        private readonly IConnectionMultiplexer _redis;
        private readonly ISessionCache _sessionCache;

        private readonly System.Collections.Generic.List<RedisKey> _trackedKeys = new();

        public CacheConsistencyTests(WebApiFixture factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _redis = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            _sessionCache = _scope.ServiceProvider.GetRequiredService<ISessionCache>();
        }

        public async Task InitializeAsync()
        {
            await _db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            var config = _scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var pgConn = config["ConnectionStrings:PostgresConnection"];
            var redisConn = config["ConnectionStrings:RedisConnection"];
            await DatabaseRespawn.ResetAsync(pgConn!);
            await DatabaseRespawn.DeleteKeysAtomicsAsync(redisConn!, _trackedKeys.ToArray());
            _scope.Dispose();
        }

        [Fact]
        public async Task D1_IsCosmeticOwnedAsync_ShouldSetEmptyFlag_OnCacheMiss()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();
            var cosmeticId = Guid.NewGuid();
            _trackedKeys.Add($"inventory:player:{player.Id}:empty_flag");

            // Act - Cache Miss
            var owned = await _sessionCache.IsCosmeticOwnedAsync(player.Id, cosmeticId);

            // Assert
            Assert.False(owned);
            var dbRedis = _redis.GetDatabase();
            var emptyFlagExists = await dbRedis.KeyExistsAsync($"inventory:player:{player.Id}:empty_flag");
            Assert.True(emptyFlagExists, "El empty_flag debe persistirse tras un Cache Miss para mitigar Cache Penetration.");
            var ttl = await dbRedis.KeyTimeToLiveAsync($"inventory:player:{player.Id}:empty_flag");
            Assert.True(ttl.HasValue && ttl.Value.TotalSeconds <= 30);
        }

        [Fact]
        public async Task D2_GetIcePlusBenefitsAsync_ShouldSetNullSentinel_OnCacheMiss()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();
            _trackedKeys.Add($"subscription:player:{player.Id}");

            // Act - Cache Miss
            var benefits = await _sessionCache.GetIcePlusBenefitsAsync(player.Id);

            // Assert
            Assert.Null(benefits);
            var dbRedis = _redis.GetDatabase();
            var sentinelData = await dbRedis.StringGetAsync($"subscription:player:{player.Id}");
            Assert.True(sentinelData.HasValue);
            Assert.Contains("\"IsNull\":true", sentinelData.ToString());
            
            var ttl = await dbRedis.KeyTimeToLiveAsync($"subscription:player:{player.Id}");
            Assert.True(ttl.HasValue && ttl.Value.TotalSeconds <= 30);
        }

        [Fact]
        public async Task D3_PurgePlayerSessionsAsync_ShouldDeleteAllKeys()
        {
            // Arrange
            var playerId = new PlayerId(Guid.NewGuid());
            var player = new Player(playerId, "test_user", UuidType.ICE, null);
            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            var token1 = Guid.NewGuid().ToString();
            var token2 = Guid.NewGuid().ToString();

            await _sessionCache.SetSessionAsync(token1, player.Id.ToString(), TimeSpan.FromHours(1));
            await _sessionCache.SetSessionAsync(token2, player.Id.ToString(), TimeSpan.FromHours(1));

            // Simular datos de suscripción e inventario en caché
            var dbRedis = _redis.GetDatabase();
            await dbRedis.StringSetAsync($"subscription:player:{player.Id}", "dummy_data", TimeSpan.FromSeconds(60));
            await dbRedis.SetAddAsync($"inventory:player:{player.Id}", Guid.NewGuid().ToString());
            await dbRedis.KeyExpireAsync($"inventory:player:{player.Id}", TimeSpan.FromSeconds(60));

            _trackedKeys.AddRange(new RedisKey[] {
                $"session:token:{token1}", 
                $"session:token:{token2}", 
                $"player:sessions:{player.Id}",
                $"subscription:player:{player.Id}",
                $"inventory:player:{player.Id}"
            });

            // Act
            await _sessionCache.PurgePlayerSessionsAsync(player.Id.Value);

            // Assert
            Assert.False(await dbRedis.KeyExistsAsync($"session:token:{token1}"));
            Assert.False(await dbRedis.KeyExistsAsync($"session:token:{token2}"));
            Assert.False(await dbRedis.KeyExistsAsync($"player:sessions:{player.Id}"));
            
            // Suscripción se limpia automáticamente
            Assert.False(await dbRedis.KeyExistsAsync($"subscription:player:{player.Id}"));
            
            // Nota: inventory:player no se limpia con PurgePlayerSessionsAsync porque el logout
            // no quita los cosméticos del usuario. Se invalida aparte si cambia la sub.
        }
    }
}
