using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IceBackend.Application.DTOs;
using IceBackend.Application.Interfaces;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IceBackend.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/assets")]
    public class AssetDeliveryController : ControllerBase
    {
        private readonly ISessionCache _sessionCache;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICdnUrlSigner _cdnUrlSigner;
        private readonly IAssetTokenService _assetTokenService;

        public AssetDeliveryController(
            ISessionCache sessionCache,
            ApplicationDbContext dbContext,
            ICdnUrlSigner cdnUrlSigner,
            IAssetTokenService assetTokenService)
        {
            _sessionCache = sessionCache;
            _dbContext = dbContext;
            _cdnUrlSigner = cdnUrlSigner;
            _assetTokenService = assetTokenService;
        }

        public class RequestDeliveryBody
        {
            public string Hash { get; set; } = null!;
        }

        /// <summary>
        /// Obtiene el catálogo de versiones (hashes) para un cosmético específico.
        /// El DTO devuelto es estático y no contiene URLs efímeras.
        /// </summary>
        [HttpGet("info/{id:guid}")]
        public async Task<IActionResult> GetCosmeticAssetInfo(Guid id)
        {
            var cosmetic = await _dbContext.CosmeticAssets
                .Include(c => c.Versions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cosmetic == null)
            {
                return NotFound(new { message = "Cosmetic not found" });
            }

            var versionDtos = cosmetic.Versions.Select(v => new CosmeticVersionDto
            {
                Arch = v.Architecture.ToString(),
                Hash = v.Sha256Hash,
                SizeBytes = v.SizeBytes,
                Metadata = v.MetadataJson ?? new Dictionary<string, object>()
            }).ToList();

            var responseDto = new CosmeticAssetDto
            {
                Id = cosmetic.Id,
                Type = cosmetic.CosmeticType.ToString(),
                DisplayName = cosmetic.DisplayName,
                AssetVersion = cosmetic.AssetVersion,
                Versions = versionDtos
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// Solicita un token de descarga efímero para un activo determinado.
        /// El cliente debe estar autenticado con su token de sesión general.
        /// </summary>
        [HttpPost("request-delivery")]
        public async Task<IActionResult> RequestDelivery([FromBody] RequestDeliveryBody request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Hash))
            {
                return BadRequest(new { message = "Hash is required" });
            }

            var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (playerIdClaim == null || !Guid.TryParse(playerIdClaim.Value, out var playerId))
            {
                return Unauthorized();
            }

            // 1. Resolver Hash -> CosmeticId (Cache-Aside)
            var cosmeticId = await _sessionCache.GetCosmeticIdByHashAsync(request.Hash);
            if (cosmeticId == null)
            {
                return NotFound(new { message = "Asset hash not recognized" });
            }

            // 2. Verificar propiedad (Cache-Aside O(1))
            var isOwner = await _sessionCache.IsCosmeticOwnedAsync(playerId, cosmeticId.Value);
            if (!isOwner)
            {
                return StatusCode(403, new { message = "Access denied: You do not own this asset" });
            }

            // 3. Generar token firmado de corta duración (2 minutos)
            var deliveryToken = _assetTokenService.GenerateDeliveryToken(playerId, request.Hash, TimeSpan.FromMinutes(2));
            var deliveryUrl = $"/api/v1/assets/deliver/{request.Hash}?token={deliveryToken}";

            return Ok(new
            {
                token = deliveryToken,
                url = deliveryUrl
            });
        }

        /// <summary>
        /// Endpoint central de entrega de activos.
        /// Valida el token efímero firmado por el servidor y realiza un redirect 307 al CDN.
        /// No revela detalles de error específicos ante fallos de validación (403 genérico).
        /// </summary>
        [HttpGet("deliver/{hash}")]
        [AllowAnonymous]
        public IActionResult Deliver(string hash, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(token))
            {
                return Forbid(); // Retorna 403 Forbidden sin cuerpo explicativo
            }

            // Validar token de corta duración del Launcher
            var isValid = _assetTokenService.ValidateDeliveryToken(token, hash, out _);
            if (!isValid)
            {
                return Forbid(); // Retorna 403 Forbidden sin cuerpo explicativo
            }

            // Generar URL firmada efímera (60s TTL) para el CDN
            var (signedUrl, _) = _cdnUrlSigner.GeneratePresignedUrl(hash);

            // Redirección temporal al CDN (307)
            return Redirect(signedUrl);
        }
    }
}
