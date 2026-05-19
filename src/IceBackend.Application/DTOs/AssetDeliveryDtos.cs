namespace IceBackend.Application.DTOs
{
    /// <summary>
    /// Respuesta de la Asset Delivery API para un cosmético individual.
    ///
    /// CONTRATO DE RESPUESTA (inmutable hasta aprobación de nueva versión):
    /// El backend SOLO entrega este mapa. El Launcher decide qué versión descargar
    /// basándose en su arquitectura local. El backend nunca envía binarios.
    ///
    /// Ejemplo de respuesta JSON:
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "type": "HAT",
    ///   "displayName": "Sombrero del Capitán",
    ///   "assetVersion": 3,
    ///   "versions": [
    ///     {
    ///       "arch": "Legacy",
    ///       "hash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
    ///       "sizeBytes": 48291,
    ///       "url": "https://cdn.icelauncher.com/assets/e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
    ///       "metadata": { "scale": [1.0, 1.0, 1.0], "offset": [0, 24, 0] }
    ///     },
    ///     {
    ///       "arch": "Modern",
    ///       "hash": "b94d27b99f1c073f7a0d45a3ef4a1b7c62e3918f5a4e2b8c0d1f3a6e9d2b5c8a",
    ///       "sizeBytes": 182740,
    ///       "url": "https://cdn.icelauncher.com/assets/b94d27b99f...",
    ///       "metadata": { "model": "blockbench_v4", "animations": ["idle", "walk"] }
    ///     }
    ///   ]
    /// }
    /// </summary>
    public sealed class CosmeticAssetDto
    {
        /// <summary>UUID interno de ICE — identificador estable para el Launcher.</summary>
        public Guid Id { get; init; }

        /// <summary>Tipo de slot (HAT, WING, CAPE, …).</summary>
        public string Type { get; init; } = null!;

        /// <summary>Nombre visible en la UI del Launcher.</summary>
        public string DisplayName { get; init; } = null!;

        /// <summary>
        /// Número de versión lógica del cosmético.
        /// Si este número cambia, el Launcher debe invalidar su caché local.
        /// </summary>
        public int AssetVersion { get; init; }

        /// <summary>
        /// Lista de versiones binarias disponibles.
        /// El Launcher filtra por <see cref="CosmeticVersionDto.Arch"/>
        /// y descarga SOLO la que coincide con su cliente.
        /// </summary>
        public IReadOnlyList<CosmeticVersionDto> Versions { get; init; } = [];
    }

    /// <summary>
    /// Versión binaria de un cosmético para una arquitectura específica.
    /// Contiene toda la información que el Launcher necesita para:
    /// 1. Validar si ya tiene el archivo en caché (hash).
    /// 2. Estimar el progreso de descarga (sizeBytes).
    /// 3. Obtener el binario (url → CDN, nunca la API).
    /// 4. Renderizarlo correctamente (metadata).
    /// </summary>
    public sealed class CosmeticVersionDto
    {
        /// <summary>Arquitectura de destino: "Legacy" | "Modern" | "Universal".</summary>
        public string Arch { get; init; } = null!;

        /// <summary>
        /// SHA-256 del binario en hexadecimal minúsculas (64 chars).
        /// El Launcher usa este valor para:
        /// a) Verificar integridad tras la descarga.
        /// b) Construir la clave de caché local.
        /// </summary>
        public string Hash { get; init; } = null!;

        /// <summary>Tamaño del binario en bytes para mostrar progreso de descarga.</summary>
        public long SizeBytes { get; init; }

        /// <summary>
        /// URL prefirmada o ruta en CDN para descargar el binario.
        /// NUNCA apunta a esta API. Siempre es una URL de CDN (S3/Cloudflare R2).
        /// Se construye como: {CdnBaseUrl}/assets/{Hash}
        /// </summary>
        public string Url { get; init; } = null!;

        /// <summary>
        /// Momento exacto en formato ISO 8601 UTC en el que la URL prefirmada expirará.
        /// El Launcher debe usar esta fecha para invalidar su caché de enlaces y solicitar uno nuevo.
        /// </summary>
        public DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Metadatos de renderizado específicos del motor de destino.
        /// El Launcher los consume intactos; el backend no los interpreta.
        /// </summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Respuesta de catálogo: lista de cosméticos que el jugador tiene autorización de usar.
    /// El backend verifica la membresía/propiedad antes de incluir cada cosmético en esta lista.
    /// </summary>
    public sealed class PlayerCosmeticCatalogDto
    {
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Solo incluye cosméticos a los que el jugador tiene acceso.
        /// El Launcher no recibe el catálogo completo; solo lo que le corresponde.
        /// </summary>
        public IReadOnlyList<CosmeticAssetDto> OwnedCosmetics { get; init; } = [];
    }
}
