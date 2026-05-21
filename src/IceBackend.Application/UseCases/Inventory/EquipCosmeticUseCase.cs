using System;
using System.Linq;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Enums;

namespace IceBackend.Application.UseCases.Inventory
{
    /// <summary>
    /// Caso de Uso: Equipar un cosmético en un slot del jugador.
    /// 
    /// Flujo:
    /// 1. Verificar que el cliente envió su arquitectura (header X-Client-Architecture).
    /// 2. Verificar que el jugador posee el cosmético.
    /// 3. Verificar que el cosmético tiene una versión compatible con la arquitectura del cliente.
    /// 4. Verificar que el tipo del cosmético coincide con el slot solicitado.
    /// 5. Delegar al método de dominio Player.EquipCosmetic().
    /// 6. Persistir en PostgreSQL. Si falla, invalidar caché Redis.
    /// </summary>
    public class EquipCosmeticUseCase
    {
        private readonly IInventoryRepository _repo;
        private readonly ISessionCache _cache;
        private readonly IClientContext _clientContext;

        public EquipCosmeticUseCase(
            IInventoryRepository repo,
            ISessionCache cache,
            IClientContext clientContext)
        {
            _repo = repo;
            _cache = cache;
            _clientContext = clientContext;
        }

        public async Task<EquipResult> ExecuteAsync(Guid playerId, Guid cosmeticId, CosmeticType slot)
        {
            // 1. La arquitectura del cliente es obligatoria para validar compatibilidad.
            if (_clientContext.Architecture == null)
                return EquipResult.ArchitectureNotProvided;

            var clientArch = _clientContext.Architecture.Value;

            // 2. Verificar propiedad del cosmético.
            if (!await _repo.PlayerOwnsCosmeticAsync(playerId, cosmeticId))
                return EquipResult.NotOwned;

            // 3. Verificar compatibilidad de arquitectura.
            var cosmetic = await _repo.GetCosmeticWithVersionsAsync(cosmeticId);
            if (cosmetic == null)
                return EquipResult.CosmeticNotFound;

            bool hasCompatibleVersion = cosmetic.Versions.Any(v =>
                v.Architecture == clientArch || v.Architecture == AssetArchitecture.Universal);

            if (!hasCompatibleVersion)
                return EquipResult.ArchitectureIncompatible;

            // 4. Verificar que el tipo del cosmético coincide con el slot.
            if (cosmetic.CosmeticType != slot)
                return EquipResult.SlotMismatch;

            // 5. Obtener jugador y ejecutar lógica de dominio.
            var player = await _repo.GetPlayerWithCosmeticsAsync(playerId);
            if (player == null)
                return EquipResult.PlayerNotFound;

            player.EquipCosmetic(slot, cosmeticId);

            // 6. Persistir con protección transaccional.
            //    Si PostgreSQL falla, invalidamos Redis para mantener la fuente de verdad.
            try
            {
                await _repo.SaveChangesAsync();
            }
            catch
            {
                await _cache.InvalidatePlayerCosmeticsAsync(playerId);
                throw;
            }

            return EquipResult.Success;
        }
    }
}
