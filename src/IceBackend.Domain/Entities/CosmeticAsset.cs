using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class CosmeticAsset
    {
        public Guid Id { get; set; }
        public CosmeticType CosmeticType { get; set; }
        public string DisplayName { get; set; } = null!;
        public string AssetKey { get; set; } = null!;
        public string ModelUrl { get; set; } = null!;
        public string TextureUrl { get; set; } = null!;
        public string Sha256 { get; set; } = null!;
        public int AssetVersion { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public DateTime CreatedAt { get; set; }

        public ICollection<PlayerCosmeticOwnership> Ownerships { get; set; } = new List<PlayerCosmeticOwnership>();
    }
}
