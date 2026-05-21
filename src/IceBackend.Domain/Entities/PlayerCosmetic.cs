using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class PlayerCosmetic
    {
        public Guid PlayerId { get; private set; }
        public Player Player { get; private set; } = null!;

        public CosmeticType Slot { get; private set; }

        public Guid? CosmeticId { get; private set; }
        public CosmeticAsset? Cosmetic { get; private set; }

        public DateTime EquippedAt { get; private set; }

        private PlayerCosmetic() { } // For EF Core

        internal PlayerCosmetic(Guid playerId, CosmeticType slot)
        {
            PlayerId = playerId;
            Slot = slot;
            CosmeticId = null;
            EquippedAt = DateTime.UtcNow;
        }

        public void Equip(Guid cosmeticId)
        {
            CosmeticId = cosmeticId;
            EquippedAt = DateTime.UtcNow;
        }

        public void Unequip()
        {
            CosmeticId = null;
            EquippedAt = DateTime.UtcNow;
        }
    }
}
