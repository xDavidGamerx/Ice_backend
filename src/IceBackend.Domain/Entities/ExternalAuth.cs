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
            if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));
            if (playerId == Guid.Empty) throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
            if (string.IsNullOrWhiteSpace(externalId)) throw new ArgumentException("External ID cannot be empty.", nameof(externalId));

            Id = id;
            PlayerId = playerId;
            Provider = provider;
            ExternalId = externalId;
            Email = email;
            LinkedAt = DateTime.UtcNow;
        }
    }
}
