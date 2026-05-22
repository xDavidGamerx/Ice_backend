using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace IceBackend.Infrastructure.Services
{
    /// <summary>
    /// Implementación de ISessionCache respaldada por Redis.
    /// Combina IDistributedCache para sesiones y IConnectionMultiplexer para Sets de inventario (SISMEMBER).
    /// Implementa patrón Cache-Aside para evitar hits innecesarios a PostgreSQL.
    /// </summary>
    public class RedisSessionCache : ISessionCache
    {
        private const string KeyPrefix = "session:player:";
        private const string TokenPrefix = "session:token:";
        private const string DevKeyPrefix = "dev_session:player:";
        private const string DevTokenPrefix = "dev_session:token:";
        private const string InventoryPrefix = "inventory:player:";
        private const string HashMappingPrefix = "asset:hash:";
        
        // TTL para el inventario en caché (1 hora por defecto, se refresca con actividad)
        private readonly TimeSpan _inventoryTtl = TimeSpan.FromHours(1);
        // TTL para mapeo de hashes (24 horas, es casi inmutable)
        private readonly TimeSpan _hashMappingTtl = TimeSpan.FromDays(1);

        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ApplicationDbContext _dbContext;

        public RedisSessionCache(
            IDistributedCache cache, 
            IConnectionMultiplexer redis,
            ApplicationDbContext dbContext)
        {
            _cache = cache;
            _redis = redis;
            _dbContext = dbContext;
        }

        public async Task SetSessionAsync(string playerId, string sessionToken, TimeSpan ttl)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
            var tokenBytes = Encoding.UTF8.GetBytes(sessionToken);
            var playerBytes = Encoding.UTF8.GetBytes(playerId);
            
            await _cache.SetAsync(BuildKey(playerId), tokenBytes, options);
            await _cache.SetAsync($"{TokenPrefix}{sessionToken}", playerBytes, options);
        }

        public async Task<string?> GetSessionAsync(string playerId)
        {
            var tokenBytes = await _cache.GetAsync(BuildKey(playerId));
            return tokenBytes is null ? null : Encoding.UTF8.GetString(tokenBytes);
        }

        public async Task RemoveSessionAsync(string playerId)
        {
            // Limpiar sesión real
            var sessionToken = await GetSessionAsync(playerId);
            await _cache.RemoveAsync(BuildKey(playerId));
            if (sessionToken != null) await _cache.RemoveAsync($"{TokenPrefix}{sessionToken}");

            // Limpiar sesión de desarrollo si existe
            await RemoveDevSessionAsync(playerId);

            await InvalidatePlayerCosmeticsAsync(Guid.Parse(playerId));
        }

        public async Task<string?> GetPlayerIdBySessionAsync(string sessionToken)
        {
            // 1. Intentar sesión real
            var playerBytes = await _cache.GetAsync($"{TokenPrefix}{sessionToken}");
            if (playerBytes != null) return Encoding.UTF8.GetString(playerBytes);

            // 2. Fallback a sesión de desarrollo (dev_session:)
            playerBytes = await _cache.GetAsync($"{DevTokenPrefix}{sessionToken}");
            return playerBytes is null ? null : Encoding.UTF8.GetString(playerBytes);
        }

        public async Task<bool> IsCosmeticOwnedAsync(Guid playerId, Guid cosmeticId)
        {
            var db = _redis.GetDatabase();
            var key = $"{InventoryPrefix}{playerId}";

            // 1. Intentar SISMEMBER en Redis
            if (await db.KeyExistsAsync(key))
            {
                return await db.SetContainsAsync(key, cosmeticId.ToString());
            }

            // 2. Cache Miss: Hidratar desde PostgreSQL (Pattern: Cache-Aside)
            var ownedIds = await _dbContext.PlayerCosmeticOwnerships
                .Where(o => o.PlayerId == playerId)
                .Select(o => o.CosmeticId.ToString())
                .ToListAsync();

            if (ownedIds.Any())
            {
                var values = ownedIds.Select(id => (RedisValue)id).ToArray();
                await db.SetAddAsync(key, values);
                await db.KeyExpireAsync(key, _inventoryTtl);
            }
            else
            {
                // Para evitar "Cache Penetration" con IDs inexistentes, 
                // guardamos un set vacío con TTL corto si no tiene nada.
                await db.SetAddAsync(key, "NONE"); 
                await db.KeyExpireAsync(key, TimeSpan.FromMinutes(5));
            }

            return ownedIds.Contains(cosmeticId.ToString());
        }

        public async Task InvalidatePlayerCosmeticsAsync(Guid playerId)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{InventoryPrefix}{playerId}");
        }

        public async Task<Guid?> GetCosmeticIdByHashAsync(string hash)
        {
            var db = _redis.GetDatabase();
            var key = $"{HashMappingPrefix}{hash}";

            // 1. Intentar GET en Redis
            var cachedId = await db.StringGetAsync(key);
            if (cachedId.HasValue)
            {
                return Guid.Parse(cachedId!);
            }

            // 2. Cache Miss: Resolver desde PostgreSQL
            var assetId = await _dbContext.CosmeticAssetVersions
                .Where(v => v.Sha256Hash == hash)
                .Select(v => (Guid?)v.CosmeticAssetId)
                .FirstOrDefaultAsync();

            if (assetId.HasValue)
            {
                await db.StringSetAsync(key, assetId.Value.ToString(), _hashMappingTtl);
            }

            return assetId;
        }

        public async Task SetDevSessionAsync(string playerId, string sessionToken, TimeSpan ttl)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
            await _cache.SetAsync($"{DevKeyPrefix}{playerId}", Encoding.UTF8.GetBytes(sessionToken), options);
            await _cache.SetAsync($"{DevTokenPrefix}{sessionToken}", Encoding.UTF8.GetBytes(playerId), options);
        }

        public async Task RemoveDevSessionAsync(string playerId)
        {
            var tokenBytes = await _cache.GetAsync($"{DevKeyPrefix}{playerId}");
            if (tokenBytes != null)
            {
                var token = Encoding.UTF8.GetString(tokenBytes);
                await _cache.RemoveAsync($"{DevTokenPrefix}{token}");
            }
            await _cache.RemoveAsync($"{DevKeyPrefix}{playerId}");
        }

        private static string BuildKey(string playerId) => $"{KeyPrefix}{playerId}";
    }
}
