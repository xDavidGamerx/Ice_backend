using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class PaymentEvent
    {
        public Guid Id { get; private set; }
        public PaymentProvider Provider { get; private set; }
        public string ProviderEventId { get; private set; } = null!;
        public string PaymentIntentId { get; private set; } = null!;

        public Guid PlayerId { get; private set; }
        public Player Player { get; private set; } = null!;

        public string Status { get; private set; } = null!;

        public string RawEvent { get; private set; } = null!;

        public DateTime ProcessedAt { get; private set; }

        private PaymentEvent() { }

        public PaymentEvent(Guid id, PaymentProvider provider, string providerEventId, string paymentIntentId, Guid playerId, string status, string rawEvent)
        {
            Id = id;
            Provider = provider;
            ProviderEventId = providerEventId;
            PaymentIntentId = paymentIntentId;
            PlayerId = playerId;
            Status = status;
            RawEvent = rawEvent;
            ProcessedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }
    }
}
