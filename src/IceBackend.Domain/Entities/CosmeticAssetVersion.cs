using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class CosmeticAssetVersion
    {
        public Guid Id { get; private set; }
        public Guid CosmeticAssetId { get; private set; }
        public AssetArchitecture Architecture { get; private set; }
        public string Sha256Hash { get; private set; } = null!;
        public long SizeBytes { get; private set; }
        public Dictionary<string, object> MetadataJson { get; private set; } = new();
        public DateTime UploadedAt { get; private set; }

        public CosmeticAsset CosmeticAsset { get; private set; } = null!;

        private CosmeticAssetVersion() { }

        public CosmeticAssetVersion(Guid id, Guid cosmeticAssetId, AssetArchitecture architecture, string sha256Hash, long sizeBytes, Dictionary<string, object> metadataJson)
        {
            if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));
            if (cosmeticAssetId == Guid.Empty) throw new ArgumentException("Cosmetic Asset ID cannot be empty.", nameof(cosmeticAssetId));
            if (string.IsNullOrWhiteSpace(sha256Hash)) throw new ArgumentException("SHA256 hash cannot be empty.", nameof(sha256Hash));
            if (sizeBytes <= 0) throw new ArgumentException("Size must be positive.", nameof(sizeBytes));

            Id = id;
            CosmeticAssetId = cosmeticAssetId;
            Architecture = architecture;
            Sha256Hash = sha256Hash;
            SizeBytes = sizeBytes;
            MetadataJson = metadataJson ?? new Dictionary<string, object>();
            UploadedAt = DateTime.UtcNow;
        }
    }
}
