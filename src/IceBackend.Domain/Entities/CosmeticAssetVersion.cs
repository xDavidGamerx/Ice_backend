using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class CosmeticAssetVersion
    {
        public Guid Id { get; private set; }
        public int CosmeticAssetInternalId { get; private set; } // Referencia a la clave subrogada del asset
        public AssetArchitecture Architecture { get; private set; }
        public string Sha256Hash { get; private set; } = null!;
        public long SizeBytes { get; private set; }
        public Dictionary<string, object> MetadataJson { get; private set; } = new();
        public DateTime UploadedAt { get; private set; }

        public CosmeticAsset CosmeticAsset { get; private set; } = null!;

        private CosmeticAssetVersion() { }

        public CosmeticAssetVersion(Guid id, int cosmeticAssetInternalId, AssetArchitecture architecture, string sha256Hash, long sizeBytes, Dictionary<string, object> metadataJson)
        {
            if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));
            if (cosmeticAssetInternalId <= 0) throw new ArgumentException("Cosmetic Asset Internal ID must be a positive integer.", nameof(cosmeticAssetInternalId));
            if (string.IsNullOrWhiteSpace(sha256Hash)) throw new ArgumentException("SHA256 hash cannot be empty.", nameof(sha256Hash));
            if (sizeBytes <= 0) throw new ArgumentException("Size must be positive.", nameof(sizeBytes));

            Id = id;
            CosmeticAssetInternalId = cosmeticAssetInternalId;
            Architecture = architecture;
            Sha256Hash = sha256Hash;
            SizeBytes = sizeBytes;
            MetadataJson = metadataJson ?? new Dictionary<string, object>();
            UploadedAt = DateTime.UtcNow;
        }
    }
}
