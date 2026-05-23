using System;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Enums;

namespace IceBackend.Application.UseCases.Inventory
{
    /// <summary>
    /// Caso de Uso: Desequipar un cosmético de un slot del jugador.
    /// 
    /// Flujo:
    /// 1. Obtener jugador con cosméticos equipados.
    /// 2. Delegar al método de dominio Player.UnequipCosmetic().
    /// 3. Persistir en PostgreSQL. Si falla, invalidar caché Redis.
    /// </summary>
    public class UnequipCosmeticUseCase
    {
        private readonly IInventoryRepository _repo;
        private readonly ISessionCache _cache;

        public UnequipCosmeticUseCase(IInventoryRepository repo, ISessionCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<bool> ExecuteAsync(Guid playerId, CosmeticType slot)
        {
            var player = await _repo.GetPlayerWithCosmeticsAsync(playerId);
            if (player == null)
                return false;

            player.Unequip(slot);

            await _repo.SaveChangesAsync();

            // Purga post-commit garantizada
            await _cache.InvalidatePlayerCosmeticsAsync(playerId);

            return true;
        }
    }
}
