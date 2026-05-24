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

        public static string BuildStripePayload(string customerId, string subscriptionId, string billingReason, string status)
        {
            return $$"""
            {
                "id": "evt_test_{{Guid.NewGuid():N}}",
                "type": "{{status}}",
                "data": {
                    "object": {
                        "customer": "{{customerId}}",
                        "subscription": "{{subscriptionId}}",
                        "billing_reason": "{{billingReason}}",
                        "payment_intent": "pi_test_{{Guid.NewGuid():N}}"
                    }
                },
                "created": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}}
            }
            """;
        }
    }
}
