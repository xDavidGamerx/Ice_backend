using System;
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
            return await _dbContext.Players
                .Include(p => p.EquippedCosmetics)
                .FirstOrDefaultAsync(p => p.Id == playerId);
        }

        public async Task<CosmeticAsset?> GetCosmeticWithVersionsAsync(Guid cosmeticId)
        {
            return await _dbContext.CosmeticAssets
                .AsNoTracking()
                .Include(c => c.Versions)
                .FirstOrDefaultAsync(c => c.Id == cosmeticId);
        }

        public async Task<bool> PlayerOwnsCosmeticAsync(Guid playerId, Guid cosmeticId)
        {
            return await _dbContext.PlayerCosmeticOwnerships
                .AnyAsync(o => o.PlayerId == playerId && o.CosmeticId == cosmeticId);
        }

        public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
    }
}
