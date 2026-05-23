using System;

namespace IceBackend.Domain.Entities
{
    public class PlayerCosmeticOwnership
    {
        public PlayerId PlayerId { get; private set; } = null!;
        public Player Player { get; private set; } = null!;

        public int CosmeticAssetInternalId { get; private set; }
        public CosmeticAsset Cosmetic { get; private set; } = null!;

        public string ProviderPaymentId { get; private set; } = null!;
        public DateTime AcquiredAt { get; private set; }

        private PlayerCosmeticOwnership() { } // For EF Core

        public PlayerCosmeticOwnership(PlayerId playerId, int cosmeticAssetInternalId, string providerPaymentId)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (cosmeticAssetInternalId <= 0) throw new ArgumentException("Cosmetic Asset Internal ID must be positive.", nameof(cosmeticAssetInternalId));
            if (string.IsNullOrWhiteSpace(providerPaymentId)) throw new ArgumentException("Payment ID cannot be empty.", nameof(providerPaymentId));

            PlayerId = playerId;
            CosmeticAssetInternalId = cosmeticAssetInternalId;
            ProviderPaymentId = providerPaymentId;
            AcquiredAt = DateTime.UtcNow;
        }
    }
}
