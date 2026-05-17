using System;

namespace IceBackend.Domain.Entities
{
    public class PlayerCosmeticOwnership
    {
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public Guid CosmeticId { get; set; }
        public CosmeticAsset Cosmetic { get; set; } = null!;

        public string ProviderPaymentId { get; set; } = null!;
        public DateTime AcquiredAt { get; set; }
    }
}
