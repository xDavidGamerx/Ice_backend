using System.Threading.Tasks;

namespace IceBackend.Application.Interfaces
{
    /// <summary>
    /// Abstracción de caché de sesiones de jugador.
    /// La capa Application solo conoce esta interfaz; la implementación concreta (Redis)
    /// vive exclusivamente en Infrastructure, manteniendo el Domain y Application libres.
    /// </summary>
    public interface ISessionCache
    {
        /// <summary>
        /// Persiste un token de sesión asociado al UUID del jugador con un TTL dado.
        /// </summary>
        Task SetSessionAsync(string playerId, string sessionToken, System.TimeSpan ttl);

        /// <summary>
        /// Recupera el token de sesión activo para un jugador, o null si expiró/no existe.
        /// </summary>
        Task<string?> GetSessionAsync(string playerId);

        /// <summary>
        /// Invalida la sesión de un jugador (logout / revocación).
        /// </summary>
        Task RemoveSessionAsync(string playerId);

        /// <summary>
        /// Obtiene el PlayerId asociado a un SessionToken específico.
        /// </summary>
        Task<string?> GetPlayerIdBySessionAsync(string sessionToken);

        /// <summary>
        /// Verifica si un jugador posee un cosmético específico. 
        /// Debe implementar patrón Cache-Aside para consultar DB si la clave no existe en Redis.
        /// </summary>
        Task<bool> IsCosmeticOwnedAsync(Guid playerId, Guid cosmeticId);

        /// <summary>
        /// Invalida el conjunto de cosméticos cacheados de un jugador (Stripe Sync).
        /// </summary>
        Task InvalidatePlayerCosmeticsAsync(Guid playerId);

        /// <summary>
        /// Resuelve un hash SHA256 a un UUID de CosmeticAsset.
        /// Debe implementar patrón Cache-Aside.
        /// </summary>
        Task<Guid?> GetCosmeticIdByHashAsync(string hash);

        /// <summary>
        /// Persiste un token de sesión de desarrollo con prefijo aislado (dev_session:).
        /// No debe interferir con sesiones reales de OAuth2.
        /// </summary>
        Task SetDevSessionAsync(string playerId, string sessionToken, TimeSpan ttl);

        /// <summary>
        /// Remueve una sesión de desarrollo y sus claves inversas en Redis.
        /// </summary>
        Task RemoveDevSessionAsync(string playerId);

        /// <summary>
        /// Purgado atómico de todas las sesiones de un jugador en Redis (ZSET autolimpiable con script Lua).
        /// </summary>
        Task PurgePlayerSessionsAsync(Guid playerId);

        /// <summary>
        /// Obtiene los beneficios activos de la suscripción ICE+ del jugador mediante Cache-Aside.
        /// Retorna null si la suscripción no está activa.
        /// </summary>
        Task<IceBackend.Domain.Services.IcePlusBenefits?> GetIcePlusBenefitsAsync(Guid playerId);

        /// <summary>
        /// Invalida la clave de caché de la suscripción del jugador en Redis.
        /// </summary>
        Task InvalidatePlayerSubscriptionAsync(Guid playerId);

        /// <summary>
        /// Almacena el token de recuperación de contraseña asociado a un jugador con un TTL.
        /// </summary>
        Task SetPasswordResetTokenAsync(string playerId, string token, System.TimeSpan ttl);

        /// <summary>
        /// Obtiene el PlayerId asociado a un token de recuperación de contraseña, o null si no existe/expiró.
        /// </summary>
        Task<string?> GetPasswordResetTokenAsync(string token);

        /// <summary>
        /// Elimina (invalida) el token de recuperación de contraseña de la caché.
        /// </summary>
        Task InvalidatePasswordResetTokenAsync(string token);
    }
}
