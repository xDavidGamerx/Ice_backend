using System;

namespace IceBackend.Domain.Entities
{
    public class BootstrapToken
    {
        public string TokenHash { get; set; } = null!;
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public string Context { get; set; } = null!;
    }
}
