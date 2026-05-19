namespace IceBackend.Application.DTOs
{
    /// <summary>
    /// Datos del callback OAuth2 que el Launcher entrega al backend.
    /// Contiene el código temporal del proveedor y el material PKCE necesario para validarlo.
    /// </summary>
    public sealed class OAuthCallbackDto
    {
        /// <summary>El parámetro 'state' que el proveedor devolvió al Launcher.</summary>
        public string State { get; init; } = null!;

        /// <summary>El código de autorización temporal del proveedor (Microsoft/Google).</summary>
        public string Code { get; init; } = null!;

        /// <summary>
        /// El 'code_verifier' original (generado por el Launcher en memoria volátil).
        /// Se valida en el servidor contra el 'code_challenge' almacenado en Redis.
        /// </summary>
        public string CodeVerifier { get; init; } = null!;

        /// <summary>Proveedor de identidad ("microsoft" | "google").</summary>
        public string Provider { get; init; } = null!;
    }

    /// <summary>
    /// Datos de perfil de usuario obtenidos del proveedor OAuth2.
    /// DTO que aísla la capa Application de los SDKs específicos de cada proveedor.
    /// </summary>
    public sealed class ExternalUserProfileDto
    {
        public string ExternalId { get; init; } = null!;
        public string? Email { get; init; }
        public string? PreferredUsername { get; init; }
    }

    /// <summary>
    /// Resultado del flujo completo de OAuth2 PKCE retornado al Launcher.
    /// El SessionToken sigue el estándar compatible con authlib-injector.
    /// </summary>
    public sealed class ExternalAuthResultDto
    {
        public Guid PlayerId { get; init; }
        public string Username { get; init; } = null!;

        /// <summary>Token de sesión que el Launcher usará en futuras peticiones al backend.</summary>
        public string SessionToken { get; init; } = null!;

        /// <summary>Indica si fue un registro nuevo (true) o un login existente (false).</summary>
        public bool IsNewPlayer { get; init; }
    }
}
