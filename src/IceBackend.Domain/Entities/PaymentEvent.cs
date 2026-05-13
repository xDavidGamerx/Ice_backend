using System;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class PaymentEvent
    {
        public int Id { get; set; }
        public PaymentProvider Provider { get; set; }
        public string ProviderEventId { get; set; } = null!;
        public string PaymentIntentId { get; set; } = null!;
        
        public Guid PlayerUuid { get; set; }
        public Player Player { get; set; } = null!;
        
        public string Status { get; set; } = null!;
        
        // Use Dictionary<string, object> or a JsonElement for JSONB
        public string RawEvent { get; set; } = null!; 
        
        public DateTime ProcessedAt { get; set; }
    }
}
