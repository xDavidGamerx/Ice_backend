using System;
using System.Security.Cryptography;
using System.Text;

namespace IceBackend.IntegrationTests.Fixtures
{
    public static class StripeSignatureHelper
    {
        public static string GenerateSignature(string rawJson, string secret)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payload = $"{timestamp}.{rawJson}";
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLower();
            return $"t={timestamp},v1={signature}";
        }

        public static string BuildStripePayload(string customerId, string subscriptionId, string billingReason, string status, string? eventId = null)
        {
            eventId ??= Guid.NewGuid().ToString("N");
            var objectId = Guid.NewGuid().ToString("N");
            return $$"""
            {
                "id": "evt_test_{{eventId}}",
                "object": "event",
                "api_version": "2023-10-16",
                "type": "{{status}}",
                "data": {
                    "object": {
                        "id": "obj_test_{{objectId}}",
                        "object": "{{(status.StartsWith("invoice") ? "invoice" : "subscription")}}",
                        "customer": "{{customerId}}",
                        "subscription": "{{subscriptionId}}",
                        "billing_reason": "{{billingReason}}",
                        "payment_intent": "pi_test_{{Guid.NewGuid():N}}"
                    }
                },
                "livemode": false,
                "pending_webhooks": 1,
                "request": {
                    "id": "req_test_{{eventId}}",
                    "idempotency_key": "idemp_{{eventId}}"
                },
                "created": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}}
            }
            """;
        }
    }
}
