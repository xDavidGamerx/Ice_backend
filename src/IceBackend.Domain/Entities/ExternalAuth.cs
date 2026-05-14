using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class ExternalAuth
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public AuthProvider Provider { get; set; }
        public string ExternalId { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime LinkedAt { get; set; }
    }
}
