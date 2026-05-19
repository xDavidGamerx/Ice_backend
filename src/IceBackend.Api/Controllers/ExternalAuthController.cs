using IceBackend.Application.Interfaces;
using IceBackend.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceBackend.Api.Controllers
{
    /// <summary>
    /// Controlador del flujo OAuth2 PKCE para autenticación externa (Microsoft / Google).
    /// </summary>
    [ApiController]
    [Route("api/auth/external")]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IExternalAuthService _externalAuthService;
        private readonly ILogger<ExternalAuthController> _logger;

        public ExternalAuthController(
            IExternalAuthService externalAuthService,
            ILogger<ExternalAuthController> logger)
        {
            _externalAuthService = externalAuthService;
            _logger = logger;
        }

        /// <summary>
        /// Genera la URL de autorización de Microsoft con parámetros PKCE.
        /// El Launcher abre esta URL en el browser del sistema.
        /// GET /api/auth/external/microsoft/login
        /// </summary>
        [HttpGet("microsoft/login")]
        public async Task<IActionResult> MicrosoftLogin()
        {
            var url = await _externalAuthService.GenerateAuthorizationUrlAsync("microsoft");
            // Retornamos la URL como JSON para que el Launcher la procese
            // (no hacemos redirect: el Launcher decide cómo abrir el browser).
            return Ok(new { authorizationUrl = url });
        }

        /// <summary>
        /// Genera la URL de autorización de Google con parámetros PKCE.
        /// GET /api/auth/external/google/login
        /// </summary>
        [HttpGet("google/login")]
        public async Task<IActionResult> GoogleLogin()
        {
            var url = await _externalAuthService.GenerateAuthorizationUrlAsync("google");
            return Ok(new { authorizationUrl = url });
        }

        /// <summary>
        /// Endpoint de callback unificado. Recibe los datos del Launcher tras el redirect del proveedor.
        /// El Launcher captura el code y state del URI icelauncher://oauth2/callback y los envía aquí
        /// junto con el code_verifier que generó en memoria.
        /// POST /api/auth/external/callback
        /// </summary>
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] ExternalCallbackRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _externalAuthService.ProcessCallbackAsync(new Application.DTOs.OAuthCallbackDto
                {
                    State = request.State,
                    Code = request.Code,
                    CodeVerifier = request.CodeVerifier,
                    Provider = request.Provider
                });

                return Ok(new
                {
                    playerId = result.PlayerId,
                    username = result.Username,
                    sessionToken = result.SessionToken,
                    isNewPlayer = result.IsNewPlayer
                });
            }
            catch (SecurityException ex)
            {
                // Errores de seguridad → 400 Bad Request (no 401: el token aún no existe).
                _logger.LogWarning("OAuth callback security violation: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                // Error al comunicarse con Microsoft/Google.
                _logger.LogError(ex, "Error communicating with OAuth provider.");
                return StatusCode(502, new { error = "Failed to communicate with identity provider." });
            }
        }
    }

    /// <summary>
    /// Request body del endpoint de callback. No usamos OAuthCallbackDto directamente
    /// para mantener la capa Api desacoplada de los contratos internos de Application.
    /// </summary>
    public sealed class ExternalCallbackRequest
    {
        public string State { get; init; } = null!;
        public string Code { get; init; } = null!;
        public string CodeVerifier { get; init; } = null!;
        public string Provider { get; init; } = null!;
    }
}
