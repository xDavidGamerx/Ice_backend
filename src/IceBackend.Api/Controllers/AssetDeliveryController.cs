using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IceBackend.Application.DTOs;
using IceBackend.Application.Interfaces;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IceBackend.Api.Controllers
{
    [ApiController]
    [Route("api/cosmetics")]
    public class AssetDeliveryController : ControllerBase
    {
        private readonly ISessionCache _sessionCache;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICdnUrlSigner _cdnUrlSigner;

        public AssetDeliveryController(
            ISessionCache sessionCache,
            ApplicationDbContext dbContext,
            ICdnUrlSigner cdnUrlSigner)
        {
            _sessionCache = sessionCache;
            _dbContext = dbContext;
            _cdnUrlSigner = cdnUrlSigner;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCosmeticAsset(Guid id)
        {
            // 1. Extraer el SessionToken del header Authorization
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Unauthorized(new { message = "Missing Authorization header" });
            }

            var token = authHeader.ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Invalid Authorization header" });
            }

            // 2. Extraer el PlayerId validando contra Redis
            var playerIdStr = await _sessionCache.GetPlayerIdBySessionAsync(token);
            if (string.IsNullOrEmpty(playerIdStr) || !Guid.TryParse(playerIdStr, out var playerId))
            {
                return Unauthorized(new { message = "Invalid or expired session" });
            }

            // 3. Consultar propiedad estricta en PostgreSQL
            var isOwner = await _dbContext.PlayerCosmeticOwnerships
                .AnyAsync(o => o.PlayerId == playerId && o.CosmeticId == id);

            if (!isOwner)
            {
                return StatusCode(403, new { message = "You do not own this cosmetic asset" });
            }

            // 4. Obtener el cosmético y sus versiones
            var cosmetic = await _dbContext.CosmeticAssets
                .Include(c => c.Versions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cosmetic == null)
            {
                return NotFound(new { message = "Cosmetic not found" });
            }

            // 5. Construir el DTO con las URLs prefirmadas
            var versionDtos = new List<CosmeticVersionDto>();
            foreach (var version in cosmetic.Versions)
            {
                var (signedUrl, expiresAt) = _cdnUrlSigner.GeneratePresignedUrl(version.Sha256Hash);

                var metadataDict = version.MetadataJson ?? new Dictionary<string, object>();

                versionDtos.Add(new CosmeticVersionDto
                {
                    Arch = version.Architecture.ToString(),
                    Hash = version.Sha256Hash,
                    SizeBytes = version.SizeBytes,
                    Url = signedUrl,
                    ExpiresAt = expiresAt,
                    Metadata = metadataDict
                });
            }

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
    }
}
