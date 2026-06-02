using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        // Prefijo configurado en AddStackExchangeRedisCache(InstanceName = "IceLauncher:")
        private const string RedisInstancePrefix = "IceLauncher:";
        private const string KeyPrefix = "session:player:";
        private const string TokenPrefix = "session:token:";
        private const string DevKeyPrefix = "dev_session:player:";
        private const string DevTokenPrefix = "dev_session:token:";
        private const string InventoryPrefix = "inventory:player:";
        private const string HashMappingPrefix = "asset:hash:";
        private const string SubscriptionPrefix = "subscription:player:";
        private const string PwdResetPrefix = "pwd_reset:";
        
        // TTL para el inventario en caché (1 hora por defecto, se refresca con actividad)
        private readonly TimeSpan _inventoryTtl = TimeSpan.FromHours(1);
        // TTL para mapeo de hashes (24 horas, es casi inmutable)
        private readonly TimeSpan _hashMappingTtl = TimeSpan.FromDays(1);
        // TTL para la suscripción en caché (1 hora)
        private readonly TimeSpan _subscriptionTtl = TimeSpan.FromHours(1);

        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ApplicationDbContext _dbContext;

        // Script Lua precargado de forma estática en el Connection Multiplexer (EVALSHA optimizado)
        private static readonly LuaScript PurgeLuaScript = LuaScript.Prepare(@"
            -- 1. Eliminar referencias de sesiones ya expiradas del Sorted Set
            redis.call('ZREMRANGEBYSCORE', KEYS[1], 0, ARGV[1])

            -- 2. Obtener la lista de tokens activos remanentes
            local activeTokens = redis.call('ZRANGE', KEYS[1], 0, -1)

            -- 3. Eliminar de forma asíncrona cada clave de sesión física usando UNLINK
            for i, token in ipairs(activeTokens) do
                redis.call('UNLINK', ARGV[2] .. token)
            end

            -- 4. Eliminar el Sorted Set secundario de índice
            redis.call('UNLINK', KEYS[1])

            return #activeTokens");

        private static readonly LuaScript RemoveByHashLuaScript = LuaScript.Prepare(@"
            -- KEYS[1] = player:sessions:{playerId}
            -- ARGV[1] = tokenHash buscado (SHA-1 en minúsculas)
            -- ARGV[2] = ""IceLauncher:session:token:"" (prefijo para token keys)
            local members = redis.call('ZRANGE', KEYS[1], 0, -1)
            for i, token in ipairs(members) do
                if redis.sha1hex(token) == ARGV[1] then
                    redis.call('ZREM', KEYS[1], token)
                    redis.call('UNLINK', ARGV[2] .. token)
                    return token
                end
            end
            return false");

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

            // Registrar la sesión en el Sorted Set de control del jugador (con expiración Unix como score)
            var db = _redis.GetDatabase();
            var sessionsIndexKey = $"player:sessions:{playerId}";
            double expireScore = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds();
            await db.SortedSetAddAsync(sessionsIndexKey, sessionToken, expireScore);
        }

        public async Task<string?> GetSessionAsync(string playerId)
        {
            var tokenBytes = await _cache.GetAsync(BuildKey(playerId));
            return tokenBytes is null ? null : Encoding.UTF8.GetString(tokenBytes);
        }

        public async Task RemoveSessionAsync(string playerId)
        {
            var db = _redis.GetDatabase();

            // 1. Leer token via IDistributedCache (respeta prefijo y formato HASH)
            var sessionToken = await GetSessionAsync(playerId);

            // 2. Reunir todas las llaves a invalidar en un lote único
            //    Nota: IDistributedCache almacena con RedisInstancePrefix y formato HASH.
            //    Para borrar usamos KeyDeleteAsync directo con el nombre completo de la key.
            var keysToDelete = new List<RedisKey>
            {
                $"{RedisInstancePrefix}{KeyPrefix}{playerId}",
                $"{RedisInstancePrefix}{DevKeyPrefix}{playerId}",
                $"{InventoryPrefix}{playerId}",
                $"{InventoryPrefix}{playerId}:empty_flag",
                $"{SubscriptionPrefix}{playerId}",
                $"player:sessions:{playerId}"
            };

            if (sessionToken != null)
            {
                keysToDelete.Add($"{RedisInstancePrefix}{TokenPrefix}{sessionToken}");
                keysToDelete.Add($"{RedisInstancePrefix}{DevTokenPrefix}{sessionToken}");
            }

            // 3. Ejecutar borrado masivo por lotes en Redis (eficiente y atómico)
            await db.KeyDeleteAsync(keysToDelete.ToArray());
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
            var emptyFlagKey = $"{InventoryPrefix}{playerId}:empty_flag";

            // 1. Pipeline asíncrono para consultar Set principal y Flag simultáneamente
            var keyExistsTask = db.KeyExistsAsync(key);
            var emptyFlagExistsTask = db.KeyExistsAsync(emptyFlagKey);

            await Task.WhenAll(keyExistsTask, emptyFlagExistsTask);

            if (keyExistsTask.Result)
            {
                return await db.SetContainsAsync(key, cosmeticId.ToString());
            }

            if (emptyFlagExistsTask.Result)
            {
                return false; // Sabemos con certeza que no tiene nada
            }

            // 2. Cache Miss: Hidratar desde PostgreSQL (Pattern: Cache-Aside)
            var ownedIds = await _dbContext.PlayerCosmeticOwnerships
                .Where(o => o.PlayerId == new IceBackend.Domain.Entities.PlayerId(playerId))
                .Select(o => o.Cosmetic.Id)
                .ToListAsync();

            var ownedIdStrings = ownedIds.Select(id => id.Value.ToString()).ToList();

            if (ownedIdStrings.Any())
            {
                var values = ownedIdStrings.Select(id => (RedisValue)id).ToArray();
                await db.SetAddAsync(key, values);
                await db.KeyExpireAsync(key, _inventoryTtl);
            }
            else
            {
                // Mitigar Cache Penetration sin ensuciar el Set
                await db.StringSetAsync(emptyFlagKey, "1", TimeSpan.FromSeconds(30));
            }

            return ownedIdStrings.Contains(cosmeticId.ToString());
        }

        public async Task InvalidatePlayerCosmeticsAsync(Guid playerId)
        {
            var db = _redis.GetDatabase();
            var key = $"{InventoryPrefix}{playerId}";
            var emptyFlagKey = $"{InventoryPrefix}{playerId}:empty_flag";
            await db.KeyDeleteAsync(new RedisKey[] { key, emptyFlagKey });
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
                .Select(v => (Guid?)v.CosmeticAsset.Id) // Referenciar la clave UUID de dominio
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
            var db = _redis.GetDatabase();
            var keysToDelete = new List<RedisKey>
            {
                $"{RedisInstancePrefix}{DevKeyPrefix}{playerId}"
            };

            var tokenBytes = await _cache.GetAsync($"{DevKeyPrefix}{playerId}");
            if (tokenBytes != null)
            {
                var token = Encoding.UTF8.GetString(tokenBytes);
                keysToDelete.Add($"{RedisInstancePrefix}{DevTokenPrefix}{token}");
            }

            await db.KeyDeleteAsync(keysToDelete.ToArray());
        }

        public async Task PurgePlayerSessionsAsync(Guid playerId)
        {
            var db = _redis.GetDatabase();
            var sessionsIndexKey = $"player:sessions:{playerId}";
            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await db.ScriptEvaluateAsync(PurgeLuaScript.ExecutableScript,
                keys: new RedisKey[] { sessionsIndexKey },
                values: new RedisValue[] { nowUnix, $"{RedisInstancePrefix}{TokenPrefix}" });

            await InvalidatePlayerSubscriptionAsync(playerId);
        }

        public async Task<IceBackend.Domain.Services.IcePlusBenefits?> GetIcePlusBenefitsAsync(Guid playerId)
        {
            var db = _redis.GetDatabase();
            var key = $"{SubscriptionPrefix}{playerId}";

            // 1. Intentar GET en Redis
            var cachedValue = await db.StringGetAsync(key);
            if (cachedValue.HasValue)
            {
                if (cachedValue == "{\"IsNull\":true}") return null;
                try
                {
                    return JsonSerializer.Deserialize<IceBackend.Domain.Services.IcePlusBenefits>(cachedValue!);
                }
                catch
                {
                    // Ante cualquier error de deserialización, ignorar caché y rehidratar
                }
            }

            // 2. Cache Miss: Rehidratar desde PostgreSQL (Cache-Aside)
            var subscription = await _dbContext.PlayerSubscriptions
                .FirstOrDefaultAsync(s => s.PlayerId == new IceBackend.Domain.Entities.PlayerId(playerId));

            if (subscription == null || !subscription.IsActive || (subscription.ExpiresAt.HasValue && subscription.ExpiresAt.Value < DateTime.UtcNow))
            {
                // Cache Penetration Protection: Guardar JSON con TTL corto
                await db.StringSetAsync(key, "{\"IsNull\":true}", TimeSpan.FromSeconds(30));
                return null;
            }

            // Obtener los beneficios y serializarlos
            var benefits = IceBackend.Domain.Services.IcePlusBenefitsProvider.GetBenefits(subscription.AccumulatedMonths);
            var serialized = JsonSerializer.Serialize(benefits);

            // Calcular TTL óptimo (el tiempo restante de la suscripción, limitado al TTL máximo de caché)
            var ttl = _subscriptionTtl;
            if (subscription.ExpiresAt.HasValue)
            {
                var remaining = subscription.ExpiresAt.Value - DateTime.UtcNow;
                if (remaining < ttl)
                {
                    ttl = remaining > TimeSpan.Zero ? remaining : TimeSpan.FromSeconds(5);
                }
            }

            await db.StringSetAsync(key, serialized, ttl);
            return benefits;
        }

        public async Task InvalidatePlayerSubscriptionAsync(Guid playerId)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{SubscriptionPrefix}{playerId}");
        }

        public async Task SetPasswordResetTokenAsync(string playerId, string token, TimeSpan ttl)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync($"{PwdResetPrefix}{token}", playerId, ttl);
        }

        public async Task<string?> GetPasswordResetTokenAsync(string token)
        {
            var db = _redis.GetDatabase();
            var result = await db.StringGetAsync($"{PwdResetPrefix}{token}");
            return result.HasValue ? result.ToString() : null;
        }

        public async Task InvalidatePasswordResetTokenAsync(string token)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{PwdResetPrefix}{token}");
        }

        public async Task<List<SessionInfo>> GetSessionsAsync(string playerId)
        {
            var db = _redis.GetDatabase();
            var sessionsIndexKey = $"player:sessions:{playerId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var entries = await db.SortedSetRangeByScoreWithScoresAsync(
                sessionsIndexKey, now, double.PositiveInfinity, Exclude.None, Order.Ascending);

            // Leer token actual para marcar IsCurrent
            var currentToken = await GetSessionAsync(playerId);

            return entries.Select(e =>
            {
                var token = e.Element.ToString();
                var tokenHash = ComputeSha1(token);
                return new SessionInfo(tokenHash, (long)e.Score, token == currentToken);
            }).ToList();
        }

        public async Task<bool> RemoveSessionByTokenHashAsync(string playerId, string tokenHash)
        {
            var db = _redis.GetDatabase();
            var sessionsIndexKey = $"player:sessions:{playerId}";
            var tokenPrefix = $"{RedisInstancePrefix}{TokenPrefix}";

            var result = await db.ScriptEvaluateAsync(RemoveByHashLuaScript.ExecutableScript,
                keys: new RedisKey[] { sessionsIndexKey },
                values: new RedisValue[] { tokenHash, tokenPrefix });

            if (result.IsNull) return false;

            var deletedToken = result.ToString();

            // Si el token eliminado es la sesión actual, también limpiar session:player:{id}
            var currentToken = await GetSessionAsync(playerId);
            if (currentToken == deletedToken)
            {
                await db.KeyDeleteAsync($"{RedisInstancePrefix}{KeyPrefix}{playerId}");
                await db.KeyDeleteAsync($"{RedisInstancePrefix}{DevKeyPrefix}{playerId}");
                await db.KeyDeleteAsync($"{InventoryPrefix}{playerId}");
                await db.KeyDeleteAsync($"{InventoryPrefix}{playerId}:empty_flag");
                await db.KeyDeleteAsync($"{SubscriptionPrefix}{playerId}");
            }

            return true;
        }

        private static string ComputeSha1(string input)
        {
            var bytes = System.Security.Cryptography.SHA1.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static string BuildKey(string playerId) => $"{KeyPrefix}{playerId}";
    }
}
