using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IceBackend.Api.Controllers
{
    /// <summary>
    /// Controlador de autenticación exclusivo para desarrollo local.
    /// Permite obtener un SessionToken válido sin depender del flujo OAuth2 ni del launcher.
    /// 
    /// SEGURIDAD:
    /// - Oculto de Swagger ([ApiExplorerSettings(IgnoreApi = true)])
    /// - Guardia de entorno: lanza InvalidOperationException si ASPNETCORE_ENVIRONMENT != Development
    /// - Token maestro leído de configuración (DEV_MASTER_TOKEN), nunca hardcodeado
    /// - Sesiones almacenadas con prefijo dev_session: para aislamiento
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/v1/dev")]
    public class DevAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthService _authService;
        private readonly ISessionCache _sessionCache;
        private readonly IHostEnvironment _env;
        private readonly string _masterToken;

        public DevAuthController(
            ApplicationDbContext dbContext,
            IAuthService authService,
            ISessionCache sessionCache,
            IHostEnvironment env,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _authService = authService;
            _sessionCache = sessionCache;
            _env = env;
            _masterToken = configuration["DEV_MASTER_TOKEN"]
                ?? throw new InvalidOperationException(
                    "CRITICAL: DEV_MASTER_TOKEN no configurado. Agregar al .env para desarrollo local.");
        }

        /// <summary>
        /// Genera un SessionToken de desarrollo para un jugador.
        /// Si el jugador no existe, lo crea con una contraseña aleatoria.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DevLoginRequest request)
        {
            // ── GUARDIA DE ENTORNO ──────────────────────────────────────────
            if (!_env.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Backdoor prohibida en entornos no locales.");
            }

            // ── VALIDACIÓN DE ENTRADA ───────────────────────────────────────
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { message = "Username is required." });

            if (string.IsNullOrWhiteSpace(request.MasterToken))
                return BadRequest(new { message = "Master token is required." });

            // ── VALIDAR TOKEN MAESTRO ───────────────────────────────────────
            if (!string.Equals(request.MasterToken, _masterToken, StringComparison.Ordinal))
                return Unauthorized(new { message = "Invalid master token." });

            // ── BUSCAR O CREAR JUGADOR ─────────────────────────────────────
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.Username.ToLower() == request.Username.ToLower());

            if (player == null)
            {
                // Crear jugador con contraseña aleatoria (no se usará para login real)
                player = await _authService.RegisterIceAccountAsync(
                    request.Username,
                    "dev_" + Guid.NewGuid().ToString("N"));
            }

            // ── GENERAR SESSION TOKEN AISLADO ──────────────────────────────
            var sessionToken = GenerateDevSessionToken();
            var ttl = TimeSpan.FromHours(8);

            await _sessionCache.SetDevSessionAsync(
                player.Id.ToString(),
                sessionToken,
                ttl);

            return Ok(new
            {
                playerId = player.Id,
                username = player.Username,
                sessionToken
            });
        }

        /// <summary>
        /// Genera un token de sesión criptográficamente seguro para desarrollo.
        /// </summary>
        private static string GenerateDevSessionToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    public class DevLoginRequest
    {
        public string Username { get; init; } = null!;
        public string MasterToken { get; init; } = null!;
    }
}
