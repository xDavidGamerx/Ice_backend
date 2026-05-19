using System;
using System.Text;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace IceBackend.Infrastructure.Services
{
    /// <summary>
    /// Implementación de ISessionCache respaldada por Redis mediante IDistributedCache.
    /// Esta clase es la única que conoce los detalles de Redis en todo el sistema.
    /// </summary>
    public class RedisSessionCache : ISessionCache
    {
        // Prefijo de clave para evitar colisiones con otros usos de la misma instancia de Redis.
        private const string KeyPrefix = "session:player:";
        private const string TokenPrefix = "session:token:";

        private readonly IDistributedCache _cache;

        public RedisSessionCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetSessionAsync(string playerId, string sessionToken, TimeSpan ttl)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            var tokenBytes = Encoding.UTF8.GetBytes(sessionToken);
            var playerBytes = Encoding.UTF8.GetBytes(playerId);
            
            // Guardamos Player -> Token
            await _cache.SetAsync(BuildKey(playerId), tokenBytes, options);
            // Guardamos Token -> Player
            await _cache.SetAsync($"{TokenPrefix}{sessionToken}", playerBytes, options);
        }

        public async Task<string?> GetSessionAsync(string playerId)
        {
            var tokenBytes = await _cache.GetAsync(BuildKey(playerId));
            return tokenBytes is null ? null : Encoding.UTF8.GetString(tokenBytes);
        }

        public async Task RemoveSessionAsync(string playerId)
        {
            var sessionToken = await GetSessionAsync(playerId);
            await _cache.RemoveAsync(BuildKey(playerId));
            
            if (sessionToken != null)
            {
                await _cache.RemoveAsync($"{TokenPrefix}{sessionToken}");
            }
        }

        public async Task<string?> GetPlayerIdBySessionAsync(string sessionToken)
        {
            var playerBytes = await _cache.GetAsync($"{TokenPrefix}{sessionToken}");
            return playerBytes is null ? null : Encoding.UTF8.GetString(playerBytes);
        }

        private static string BuildKey(string playerId) => $"{KeyPrefix}{playerId}";
    }
}
