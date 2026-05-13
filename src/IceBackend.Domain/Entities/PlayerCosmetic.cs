using System;

namespace IceBackend.Domain.Entities
{
    public class PlayerCosmetic
    {
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int? HatId { get; set; }
        public CosmeticAsset? Hat { get; set; }

        public int? WingId { get; set; }
        public CosmeticAsset? Wing { get; set; }

        public int? CapeId { get; set; }
        public CosmeticAsset? Cape { get; set; }

        public int? ShirtId { get; set; }
        public CosmeticAsset? Shirt { get; set; }

        public int? PantsId { get; set; }
        public CosmeticAsset? Pants { get; set; }

        public int? ShoesId { get; set; }
        public CosmeticAsset? Shoes { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
