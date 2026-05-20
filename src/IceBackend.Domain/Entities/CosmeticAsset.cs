using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Cosmético lógico del catálogo de ICE Launcher.
    ///
    /// IMPORTANTE — Modelo CAS:
    /// ─ Esta entidad NO almacena URLs, hashes ni blobs binarios directamente.
    /// ─ Los binarios viven en <see cref="CosmeticAssetVersion"/> (1 fila por arquitectura).
    /// ─ La entrega física al cliente siempre pasa por CDN; nunca por esta API.
    /// </summary>
    public class CosmeticAsset
    {
        public Guid Id { get; set; }

        /// <summary>Tipo de slot en el personaje (HAT, WING, CAPE, …).</summary>
        public CosmeticType CosmeticType { get; set; }

        /// <summary>Nombre visible en el Launcher (ej. "Sombrero del Capitán").</summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>
        /// Versión lógica del cosmético.
        /// Se incrementa cada vez que se sube una revisión nueva de cualquier arquitectura.
        /// Permite al Launcher invalidar su caché local comparando este número.
        /// </summary>
        public int AssetVersion { get; set; }

        /// <summary>Fecha en que el cosmético fue incorporado al catálogo.</summary>
        public DateTime CreatedAt { get; set; }

        // ── Relaciones ──────────────────────────────────────────────────────────

        /// <summary>
        /// Versiones binarias del cosmético agrupadas por arquitectura.
        /// El Launcher filtra esta colección según su versión de Minecraft.
        /// </summary>
        public ICollection<CosmeticAssetVersion> Versions { get; set; } = new List<CosmeticAssetVersion>();

        public ICollection<PlayerCosmeticOwnership> Ownerships { get; set; } = new List<PlayerCosmeticOwnership>();
    }
}
