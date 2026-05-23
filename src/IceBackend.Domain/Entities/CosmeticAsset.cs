using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class CosmeticAsset
    {
        // 1. Clave subrogada de base de datos para optimización de almacenamiento relacional e indexación física
        public int InternalId { get; private set; }

        // 2. Clave pública de dominio (no enumerable) para API/Launcher (UUID)
        public CosmeticId Id { get; private set; } = null!;

        public CosmeticType CosmeticType { get; private set; }
        public string DisplayName { get; private set; } = null!;
        public int AssetVersion { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<CosmeticAssetVersion> _versions = new();
        public IReadOnlyCollection<CosmeticAssetVersion> Versions => _versions.AsReadOnly();

        private readonly List<PlayerCosmeticOwnership> _ownerships = new();
        public IReadOnlyCollection<PlayerCosmeticOwnership> Ownerships => _ownerships.AsReadOnly();

        private CosmeticAsset() { }

        public CosmeticAsset(CosmeticId id, CosmeticType type, string displayName)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            CosmeticType = type;
            DisplayName = !string.IsNullOrWhiteSpace(displayName) 
                ? displayName 
                : throw new ArgumentException("El nombre del cosmético no puede estar vacío.", nameof(displayName));
            AssetVersion = 1;
            CreatedAt = DateTime.UtcNow;
        }

        public void IncrementVersion() => AssetVersion++;
        public void AddVersion(CosmeticAssetVersion version) => _versions.Add(version);
    }
}
