using System;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;

namespace IceBackend.Application.Interfaces
{
    /// <summary>
    /// Abstracción de persistencia para operaciones de inventario.
    /// Los Use Cases dependen de esta interfaz; la implementación concreta vive en Infrastructure.
    /// </summary>
    public interface IInventoryRepository
    {
        /// <summary>
        /// Obtiene un jugador con su colección de cosméticos equipados cargada.
        /// </summary>
        Task<Player?> GetPlayerWithCosmeticsAsync(Guid playerId);

        /// <summary>
        /// Obtiene un cosmético con sus versiones (para validar arquitectura).
        /// Consulta de solo lectura (AsNoTracking).
        /// </summary>
        Task<CosmeticAsset?> GetCosmeticWithVersionsAsync(Guid cosmeticId);

        /// <summary>
        /// Verifica si un jugador es propietario de un cosmético.
        /// </summary>
        Task<bool> PlayerOwnsCosmeticAsync(Guid playerId, Guid cosmeticId);

        /// <summary>
        /// Persiste los cambios realizados en las entidades rastreadas.
        /// </summary>
        Task SaveChangesAsync();
    }
}
