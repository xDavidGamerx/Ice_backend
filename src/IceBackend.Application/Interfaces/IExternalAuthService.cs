using IceBackend.Application.DTOs;

namespace IceBackend.Application.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de autenticación externa OAuth2 con PKCE.
    /// La implementación concreta (con llamadas HTTP reales a Microsoft/Google) vive en Infrastructure.
    /// </summary>
    public interface IExternalAuthService
    {
        /// <summary>
        /// Genera la URL de autorización para el proveedor indicado con los parámetros
        /// PKCE (code_challenge, code_challenge_method=S256) y un state de alta entropía.
        /// Persiste el estado PKCE en Redis con TTL de 5 minutos.
        /// </summary>
        /// <param name="provider">"microsoft" | "google"</param>
        /// <returns>La URL de redirección completa hacia el proveedor.</returns>
        Task<string> GenerateAuthorizationUrlAsync(string provider);

        /// <summary>
        /// Procesa el callback OAuth2:
        /// 1. Valida el 'state' contra Redis (anti-CSRF).
        /// 2. Valida el 'code_verifier' contra el 'code_challenge' almacenado (PKCE).
        /// 3. Intercambia el código con el proveedor server-side.
        /// 4. Busca o crea el Player mapeando el ID externo al UUID interno de ICE.
        /// 5. Implementa protección anti-replay: destruye el código tras el primer uso.
        /// </summary>
        Task<ExternalAuthResultDto> ProcessCallbackAsync(OAuthCallbackDto callbackDto);
    }
}
