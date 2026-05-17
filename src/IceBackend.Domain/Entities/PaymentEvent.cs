using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class PaymentEvent
    {
        public Guid Id { get; set; }
        public PaymentProvider Provider { get; set; }
        public string ProviderEventId { get; set; } = null!;
        public string PaymentIntentId { get; set; } = null!;

        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public string Status { get; set; } = null!;

        // JSONB raw payload from Stripe webhook
        public string RawEvent { get; set; } = null!;

        public DateTime ProcessedAt { get; set; }
    }
}
