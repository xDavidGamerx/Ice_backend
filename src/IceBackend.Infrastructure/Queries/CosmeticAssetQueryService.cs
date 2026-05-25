using System;
using System.Linq;
using System.Threading.Tasks;
using IceBackend.Application.DTOs;
using IceBackend.Application.Interfaces;
using IceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IceBackend.Infrastructure.Queries
{
    public class CosmeticAssetQueryService : ICosmeticAssetQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public CosmeticAssetQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CosmeticAssetDto?> GetCosmeticAssetInfoAsync(Guid id)
        {
            var cosmetic = await _dbContext.CosmeticAssets
                .AsNoTracking()
                .Include(c => c.Versions)
                .FirstOrDefaultAsync(c => (Guid)c.Id == id);

            if (cosmetic == null)
            {
                return null;
            }

            var versionDtos = cosmetic.Versions.Select(v => new CosmeticVersionDto
            {
                Arch = v.Architecture.ToString(),
                Hash = v.Sha256Hash,
                SizeBytes = v.SizeBytes,
                Metadata = v.MetadataJson ?? new System.Collections.Generic.Dictionary<string, object>()
            }).ToList();

            var responseDto = new CosmeticAssetDto
            {
                Id = cosmetic.Id.Value, // Obtener el Guid primitivo expuesto en el DTO
                Type = cosmetic.CosmeticType.ToString(),
                DisplayName = cosmetic.DisplayName,
                AssetVersion = cosmetic.AssetVersion,
                Versions = versionDtos
            };

            return responseDto;
        }
    }
}
