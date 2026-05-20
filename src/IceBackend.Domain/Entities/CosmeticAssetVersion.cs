using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Representa una versión específica de un cosmético para una arquitectura de cliente concreta.
    ///
    /// MODELO CAS (Content-Addressable Storage):
    /// ─ Los archivos NO tienen nombres semánticos.
    /// ─ Cada fila corresponde a un binario único identificado exclusivamente por su Sha256Hash.
    /// ─ La URL en CDN se deriva del hash: /assets/{Sha256Hash} (nunca se almacena directamente).
    ///
    /// Relación: CosmeticAsset 1──* CosmeticAssetVersion
    /// </summary>
    public class CosmeticAssetVersion
    {
        /// <summary>PK surrogate — UUID generado en servidor.</summary>
        public Guid Id { get; set; }

        /// <summary>FK hacia el cosmético padre.</summary>
        public Guid CosmeticAssetId { get; set; }

        /// <summary>Arquitectura de destino (Legacy/Modern/Universal).</summary>
        public AssetArchitecture Architecture { get; set; }

        /// <summary>
        /// SHA-256 del binario en hexadecimal minúsculas (64 chars).
        /// Es el identificador del contenido en el almacenamiento CAS.
        /// Garantía de unicidad: si dos cosméticos distintos comparten un hash,
        /// apuntan al mismo objeto físico en CDN (deduplicación automática).
        /// </summary>
        public string Sha256Hash { get; set; } = null!;

        /// <summary>
        /// Tamaño del binario en bytes. Permite al Launcher estimar el progreso
        /// de descarga sin hacer un HEAD request previo al CDN.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Metadatos específicos del motor de renderizado (JSONB en PostgreSQL).
        /// Ejemplos:
        ///   Legacy:  { "scale": [1.0, 1.0, 1.0], "offset": [0, 24, 0] }
        ///   Modern:  { "model": "blockbench_v4", "animations": ["idle", "walk"] }
        ///   Universal: { "layer": "outer" }
        /// El Launcher lo consume intacto; el backend no lo interpreta.
        /// </summary>
        public Dictionary<string, object> MetadataJson { get; set; } = new();

        /// <summary>Marca de tiempo de carga del binario al sistema.</summary>
        public DateTime UploadedAt { get; set; }

        // ── Navegación ──────────────────────────────────────────────────────────
        public CosmeticAsset CosmeticAsset { get; set; } = null!;
    }
}
