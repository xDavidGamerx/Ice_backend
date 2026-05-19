using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class PlayerCosmetic
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public CosmeticType Slot { get; set; }

        public Guid? CosmeticId { get; set; }
        public CosmeticAsset? Cosmetic { get; set; }

        public DateTime EquippedAt { get; set; }
    }
}
