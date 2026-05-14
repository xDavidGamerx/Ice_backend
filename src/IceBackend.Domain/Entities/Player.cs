using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public UuidType UuidType { get; set; }
        public Guid PlayerUuid { get; set; }
        public string? PasswordHash { get; set; }
        public string? SessionHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public PlayerCosmetic? Cosmetic { get; set; }
        public ICollection<ExternalAuth> ExternalAuths { get; set; } = new List<ExternalAuth>();
        public ICollection<BootstrapToken> BootstrapTokens { get; set; } = new List<BootstrapToken>();
        public ICollection<PlayerCosmeticOwnership> CosmeticOwnerships { get; set; } = new List<PlayerCosmeticOwnership>();
        public ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
    }
}
