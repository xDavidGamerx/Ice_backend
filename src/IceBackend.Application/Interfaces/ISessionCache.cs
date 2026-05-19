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
    }
}
