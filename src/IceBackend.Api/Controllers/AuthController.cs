using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceBackend.Api.Controllers
{
    /// <summary>
    /// Controlador público de autenticación para cuentas locales ICE.
    /// Permite el registro de usuarios, el inicio de sesión nativo y el cierre de sesión.
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ISessionCache _sessionCache;

        public AuthController(IAuthService authService, ISessionCache sessionCache)
        {
            _authService = authService;
            _sessionCache = sessionCache;
        }

        /// <summary>
        /// Registra una nueva cuenta ICE e inicia sesión de forma automática.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var (player, token) = await _authService.RegisterAndLoginAsync(request.Username, request.Password);
                return Ok(new
                {
                    playerId = player.Id,
                    username = player.Username,
                    sessionToken = token,
                    createdAt = player.CreatedAt
                });
            }
            catch (Exception ex) when (ex.Message.Contains("already taken", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = "Username is already taken." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Inicia sesión con una cuenta local ICE existente.
        /// Soporta el re-hashing transparente de claves legacy.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginIceAccountAsync(request.Username, request.Password);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(new
            {
                playerId = result.Value.Player.Id,
                username = result.Value.Player.Username,
                sessionToken = result.Value.SessionToken
            });
        }

        /// <summary>
        /// Cierra la sesión activa del jugador autenticado, invalidando su token.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (playerIdClaim == null || !Guid.TryParse(playerIdClaim.Value, out var playerId))
                return Unauthorized();

            await _sessionCache.RemoveSessionAsync(playerId.ToString());
            return Ok(new { message = "Session closed successfully." });
        }
    }

    public record RegisterRequest(
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        [MaxLength(20, ErrorMessage = "Username cannot exceed 20 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain alphanumeric characters and underscores.")]
        string Username,

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        string Password);

    public record LoginRequest(
        [Required(ErrorMessage = "Username is required.")]
        string Username,

        [Required(ErrorMessage = "Password is required.")]
        string Password);
}
