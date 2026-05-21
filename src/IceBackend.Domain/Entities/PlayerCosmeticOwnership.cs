using System;

namespace IceBackend.Domain.Entities
{
    public class PlayerCosmeticOwnership
    {
        public Guid PlayerId { get; private set; }
        public Player Player { get; private set; } = null!;

        public Guid CosmeticId { get; private set; }
        public CosmeticAsset Cosmetic { get; private set; } = null!;

        public string ProviderPaymentId { get; private set; } = null!;
        public DateTime AcquiredAt { get; private set; }

        private PlayerCosmeticOwnership() { } // For EF Core

        public PlayerCosmeticOwnership(Guid playerId, Guid cosmeticId, string providerPaymentId)
        {
            if (playerId == Guid.Empty) throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
            if (cosmeticId == Guid.Empty) throw new ArgumentException("Cosmetic ID cannot be empty.", nameof(cosmeticId));
            if (string.IsNullOrWhiteSpace(providerPaymentId)) throw new ArgumentException("Payment ID cannot be empty.", nameof(providerPaymentId));

            PlayerId = playerId;
            CosmeticId = cosmeticId;
            ProviderPaymentId = providerPaymentId;
            AcquiredAt = DateTime.UtcNow;
        }
    }
}
