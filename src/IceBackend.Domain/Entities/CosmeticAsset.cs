using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class CosmeticAsset
    {
        public Guid Id { get; private set; }
        public CosmeticType CosmeticType { get; private set; }
        public string DisplayName { get; private set; } = null!;
        public int AssetVersion { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<CosmeticAssetVersion> _versions = new();
        public IReadOnlyCollection<CosmeticAssetVersion> Versions => _versions.AsReadOnly();

        private readonly List<PlayerCosmeticOwnership> _ownerships = new();
        public IReadOnlyCollection<PlayerCosmeticOwnership> Ownerships => _ownerships.AsReadOnly();

        private CosmeticAsset() { }

        public CosmeticAsset(Guid id, CosmeticType type, string displayName)
        {
            if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

            Id = id;
            CosmeticType = type;
            DisplayName = displayName;
            AssetVersion = 1;
            CreatedAt = DateTime.UtcNow;
        }

        public void IncrementVersion()
        {
            AssetVersion++;
        }

        public void AddVersion(CosmeticAssetVersion version)
        {
            _versions.Add(version);
        }
    }
}
