using System;
using System.Linq;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Enums;
using IceBackend.Domain.Services;

namespace IceBackend.Application.UseCases.Inventory
{
    /// <summary>
    /// Caso de Uso: Equipar un cosmético en un slot del jugador.
    /// 
    /// Flujo:
    /// 1. La arquitectura del cliente es obligatoria (X-Client-Architecture).
    /// 2. Obtener metadatos del cosmético.
    /// 3. Validar propiedad del cosmético.
    /// 4. Obtener jugador.
    /// 5. Validar políticas de dominio y emitir token firmado (CosmeticEquipmentPolicy).
    /// 6. Delegar la mutación de estado a Player.Equip(token).
    /// 7. Persistir en PostgreSQL. Si falla, invalidar caché Redis.
    /// </summary>
    public class EquipCosmeticUseCase
    {
        private readonly IInventoryRepository _repo;
        private readonly ISessionCache _cache;
        private readonly IClientContext _clientContext;
        private readonly CosmeticEquipmentPolicy _policy;

        public EquipCosmeticUseCase(
            IInventoryRepository repo,
            ISessionCache cache,
            IClientContext clientContext,
            CosmeticEquipmentPolicy policy)
        {
            _repo = repo;
            _cache = cache;
            _clientContext = clientContext;
            _policy = policy;
        }

        public async Task<EquipResult> ExecuteAsync(Guid playerId, Guid cosmeticId, CosmeticType slot)
        {
            // 1. La arquitectura del cliente es obligatoria para validar compatibilidad.
            if (_clientContext.Architecture == null)
                return EquipResult.ArchitectureNotProvided;

            var clientArch = _clientContext.Architecture.Value;

            // 2. Obtener metadatos del cosmético
            var cosmetic = await _repo.GetCosmeticWithVersionsAsync(cosmeticId);
            if (cosmetic == null)
                return EquipResult.CosmeticNotFound;

            // 3. Verificar que el tipo del cosmético coincide con el slot.
            if (cosmetic.CosmeticType != slot)
                return EquipResult.SlotMismatch;

            // 4. Verificar propiedad del cosmético
            if (!await _repo.PlayerOwnsCosmeticAsync(playerId, cosmeticId))
                return EquipResult.NotOwned;

            // 5. Obtener jugador
            var player = await _repo.GetPlayerWithCosmeticsAsync(playerId);
            if (player == null)
                return EquipResult.PlayerNotFound;

            try
            {
                // 6. Validar a través del Domain Service y firmar el token de equipamiento
                // Para equipamiento regular desde el Launcher, no se requiere validación redundante contra Mojang (pasamos null)
                var token = await _policy.ValidateAndSignEquipmentAsync(player, cosmetic, clientArch, mojangAccessToken: null);

                // 7. Mutar el agregado del jugador con el token validado
                player.Equip(token);
            }
            catch (InvalidOperationException)
            {
                return EquipResult.ArchitectureIncompatible;
            }

            // 8. Persistir en DB
            await _repo.SaveChangesAsync();

            // 9. Purga post-commit garantizada
            await _cache.InvalidatePlayerCosmeticsAsync(playerId);

            return EquipResult.Success;
        }
    }
}
