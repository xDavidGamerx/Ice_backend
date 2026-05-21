using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class ExternalAuth
    {
        public Guid Id { get; private set; }
        public Guid PlayerId { get; private set; }
        public Player Player { get; private set; } = null!;
        public AuthProvider Provider { get; private set; }
        public string ExternalId { get; private set; } = null!;
        public string? Email { get; private set; }
        public DateTime LinkedAt { get; private set; }

        private ExternalAuth() { }

        public ExternalAuth(Guid id, Guid playerId, AuthProvider provider, string externalId, string? email)
        {
            Id = id;
            PlayerId = playerId;
            Provider = provider;
            ExternalId = externalId;
            Email = email;
            LinkedAt = DateTime.UtcNow;
        }
    }
}
