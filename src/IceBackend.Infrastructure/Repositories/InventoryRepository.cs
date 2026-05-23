using System;
using System.Linq;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Entities;
using IceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IceBackend.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public InventoryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Player?> GetPlayerWithCosmeticsAsync(Guid playerId)
        {
            // Ya no se requiere .Include(p => p.EquippedCosmetics) porque los wearables son columnas directas
            return await _dbContext.Players
                .FirstOrDefaultAsync(p => p.Id.Value == playerId);
        }

        public async Task<CosmeticAsset?> GetCosmeticWithVersionsAsync(Guid cosmeticId)
        {
            return await _dbContext.CosmeticAssets
                .AsNoTracking()
                .Include(c => c.Versions)
                .FirstOrDefaultAsync(c => c.Id.Value == cosmeticId);
        }

        public async Task<bool> PlayerOwnsCosmeticAsync(Guid playerId, Guid cosmeticId)
        {
            return await _dbContext.PlayerCosmeticOwnerships
                .AnyAsync(o => o.PlayerId.Value == playerId && o.Cosmetic.Id == new CosmeticId(cosmeticId));
        }

        public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
    }
}
